using System.ComponentModel.DataAnnotations;

namespace FlowCare.Api.CustomWebModels;

public class RegisterCustomerFormRequest
{
    [Required]
    [StringLength(100, MinimumLength = 3)]
    public required string Username { get; init; }

    [Required]
    [StringLength(100, MinimumLength = 6)]
    public required string Password { get; init; }

    [Required]
    [StringLength(200, MinimumLength = 2)]
    public required string FullName { get; init; }

    [Required]
    [EmailAddress]
    [StringLength(200)]
    public required string Email { get; init; }

    [Phone]
    [StringLength(20)]
    public string? Phone { get; init; }

    [Required]
    public required IFormFile IdImage { get; init; }
}
