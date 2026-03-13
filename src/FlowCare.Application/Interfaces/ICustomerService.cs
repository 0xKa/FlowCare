using FlowCare.Application.DTOs;

namespace FlowCare.Application.Interfaces;

public interface ICustomerService
{
    Task<List<CustomerResponse>> ListCustomersAsync();
    Task<CustomerResponse?> GetCustomerByIdAsync(string customerId);
    Task<(Stream Stream, string ContentType, string FileName)?> GetCustomerIdImageAsync(string customerId);
}
