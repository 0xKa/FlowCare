using FlowCare.Application.DTOs;

namespace FlowCare.Application.Interfaces;

public interface IStaffManagementService
{
    Task<PagedResponse<StaffResponse>> ListStaffAsync(
        string actorRole,
        string? actorBranchId,
        int page,
        int size,
        string? term);

    Task<(bool Success, string? Error)> AssignStaffToServicesAsync(
        string staffId, List<string> serviceTypeIds, string actorId, string actorRole, string? actorBranchId);

    Task<(bool Success, string? Error)> UnassignStaffFromServiceAsync(
        string staffId, string serviceTypeId, string actorId, string actorRole, string? actorBranchId);
}
