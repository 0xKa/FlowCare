using FlowCare.Application.DTOs;

namespace FlowCare.Application.Interfaces;

public interface IBranchService
{
    Task<PagedResponse<BranchResponse>> ListBranchesAsync(int page, int size, string? term);
    Task<PagedResponse<ServiceTypeResponse>?> ListServicesAsync(string branchId, int page, int size, string? term);
    Task<PagedResponse<SlotResponse>?> ListAvailableSlotsAsync(
        string branchId,
        string serviceTypeId,
        DateOnly? date,
        int page,
        int size,
        string? term);
}
