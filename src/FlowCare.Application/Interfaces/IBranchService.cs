using FlowCare.Application.DTOs;

namespace FlowCare.Application.Interfaces;

public interface IBranchService
{
    Task<List<BranchResponse>> ListBranchesAsync();
    Task<List<ServiceTypeResponse>?> ListServicesAsync(string branchId);
    Task<List<SlotResponse>?> ListAvailableSlotsAsync(string branchId, string serviceTypeId, DateOnly? date);
}
