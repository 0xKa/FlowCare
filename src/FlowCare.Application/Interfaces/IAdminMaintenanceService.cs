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
}
