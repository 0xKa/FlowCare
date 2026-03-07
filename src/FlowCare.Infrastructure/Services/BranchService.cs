using FlowCare.Application.DTOs;
using FlowCare.Application.Interfaces;
using FlowCare.Domain.Enums;
using FlowCare.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace FlowCare.Infrastructure.Services;

public class BranchService(FlowCareDbContext db) : IBranchService
{
    public async Task<List<BranchResponse>> ListBranchesAsync()
    {
        return await db.Branches
            .AsNoTracking()
            .Where(b => b.IsActive)
            .OrderBy(b => b.Name)
            .Select(b => new BranchResponse(
                b.Id, b.Name, b.City, b.Address, b.Timezone, b.IsActive))
            .ToListAsync();
    }

    /// <returns>null if the branch does not exist.</returns>
    public async Task<List<ServiceTypeResponse>?> ListServicesAsync(string branchId)
    {
        if (!await db.Branches.AnyAsync(b => b.Id == branchId && b.IsActive))
            return null;

        return await db.ServiceTypes
            .AsNoTracking()
            .Where(s => s.BranchId == branchId && s.IsActive)
            .OrderBy(s => s.Name)
            .Select(s => new ServiceTypeResponse(
                s.Id, s.BranchId, s.Name, s.Description, s.DurationMinutes, s.IsActive))
            .ToListAsync();
    }

    /// <returns>null if the branch or service type does not exist.</returns>
    public async Task<List<SlotResponse>?> ListAvailableSlotsAsync(
        string branchId, string serviceTypeId, DateOnly? date)
    {
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

        return await query
            .OrderBy(s => s.StartAt)
            .Where(s => s.Appointments.Count(a =>
                    a.Status != AppointmentStatus.Cancelled
                    && a.Status != AppointmentStatus.Rescheduled) < s.Capacity)
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
    }
}
