using FlowCare.Application.DTOs;
using FlowCare.Application.Interfaces;
using FlowCare.Domain.Enums;
using FlowCare.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace FlowCare.Infrastructure.Services;

public class BranchService(FlowCareDbContext db) : IBranchService
{
    public async Task<PagedResponse<BranchResponse>> ListBranchesAsync(int page, int size, string? term)
    {
        (page, size) = NormalizePage(page, size);

        var query = db.Branches
            .AsNoTracking()
            .Where(b => b.IsActive);

        if (!string.IsNullOrWhiteSpace(term))
        {
            var pattern = $"%{term.Trim()}%";
            query = query.Where(b =>
                EF.Functions.ILike(b.Name, pattern)
                || EF.Functions.ILike(b.City, pattern)
                || EF.Functions.ILike(b.Address, pattern)
                || EF.Functions.ILike(b.Timezone, pattern));
        }

        var total = await query.CountAsync();
        var results = await query
            .OrderBy(b => b.Name)
            .Skip((page - 1) * size)
            .Take(size)
            .Select(b => new BranchResponse(
                b.Id, b.Name, b.City, b.Address, b.Timezone, b.IsActive))
            .ToListAsync();

        return PagedResponse<BranchResponse>.Create(results, total, page, size);
    }

    /// <returns>null if the branch does not exist.</returns>
    public async Task<PagedResponse<ServiceTypeResponse>?> ListServicesAsync(
        string branchId,
        int page,
        int size,
        string? term)
    {
        (page, size) = NormalizePage(page, size);

        if (!await db.Branches.AnyAsync(b => b.Id == branchId && b.IsActive))
            return null;

        var query = db.ServiceTypes
            .AsNoTracking()
            .Where(s => s.BranchId == branchId && s.IsActive);

        if (!string.IsNullOrWhiteSpace(term))
        {
            var pattern = $"%{term.Trim()}%";
            query = query.Where(s =>
                EF.Functions.ILike(s.Name, pattern)
                || EF.Functions.ILike(s.Description, pattern));
        }

        var total = await query.CountAsync();
        var results = await query
            .OrderBy(s => s.Name)
            .Skip((page - 1) * size)
            .Take(size)
            .Select(s => new ServiceTypeResponse(
                s.Id, s.BranchId, s.Name, s.Description, s.DurationMinutes, s.IsActive))
            .ToListAsync();

        return PagedResponse<ServiceTypeResponse>.Create(results, total, page, size);
    }

    /// <returns>null if the branch or service type does not exist.</returns>
    public async Task<PagedResponse<SlotResponse>?> ListAvailableSlotsAsync(
        string branchId,
        string serviceTypeId,
        DateOnly? date,
        int page,
        int size,
        string? term)
    {
        (page, size) = NormalizePage(page, size);

        if (!await db.Branches.AnyAsync(b => b.Id == branchId && b.IsActive))
            return null;

        if (!await db.ServiceTypes.AnyAsync(
                s => s.Id == serviceTypeId && s.BranchId == branchId && s.IsActive))
            return null;

        var query = db.Slots
            .AsNoTracking()
            .Where(s => s.BranchId == branchId
                && s.ServiceTypeId == serviceTypeId
                && s.IsActive);

        if (date.HasValue)
        {
            var startOfDay = date.Value.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
            var endOfDay = date.Value.AddDays(1).ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
            query = query.Where(s => s.StartAt >= startOfDay && s.StartAt < endOfDay);
        }

        query = query.Where(s => s.Appointments.Count(a =>
                    a.Status != AppointmentStatus.Cancelled
                    && a.Status != AppointmentStatus.Rescheduled) < s.Capacity);

        if (!string.IsNullOrWhiteSpace(term))
        {
            var pattern = $"%{term.Trim()}%";
            query = query.Where(s =>
                EF.Functions.ILike(s.Id, pattern)
                || (s.Staff != null && EF.Functions.ILike(s.Staff.FullName, pattern)));
        }

        var total = await query.CountAsync();
        var results = await query
            .OrderBy(s => s.StartAt)
            .Skip((page - 1) * size)
            .Take(size)
            .Select(s => new SlotResponse(
                s.Id,
                s.BranchId,
                s.ServiceTypeId,
                s.StaffId,
                s.Staff != null ? s.Staff.FullName : null,
                s.StartAt,
                s.EndAt,
                s.Capacity,
                true))
            .ToListAsync();

        return PagedResponse<SlotResponse>.Create(results, total, page, size);
    }

    private static (int Page, int Size) NormalizePage(int page, int size)
    {
        var normalizedPage = page < 1 ? 1 : page;
        var normalizedSize = size < 1 ? 20 : (size > 200 ? 200 : size);
        return (normalizedPage, normalizedSize);
    }
}
