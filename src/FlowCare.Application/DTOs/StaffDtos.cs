using System.ComponentModel.DataAnnotations;

namespace FlowCare.Application.DTOs;

public record StaffResponse
{
    public string Id { get; init; } = null!;
    public string Username { get; init; } = null!;
    public string FullName { get; init; } = null!;
    public string Email { get; init; } = null!;
    public string? Phone { get; init; }
    public string? BranchId { get; init; }
    public string? BranchName { get; init; }
    public string Role { get; init; } = null!;
    public bool IsActive { get; init; }
    public List<string> AssignedServiceTypeIds { get; init; }

    public StaffResponse(
        string id,
        string username,
        string fullName,
        string email,
        string? phone,
        string? branchId,
        string? branchName,
        string role,
        bool isActive,
        List<string> assignedServiceTypeIds)
    {
        Id = id;
        Username = username;
        FullName = fullName;
        Email = email;
        Phone = phone;
        BranchId = branchId;
        BranchName = branchName;
        Role = role;
        IsActive = isActive;
        AssignedServiceTypeIds = assignedServiceTypeIds;
    }
}

public record AssignStaffServicesRequest
{
    [Required]
    [MinLength(1)]
    public List<string> ServiceTypeIds { get; init; }

    public AssignStaffServicesRequest(List<string> serviceTypeIds)
    {
        ServiceTypeIds = serviceTypeIds;
    }
}
