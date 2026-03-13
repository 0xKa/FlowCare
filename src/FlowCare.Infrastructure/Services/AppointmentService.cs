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
    private const string CustomerBookingsPerDayKey = "CustomerBookingsPerDay";
    private const string MaxReschedulesPerAppointmentKey = "MaxReschedulesPerAppointment";
    private const int DefaultCustomerBookingsPerDay = 3;
    private const int DefaultMaxReschedulesPerAppointment = 2;

    public async Task<(AppointmentResponse? Result, string? Error)> BookAsync(
        string customerId, BookAppointmentRequest request, Stream? attachment, string? attachmentFileName)
    {
        var dailyBookingLimit = await GetIntSettingAsync(
            CustomerBookingsPerDayKey,
            DefaultCustomerBookingsPerDay,
            minValue: 1);

        var startOfToday = DateTimeOffset.UtcNow.Date;
        var startOfTomorrow = startOfToday.AddDays(1);
        var todayBookingsCount = await db.Appointments
            .AsNoTracking()
            .CountAsync(a => a.CustomerId == customerId
                && a.CreatedAt >= startOfToday
                && a.CreatedAt < startOfTomorrow);

        if (todayBookingsCount >= dailyBookingLimit)
            return (null, $"Daily booking limit reached. Max {dailyBookingLimit} bookings per day.");

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

    public async Task<PagedResponse<AppointmentResponse>> ListByCustomerAsync(
        string customerId,
        int page,
        int size,
        string? term)
    {
        (page, size) = NormalizePage(page, size);

        var query = db.Appointments
            .AsNoTracking()
            .Include(a => a.Branch)
            .Include(a => a.ServiceType)
            .Include(a => a.Staff)
            .Where(a => a.CustomerId == customerId);

        if (!string.IsNullOrWhiteSpace(term))
        {
            var pattern = $"%{term.Trim()}%";
            query = query.Where(a =>
                EF.Functions.ILike(a.Id, pattern)
                || EF.Functions.ILike(a.Status.ToString(), pattern)
                || (a.Branch != null && EF.Functions.ILike(a.Branch.Name, pattern))
                || (a.ServiceType != null && EF.Functions.ILike(a.ServiceType.Name, pattern))
                || (a.Staff != null && EF.Functions.ILike(a.Staff.FullName, pattern)));
        }

        var total = await query.CountAsync();
        var appointments = await query
            .OrderByDescending(a => a.CreatedAt)
            .Skip((page - 1) * size)
            .Take(size)
            .ToListAsync();

        return PagedResponse<AppointmentResponse>.Create(
            [.. appointments.Select(MapToResponse)],
            total,
            page,
            size);
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
        var maxReschedulesPerAppointment = await GetIntSettingAsync(
            MaxReschedulesPerAppointmentKey,
            DefaultMaxReschedulesPerAppointment,
            minValue: 0);

        var currentReschedulesCount = await db.AuditLogs
            .AsNoTracking()
            .CountAsync(a => a.ActionType == "APPOINTMENT_RESCHEDULED" && a.EntityId == appointmentId);

        if (currentReschedulesCount >= maxReschedulesPerAppointment)
            return (null, $"Reschedule limit reached. Max {maxReschedulesPerAppointment} reschedules per appointment.");

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

    public async Task<PagedResponse<AppointmentResponse>> ListAsync(
        string role,
        string userId,
        string? branchId,
        int page,
        int size,
        string? term)
    {
        (page, size) = NormalizePage(page, size);

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

        if (!string.IsNullOrWhiteSpace(term))
        {
            var pattern = $"%{term.Trim()}%";
            query = query.Where(a =>
                EF.Functions.ILike(a.Id, pattern)
                || EF.Functions.ILike(a.Status.ToString(), pattern)
                || (a.Customer != null && EF.Functions.ILike(a.Customer.FullName, pattern))
                || (a.Branch != null && EF.Functions.ILike(a.Branch.Name, pattern))
                || (a.ServiceType != null && EF.Functions.ILike(a.ServiceType.Name, pattern))
                || (a.Staff != null && EF.Functions.ILike(a.Staff.FullName, pattern)));
        }

        var total = await query.CountAsync();

        var appointments = await query
            .OrderByDescending(a => a.CreatedAt)
            .Skip((page - 1) * size)
            .Take(size)
            .ToListAsync();

        return PagedResponse<AppointmentResponse>.Create(
            [.. appointments.Select(MapToResponse)],
            total,
            page,
            size);
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

    private static (int Page, int Size) NormalizePage(int page, int size)
    {
        var normalizedPage = page < 1 ? 1 : page;
        var normalizedSize = size < 1 ? 20 : (size > 200 ? 200 : size);
        return (normalizedPage, normalizedSize);
    }

    private async Task<int> GetIntSettingAsync(string key, int defaultValue, int minValue)
    {
        var setting = await db.SystemSettings.FindAsync(key);
        if (setting is null)
            return defaultValue;

        if (!int.TryParse(setting.Value, out var parsed))
            return defaultValue;

        return parsed < minValue ? defaultValue : parsed;
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
