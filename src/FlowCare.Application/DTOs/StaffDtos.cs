using System.ComponentModel.DataAnnotations;

namespace FlowCare.Application.DTOs;

public record StaffResponse(
    string Id,
    string Username,
    string FullName,
    string Email,
    string? Phone,
    string? BranchId,
    string? BranchName,
    string Role,
    bool IsActive,
    List<string> AssignedServiceTypeIds);

public record AssignStaffServicesRequest(
    [property: Required]
    [property: MinLength(1)]
    List<string> ServiceTypeIds);
