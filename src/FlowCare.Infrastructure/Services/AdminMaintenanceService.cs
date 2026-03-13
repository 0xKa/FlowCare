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
        var threshold = DateTimeOffset.UtcNow.AddDays(-retentionDays);

        var slotsToDelete = await db.Slots
            .IgnoreQueryFilters()
            .Where(s => s.DeletedAt != null && s.DeletedAt <= threshold)
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

    private async Task<int> GetRetentionDaysAsync()
    {
        var setting = await db.SystemSettings.FindAsync(RetentionSettingKey);
        if (setting is null)
            return 30;

        return int.TryParse(setting.Value, out var days) && days >= 0
            ? days
            : 30;
    }
}
