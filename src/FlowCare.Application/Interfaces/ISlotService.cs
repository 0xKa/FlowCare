using FlowCare.Application.DTOs;

namespace FlowCare.Application.Interfaces;

public interface ISlotService
{
    Task<(SlotDetailResponse? Result, string? Error)> CreateSlotAsync(
        string branchId, CreateSlotRequest request, string actorId, string actorRole);

    Task<(List<SlotDetailResponse>? Results, string? Error)> CreateBulkSlotsAsync(
        string branchId, List<CreateSlotRequest> requests, string actorId, string actorRole);

    Task<(SlotDetailResponse? Result, string? Error)> UpdateSlotAsync(
        string slotId, UpdateSlotRequest request, string actorId, string actorRole, string? actorBranchId);

    Task<(bool Success, string? Error)> SoftDeleteSlotAsync(
        string slotId, string actorId, string actorRole, string? actorBranchId);

    Task<List<SlotDetailResponse>> ListSlotsAsync(
        string branchId, string actorRole, string? actorBranchId, bool includeDeleted);
}
