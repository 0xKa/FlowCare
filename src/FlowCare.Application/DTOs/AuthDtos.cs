using System.ComponentModel.DataAnnotations;

namespace FlowCare.Application.DTOs;

public record RegisterCustomerRequest(
    [property: Required]
    [property: StringLength(100, MinimumLength = 3)]
    string Username,
    [property: Required]
    [property: StringLength(100, MinimumLength = 6)]
    string Password,
    [property: Required]
    [property: StringLength(200, MinimumLength = 2)]
    string FullName,
    [property: Required]
    [property: EmailAddress]
    [property: StringLength(200)]
    string Email,
    [property: Phone]
    [property: StringLength(20)]
    string? Phone);

public record LoginResponse(
    string Id,
    string Username,
    string Role,
    string FullName,
    string Email,
    string? Phone,
    string? BranchId);
