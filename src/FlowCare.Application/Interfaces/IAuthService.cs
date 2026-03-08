using FlowCare.Application.DTOs;

namespace FlowCare.Application.Interfaces;

public interface IAuthService
{
    Task<(LoginResponse? Result, string? Error, int StatusCode)> RegisterCustomerAsync(
        RegisterCustomerRequest request,
        Stream imageStream,
        string imageFileName,
        long imageLength);

    Task<(LoginResponse? Result, string? Error, int StatusCode)> LoginAsync(string userId);
}
