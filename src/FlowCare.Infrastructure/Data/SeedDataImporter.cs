using System.Text.Json;
using FlowCare.Application.Interfaces;
using FlowCare.Domain.Entities;
using FlowCare.Domain.Enums;
using FlowCare.Infrastructure.Data.Seed;
using Microsoft.EntityFrameworkCore;

namespace FlowCare.Infrastructure.Data;

public class SeedDataImporter(FlowCareDbContext db, IAuditLogService auditLog)
{
    public async Task ImportAsync(string filePath)
    {
        var json = await File.ReadAllTextAsync(filePath);
        var seedData = JsonSerializer.Deserialize<SeedData>(json)
            ?? throw new InvalidOperationException("Failed to deserialize seed data.");

        await SeedBranchesAsync(seedData.Branches);
        await SeedUsersAsync(seedData.Users);
        await SeedServiceTypesAsync(seedData.ServiceTypes);
        await SeedStaffServiceTypesAsync(seedData.StaffServiceTypes);
        await SeedSlotsAsync(seedData.Slots);
        await SeedAppointmentsAsync(seedData.Appointments);
        await SeedAuditLogsAsync(seedData.AuditLogs);
        await SeedSystemSettingsAsync();
        await auditLog.LogAsync(
            actorId: "system",
            actorRole: "SYSTEM",
            actionType: AuditActionType.SeedImported,
            entityType: AuditEntityType.Seed,
            entityId: Path.GetFileName(filePath),
            metadata: new
            {
                message = "Initial seed import executed",
                file_name = Path.GetFileName(filePath)
            });

        await db.SaveChangesAsync();
    }

    private async Task SeedBranchesAsync(List<SeedBranch> branches)
    {
        foreach (var b in branches)
        {
            if (await db.Branches.AnyAsync(x => x.Id == b.Id)) continue;

            db.Branches.Add(new Branch
            {
                Id = b.Id,
                Name = b.Name,
                City = b.City,
                Address = b.Address,
                Timezone = b.Timezone,
                IsActive = b.IsActive
            });
        }
        await db.SaveChangesAsync();
    }

    private async Task SeedUsersAsync(SeedUsers users)
    {
        var allUsers = users.Admin
            .Concat(users.BranchManagers)
            .Concat(users.Staff)
            .Concat(users.Customers);

        foreach (var u in allUsers)
        {
            if (await db.Users.AnyAsync(x => x.Id == u.Id)) continue;

            db.Users.Add(new User
            {
                Id = u.Id,
                Username = u.Username,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(u.Password),
                Role = ParseRole(u.Role),
                FullName = u.FullName,
                Email = u.Email,
                Phone = u.Phone,
                BranchId = u.BranchId,
                IsActive = u.IsActive
            });
        }
        await db.SaveChangesAsync();
    }

    private async Task SeedServiceTypesAsync(List<SeedServiceType> serviceTypes)
    {
        foreach (var st in serviceTypes)
        {
            if (await db.ServiceTypes.AnyAsync(x => x.Id == st.Id)) continue;

            db.ServiceTypes.Add(new ServiceType
            {
                Id = st.Id,
                BranchId = st.BranchId,
                Name = st.Name,
                Description = st.Description,
                DurationMinutes = st.DurationMinutes,
                IsActive = st.IsActive
            });
        }
        await db.SaveChangesAsync();
    }

    private async Task SeedStaffServiceTypesAsync(List<SeedStaffServiceType> staffServiceTypes)
    {
        foreach (var sst in staffServiceTypes)
        {
            var exists = await db.StaffServiceTypes
                .AnyAsync(x => x.StaffId == sst.StaffId && x.ServiceTypeId == sst.ServiceTypeId);
            if (exists) continue;

            db.StaffServiceTypes.Add(new StaffServiceType
            {
                StaffId = sst.StaffId,
                ServiceTypeId = sst.ServiceTypeId
            });
        }
        await db.SaveChangesAsync();
    }

    private async Task SeedSlotsAsync(List<SeedSlot> slots)
    {
        foreach (var s in slots)
        {
            // IgnoreQueryFilters to find even soft-deleted slots
            if (await db.Slots.IgnoreQueryFilters().AnyAsync(x => x.Id == s.Id)) continue;

            db.Slots.Add(new Slot
            {
                Id = s.Id,
                BranchId = s.BranchId,
                ServiceTypeId = s.ServiceTypeId,
                StaffId = s.StaffId,
                StartAt = s.StartAt.ToUniversalTime(),
                EndAt = s.EndAt.ToUniversalTime(),
                Capacity = s.Capacity,
                IsActive = s.IsActive
            });
        }
        await db.SaveChangesAsync();
    }

    private async Task SeedAppointmentsAsync(List<SeedAppointment> appointments)
    {
        foreach (var a in appointments)
        {
            if (await db.Appointments.AnyAsync(x => x.Id == a.Id)) continue;

            db.Appointments.Add(new Appointment
            {
                Id = a.Id,
                CustomerId = a.CustomerId,
                BranchId = a.BranchId,
                ServiceTypeId = a.ServiceTypeId,
                SlotId = a.SlotId,
                StaffId = a.StaffId,
                Status = ParseStatus(a.Status),
                CreatedAt = a.CreatedAt.ToUniversalTime()
            });
        }
        await db.SaveChangesAsync();
    }

    private async Task SeedAuditLogsAsync(List<SeedAuditLog> auditLogs)
    {
        foreach (var al in auditLogs)
        {
            if (await db.AuditLogs.AnyAsync(x => x.Id == al.Id)) continue;

            db.AuditLogs.Add(new AuditLog
            {
                Id = al.Id,
                ActorId = al.ActorId,
                ActorRole = al.ActorRole,
                ActionType = al.ActionType,
                EntityType = al.EntityType,
                EntityId = al.EntityId,
                Timestamp = al.Timestamp.ToUniversalTime(),
                Metadata = al.Metadata != null ? JsonSerializer.Serialize(al.Metadata) : null
            });
        }
        await db.SaveChangesAsync();
    }

    private async Task SeedSystemSettingsAsync()
    {
        if (await db.SystemSettings.FindAsync("SoftDeleteRetentionDays") == null)
            db.SystemSettings.Add(new SystemSetting { Key = "SoftDeleteRetentionDays", Value = "30" });

        if (await db.SystemSettings.FindAsync("CleanupWorkerEnabled") == null)
            db.SystemSettings.Add(new SystemSetting { Key = "CleanupWorkerEnabled", Value = "true" });

        if (await db.SystemSettings.FindAsync("CustomerBookingsPerDay") == null)
            db.SystemSettings.Add(new SystemSetting { Key = "CustomerBookingsPerDay", Value = "3" });

        if (await db.SystemSettings.FindAsync("MaxReschedulesPerAppointment") == null)
            db.SystemSettings.Add(new SystemSetting { Key = "MaxReschedulesPerAppointment", Value = "2" });
    }

    private static UserRole ParseRole(string role) => role switch
    {
        "ADMIN" => UserRole.Admin,
        "BRANCH_MANAGER" => UserRole.BranchManager,
        "STAFF" => UserRole.Staff,
        "CUSTOMER" => UserRole.Customer,
        _ => throw new ArgumentException($"Unknown role: {role}")
    };

    private static AppointmentStatus ParseStatus(string status) => status switch
    {
        "BOOKED" => AppointmentStatus.Booked,
        "CHECKED_IN" => AppointmentStatus.CheckedIn,
        "NO_SHOW" => AppointmentStatus.NoShow,
        "COMPLETED" => AppointmentStatus.Completed,
        "CANCELLED" => AppointmentStatus.Cancelled,
        "RESCHEDULED" => AppointmentStatus.Rescheduled,
        _ => throw new ArgumentException($"Unknown appointment status: {status}")
    };
}
