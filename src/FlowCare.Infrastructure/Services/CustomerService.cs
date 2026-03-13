using FlowCare.Application.DTOs;
using FlowCare.Application.Interfaces;
using FlowCare.Domain.Entities;
using FlowCare.Domain.Enums;
using FlowCare.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace FlowCare.Infrastructure.Services;

public class CustomerService(FlowCareDbContext db, IFileStorageService fileStorage) : ICustomerService
{
    public async Task<List<CustomerResponse>> ListCustomersAsync()
    {
        var customers = await db.Users
            .AsNoTracking()
            .Where(u => u.Role == UserRole.Customer)
            .OrderBy(u => u.FullName)
            .ToListAsync();

        return [.. customers.Select(MapToResponse)];
    }

    public async Task<CustomerResponse?> GetCustomerByIdAsync(string customerId)
    {
        var customer = await db.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == customerId && u.Role == UserRole.Customer);

        return customer is null ? null : MapToResponse(customer);
    }

    public async Task<(Stream Stream, string ContentType, string FileName)?> GetCustomerIdImageAsync(string customerId)
    {
        var customer = await db.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == customerId && u.Role == UserRole.Customer);

        if (customer?.IdImagePath is null)
            return null;

        var fileInfo = fileStorage.GetFile(customer.IdImagePath);
        if (fileInfo is null)
            return null;

        var (fullPath, contentType) = fileInfo.Value;
        var stream = new FileStream(
            fullPath,
            FileMode.Open,
            FileAccess.Read,
            FileShare.Read,
            bufferSize: 4096,
            options: FileOptions.Asynchronous);

        return (stream, contentType, Path.GetFileName(fullPath));
    }

    private static CustomerResponse MapToResponse(User u) => new(
        u.Id,
        u.Username,
        u.FullName,
        u.Email,
        u.Phone,
        u.IsActive,
        u.IdImagePath is not null,
        u.CreatedAt);
}
