namespace FlowCare.Api.CustomWebModels;

public class RegisterCustomerFormRequest
{
    public required string Username { get; init; }
    public required string Password { get; init; }
    public required string FullName { get; init; }
    public required string Email { get; init; }
    public string? Phone { get; init; }
    public required IFormFile IdImage { get; init; }
}
