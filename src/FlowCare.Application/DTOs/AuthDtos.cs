using System.ComponentModel.DataAnnotations;

namespace FlowCare.Application.DTOs;

public record RegisterCustomerRequest
{
    [Required]
    [StringLength(100, MinimumLength = 3)]
    public string Username { get; init; } = null!;

    [Required]
    [StringLength(100, MinimumLength = 6)]
    public string Password { get; init; } = null!;

    [Required]
    [StringLength(200, MinimumLength = 2)]
    public string FullName { get; init; } = null!;

    [Required]
    [EmailAddress]
    [StringLength(200)]
    public string Email { get; init; } = null!;

    [Phone]
    [StringLength(20)]
    public string? Phone { get; init; }

    public RegisterCustomerRequest(
        string username,
        string password,
        string fullName,
        string email,
        string? phone)
    {
        Username = username;
        Password = password;
        FullName = fullName;
        Email = email;
        Phone = phone;
    }
}

public record LoginResponse
{
    public string Id { get; init; } = null!;
    public string Username { get; init; } = null!;
    public string Role { get; init; } = null!;
    public string FullName { get; init; } = null!;
    public string Email { get; init; } = null!;
    public string? Phone { get; init; }
    public string? BranchId { get; init; }

    public LoginResponse(
        string id,
        string username,
        string role,
        string fullName,
        string email,
        string? phone,
        string? branchId)
    {
        Id = id;
        Username = username;
        Role = role;
        FullName = fullName;
        Email = email;
        Phone = phone;
        BranchId = branchId;
    }
}
