namespace FlowCare.Application.DTOs;

public record RegisterCustomerRequest(
    string Username,
    string Password,
    string FullName,
    string Email,
    string? Phone);

public record LoginResponse(
    string Id,
    string Username,
    string Role,
    string FullName,
    string Email,
    string? Phone,
    string? BranchId);
