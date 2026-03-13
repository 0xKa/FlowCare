using FlowCare.Application.DTOs;
using FlowCare.Application.Interfaces;
using FlowCare.Domain.Entities;
using FlowCare.Domain.Enums;
using FlowCare.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace FlowCare.Infrastructure.Services;

public class CustomerService(FlowCareDbContext db, IFileStorageService fileStorage) : ICustomerService
{
    public async Task<PagedResponse<CustomerResponse>> ListCustomersAsync(int page, int size, string? term)
    {
        (page, size) = NormalizePage(page, size);

        var query = db.Users
            .AsNoTracking()
            .Where(u => u.Role == UserRole.Customer);

        if (!string.IsNullOrWhiteSpace(term))
        {
            var pattern = $"%{term.Trim()}%";
            query = query.Where(u =>
                EF.Functions.ILike(u.Id, pattern)
                || EF.Functions.ILike(u.Username, pattern)
                || EF.Functions.ILike(u.FullName, pattern)
                || EF.Functions.ILike(u.Email, pattern)
                || (u.Phone != null && EF.Functions.ILike(u.Phone, pattern)));
        }

        var total = await query.CountAsync();
        var customers = await query
            .OrderBy(u => u.FullName)
            .Skip((page - 1) * size)
            .Take(size)
            .ToListAsync();

        return new PagedResponse<CustomerResponse>(
            [.. customers.Select(MapToResponse)],
            total);
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

    private static (int Page, int Size) NormalizePage(int page, int size)
    {
        var normalizedPage = page < 1 ? 1 : page;
        var normalizedSize = size < 1 ? 20 : (size > 200 ? 200 : size);
        return (normalizedPage, normalizedSize);
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
