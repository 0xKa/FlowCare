using FlowCare.Application.DTOs;

namespace FlowCare.Application.Interfaces;

public interface ICustomerService
{
    Task<PagedResponse<CustomerResponse>> ListCustomersAsync(int page, int size, string? term);
    Task<CustomerResponse?> GetCustomerByIdAsync(string customerId);
    Task<(Stream Stream, string ContentType, string FileName)?> GetCustomerIdImageAsync(string customerId);
}
