using FlowCare.Application.DTOs;
using FlowCare.Application.Interfaces;
using FlowCare.Domain.Entities;
using FlowCare.Domain.Enums;
using FlowCare.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace FlowCare.Infrastructure.Services;

public class AppointmentService(
    FlowCareDbContext db,
    IAuditLogService auditLog,
    IFileStorageService fileStorage) : IAppointmentService
{
    private const long MaxAttachmentSize = 5 * 1024 * 1024; // 5 MB

    public async Task<(AppointmentResponse? Result, string? Error)> BookAsync(
        string customerId, BookAppointmentRequest request, Stream? attachment, string? attachmentFileName)
    {
        // Validate slot exists and is available
        var slot = await db.Slots
            .Include(s => s.Appointments)
            .FirstOrDefaultAsync(s => s.Id == request.SlotId
                && s.BranchId == request.BranchId
                && s.ServiceTypeId == request.ServiceTypeId
                && s.IsActive);

        if (slot is null)
            return (null, "Slot not found or not available.");

        var bookedCount = slot.Appointments.Count(a =>
            a.Status != AppointmentStatus.Cancelled &&
            a.Status != AppointmentStatus.Rescheduled);

        if (bookedCount >= slot.Capacity)
            return (null, "Slot is fully booked.");

        // Save optional attachment
        string? attachmentPath = null;
        if (attachment is not null && attachmentFileName is not null)
        {
            if (!fileStorage.IsValidAttachment(attachment))
                return (null, "Attachment must be a valid JPEG, PNG, or PDF file.");

            attachment.Position = 0;
            var ext = Path.GetExtension(attachmentFileName).ToLowerInvariant();
            if (string.IsNullOrEmpty(ext)) ext = ".bin";
            var appointmentId = $"appt_{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}";
            attachmentPath = await fileStorage.SaveFileAsync(
                attachment, "attachments", $"{appointmentId}{ext}");

            var appointment = new Appointment
            {
                Id = appointmentId,
                CustomerId = customerId,
                BranchId = request.BranchId,
                ServiceTypeId = request.ServiceTypeId,
                SlotId = request.SlotId,
                StaffId = slot.StaffId,
                Status = AppointmentStatus.Booked,
                AttachmentPath = attachmentPath
            };
            db.Appointments.Add(appointment);
            await db.SaveChangesAsync();

            await auditLog.LogAsync(customerId, nameof(UserRole.Customer),
                "APPOINTMENT_BOOKED", "APPOINTMENT", appointment.Id,
                new { slot_id = request.SlotId, branch_id = request.BranchId, service_type_id = request.ServiceTypeId });

            return (await MapToResponseAsync(appointment), null);
        }
        else
        {
            var appointmentId = $"appt_{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}";
            var appointment = new Appointment
            {
                Id = appointmentId,
                CustomerId = customerId,
                BranchId = request.BranchId,
                ServiceTypeId = request.ServiceTypeId,
                SlotId = request.SlotId,
                StaffId = slot.StaffId,
                Status = AppointmentStatus.Booked
            };
            db.Appointments.Add(appointment);
            await db.SaveChangesAsync();

            await auditLog.LogAsync(customerId, nameof(UserRole.Customer),
                "APPOINTMENT_BOOKED", "APPOINTMENT", appointment.Id,
                new { slot_id = request.SlotId, branch_id = request.BranchId, service_type_id = request.ServiceTypeId });

            return (await MapToResponseAsync(appointment), null);
        }
    }

    public async Task<List<AppointmentResponse>> ListByCustomerAsync(string customerId)
    {
        var appointments = await db.Appointments
            .AsNoTracking()
            .Include(a => a.Branch)
            .Include(a => a.ServiceType)
            .Include(a => a.Staff)
            .Where(a => a.CustomerId == customerId)
            .OrderByDescending(a => a.CreatedAt)
            .ToListAsync();

        return appointments.Select(MapToResponse).ToList();
    }

    public async Task<AppointmentResponse?> GetByIdForCustomerAsync(string appointmentId, string customerId)
    {
        var appointment = await db.Appointments
            .AsNoTracking()
            .Include(a => a.Branch)
            .Include(a => a.ServiceType)
            .Include(a => a.Staff)
            .Include(a => a.Customer)
            .FirstOrDefaultAsync(a => a.Id == appointmentId && a.CustomerId == customerId);

        return appointment is null ? null : MapToResponse(appointment);
    }

    public async Task<(bool Success, string? Error)> CancelAsync(
        string appointmentId, string customerId, string actorRole)
    {
        var appointment = await db.Appointments
            .FirstOrDefaultAsync(a => a.Id == appointmentId && a.CustomerId == customerId);

        if (appointment is null)
            return (false, "Appointment not found.");

        if (appointment.Status is AppointmentStatus.Cancelled or AppointmentStatus.Completed)
            return (false, $"Cannot cancel an appointment with status '{appointment.Status}'.");

        appointment.Status = AppointmentStatus.Cancelled;
        await db.SaveChangesAsync();

        await auditLog.LogAsync(customerId, actorRole,
            "APPOINTMENT_CANCELLED", "APPOINTMENT", appointmentId,
            new { previous_status = appointment.Status.ToString() });

        return (true, null);
    }

    public async Task<(AppointmentResponse? Result, string? Error)> RescheduleAsync(
        string appointmentId, string customerId, string newSlotId, string actorRole)
    {
        var appointment = await db.Appointments
            .FirstOrDefaultAsync(a => a.Id == appointmentId && a.CustomerId == customerId);

        if (appointment is null)
            return (null, "Appointment not found.");

        if (appointment.Status is AppointmentStatus.Cancelled or AppointmentStatus.Completed)
            return (null, $"Cannot reschedule an appointment with status '{appointment.Status}'.");

        // Validate new slot
        var newSlot = await db.Slots
            .Include(s => s.Appointments)
            .FirstOrDefaultAsync(s => s.Id == newSlotId
                && s.BranchId == appointment.BranchId
                && s.ServiceTypeId == appointment.ServiceTypeId
                && s.IsActive);

        if (newSlot is null)
            return (null, "New slot not found or not available for this branch/service.");

        var bookedCount = newSlot.Appointments.Count(a =>
            a.Status != AppointmentStatus.Cancelled &&
            a.Status != AppointmentStatus.Rescheduled);

        if (bookedCount >= newSlot.Capacity)
            return (null, "New slot is fully booked.");

        var oldSlotId = appointment.SlotId;
        appointment.SlotId = newSlotId;
        appointment.StaffId = newSlot.StaffId;
        appointment.Status = AppointmentStatus.Booked;
        await db.SaveChangesAsync();

        await auditLog.LogAsync(customerId, actorRole,
            "APPOINTMENT_RESCHEDULED", "APPOINTMENT", appointmentId,
            new { old_slot_id = oldSlotId, new_slot_id = newSlotId });

        return (await MapToResponseAsync(appointment), null);
    }

    // --- Staff / Manager / Admin ---

    public async Task<List<AppointmentResponse>> ListAsync(string role, string userId, string? branchId)
    {
        var query = db.Appointments
            .AsNoTracking()
            .Include(a => a.Customer)
            .Include(a => a.Branch)
            .Include(a => a.ServiceType)
            .Include(a => a.Staff)
            .AsQueryable();

        if (role == nameof(UserRole.Staff))
        {
            // Staff sees only appointments assigned to them
            query = query.Where(a => a.StaffId == userId);
        }
        else if (role == nameof(UserRole.BranchManager))
        {
            // Manager sees only their branch
            query = query.Where(a => a.BranchId == branchId);
        }
        // else Admin sees all

        var appointments = await query
            .OrderByDescending(a => a.CreatedAt)
            .ToListAsync();

        return appointments.Select(MapToResponse).ToList();
    }

    public async Task<AppointmentResponse?> GetByIdAsync(string appointmentId)
    {
        var appointment = await db.Appointments
            .AsNoTracking()
            .Include(a => a.Customer)
            .Include(a => a.Branch)
            .Include(a => a.ServiceType)
            .Include(a => a.Staff)
            .FirstOrDefaultAsync(a => a.Id == appointmentId);

        return appointment is null ? null : MapToResponse(appointment);
    }

    public async Task<(bool Success, string? Error)> UpdateStatusAsync(
        string appointmentId, string newStatus, string actorId, string actorRole, string? actorBranchId)
    {
        if (!Enum.TryParse<AppointmentStatus>(newStatus, true, out var parsedStatus))
            return (false, $"Invalid status: '{newStatus}'. Allowed: CheckedIn, NoShow, Completed.");

        if (parsedStatus is not (AppointmentStatus.CheckedIn or AppointmentStatus.NoShow or AppointmentStatus.Completed))
            return (false, "Only CheckedIn, NoShow, or Completed statuses are allowed.");

        var appointment = await db.Appointments.FirstOrDefaultAsync(a => a.Id == appointmentId);
        if (appointment is null)
            return (false, "Appointment not found.");

        // Branch scoping for non-admin
        if (actorRole == nameof(UserRole.BranchManager) && appointment.BranchId != actorBranchId)
            return (false, "Cannot update appointments outside your branch.");

        if (actorRole == nameof(UserRole.Staff) && appointment.StaffId != actorId)
            return (false, "Cannot update appointments not assigned to you.");

        var previousStatus = appointment.Status.ToString();
        appointment.Status = parsedStatus;
        await db.SaveChangesAsync();

        await auditLog.LogAsync(actorId, actorRole,
            "APPOINTMENT_STATUS_UPDATED", "APPOINTMENT", appointmentId,
            new { previous_status = previousStatus, new_status = newStatus });

        return (true, null);
    }

    // --- Mapping helpers ---

    private async Task<AppointmentResponse> MapToResponseAsync(Appointment appointment)
    {
        // Reload with all includes
        var loaded = await db.Appointments
            .AsNoTracking()
            .Include(a => a.Customer)
            .Include(a => a.Branch)
            .Include(a => a.ServiceType)
            .Include(a => a.Staff)
            .FirstAsync(a => a.Id == appointment.Id);

        return MapToResponse(loaded);
    }

    private static AppointmentResponse MapToResponse(Appointment a) => new(
        a.Id,
        a.CustomerId,
        a.Customer?.FullName,
        a.BranchId,
        a.Branch?.Name,
        a.ServiceTypeId,
        a.ServiceType?.Name,
        a.SlotId,
        a.StaffId,
        a.Staff?.FullName,
        a.Status.ToString(),
        a.AttachmentPath is not null,
        a.Notes,
        a.CreatedAt,
        a.UpdatedAt);
}
