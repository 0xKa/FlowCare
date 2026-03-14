using FlowCare.Application.DTOs;

namespace FlowCare.Application.Interfaces;

public interface IAdminMaintenanceService
{
    Task<SoftDeleteSettingsResponse> SetRetentionDaysAsync(
        int days,
        string actorId,
        string actorRole);

    Task<CleanupResultResponse> CleanupSoftDeletedSlotsAsync(
        string actorId,
        string actorRole);

    Task<RateLimitSettingsResponse> SetRateLimitsAsync(
        int customerBookingsPerDay,
        int maxReschedulesPerAppointment,
        string actorId,
        string actorRole);

    Task<CleanupWorkerSettingsResponse> SetCleanupWorkerEnabledAsync(
        bool enabled,
        string actorId,
        string actorRole);
}
