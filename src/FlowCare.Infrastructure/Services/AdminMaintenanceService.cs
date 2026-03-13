using FlowCare.Application.DTOs;
using FlowCare.Application.Interfaces;
using FlowCare.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace FlowCare.Infrastructure.Services;

public class AdminMaintenanceService(
    FlowCareDbContext db,
    IAuditLogService auditLog) : IAdminMaintenanceService
{
    private const string RetentionSettingKey = "SoftDeleteRetentionDays";
    private const string CustomerBookingsPerDayKey = "CustomerBookingsPerDay";
    private const string MaxReschedulesPerAppointmentKey = "MaxReschedulesPerAppointment";

    public async Task<SoftDeleteSettingsResponse> SetRetentionDaysAsync(
        int days,
        string actorId,
        string actorRole)
    {
        var setting = await db.SystemSettings.FindAsync(RetentionSettingKey);
        if (setting is null)
        {
            setting = new Domain.Entities.SystemSetting
            {
                Key = RetentionSettingKey,
                Value = days.ToString()
            };
            db.SystemSettings.Add(setting);
        }
        else
            setting.Value = days.ToString();

        await db.SaveChangesAsync();

        await auditLog.LogAsync(actorId, actorRole,
            "RETENTION_PERIOD_UPDATED", "SYSTEM_SETTING", RetentionSettingKey,
            new { retention_days = days });

        return new SoftDeleteSettingsResponse(days);
    }

    public async Task<CleanupResultResponse> CleanupSoftDeletedSlotsAsync(
        string actorId,
        string actorRole)
    {
        var retentionDays = await GetRetentionDaysAsync();
        // Treating retention as whole calendar days rather than exact timestamps. This prevents eligible rows from being skipped due to time-of-day differences.
        var utcToday = DateTimeOffset.UtcNow.Date;
        var thresholdExclusive = new DateTimeOffset(utcToday.AddDays(-retentionDays + 1), TimeSpan.Zero);

        var slotsToDelete = await db.Slots
            .IgnoreQueryFilters()
            .Where(s => s.DeletedAt != null && s.DeletedAt < thresholdExclusive) // Only hard delete slots that were soft-deleted before the threshold (retation period has fully passed)
            .ToListAsync();

        if (slotsToDelete.Count == 0)
            return new CleanupResultResponse(0, 0);

        var slotIds = slotsToDelete.Select(s => s.Id).ToList();

        var appointments = await db.Appointments
            .Where(a => a.SlotId != null && slotIds.Contains(a.SlotId))
            .ToListAsync();

        foreach (var appointment in appointments)
            appointment.SlotId = null;

        db.Slots.RemoveRange(slotsToDelete);
        await db.SaveChangesAsync();

        foreach (var slot in slotsToDelete)
        {
            await auditLog.LogAsync(actorId, actorRole,
                "HARD_DELETE", "SLOT", slot.Id,
                new
                {
                    deleted_at = slot.DeletedAt,
                    retention_days = retentionDays
                });
        }

        return new CleanupResultResponse(slotsToDelete.Count, appointments.Count);
    }

    public async Task<RateLimitSettingsResponse> SetRateLimitsAsync(
        int customerBookingsPerDay,
        int maxReschedulesPerAppointment,
        string actorId,
        string actorRole)
    {
        await UpsertSettingAsync(CustomerBookingsPerDayKey, customerBookingsPerDay.ToString());
        await UpsertSettingAsync(MaxReschedulesPerAppointmentKey, maxReschedulesPerAppointment.ToString());

        await db.SaveChangesAsync();

        await auditLog.LogAsync(
            actorId,
            actorRole,
            "RATE_LIMITS_UPDATED",
            "SYSTEM_SETTING",
            "RATE_LIMITS",
            new
            {
                customer_bookings_per_day = customerBookingsPerDay,
                max_reschedules_per_appointment = maxReschedulesPerAppointment
            });

        return new RateLimitSettingsResponse(customerBookingsPerDay, maxReschedulesPerAppointment);
    }

    private async Task<int> GetRetentionDaysAsync()
    {
        var setting = await db.SystemSettings.FindAsync(RetentionSettingKey);
        if (setting is null)
            return 30;

        return int.TryParse(setting.Value, out var days) && days >= 0
            ? days
            : 30;
    }

    private async Task UpsertSettingAsync(string key, string value)
    {
        var setting = await db.SystemSettings.FindAsync(key);
        if (setting is null)
        {
            db.SystemSettings.Add(new Domain.Entities.SystemSetting
            {
                Key = key,
                Value = value
            });
        }
        else
            setting.Value = value;
    }
}
