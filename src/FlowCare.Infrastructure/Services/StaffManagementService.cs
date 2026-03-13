using FlowCare.Application.DTOs;
using FlowCare.Application.Interfaces;
using FlowCare.Domain.Entities;
using FlowCare.Domain.Enums;
using FlowCare.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace FlowCare.Infrastructure.Services;

public class StaffManagementService(FlowCareDbContext db, IAuditLogService auditLog) : IStaffManagementService
{
    public async Task<PagedResponse<StaffResponse>> ListStaffAsync(
        string actorRole,
        string? actorBranchId,
        int page,
        int size,
        string? term)
    {
        (page, size) = NormalizePage(page, size);

        var query = db.Users
            .AsNoTracking()
            .Include(u => u.Branch)
            .Include(u => u.StaffServiceTypes)
            .Where(u => u.Role == UserRole.Staff || u.Role == UserRole.BranchManager);

        if (actorRole == nameof(UserRole.BranchManager))
            query = query.Where(u => u.BranchId == actorBranchId);

        if (!string.IsNullOrWhiteSpace(term))
        {
            var pattern = $"%{term.Trim()}%";
            query = query.Where(u =>
                EF.Functions.ILike(u.Id, pattern)
                || EF.Functions.ILike(u.Username, pattern)
                || EF.Functions.ILike(u.FullName, pattern)
                || EF.Functions.ILike(u.Email, pattern)
                || (u.Phone != null && EF.Functions.ILike(u.Phone, pattern))
                || (u.Branch != null && EF.Functions.ILike(u.Branch.Name, pattern))
                || EF.Functions.ILike(u.Role.ToString(), pattern));
        }

        var total = await query.CountAsync();

        var staff = await query
            .OrderBy(u => u.FullName)
            .Skip((page - 1) * size)
            .Take(size)
            .ToListAsync();

        return PagedResponse<StaffResponse>.Create(
            staff.Select(MapToStaffResponse).ToList(),
            total,
            page,
            size);
    }

    public async Task<(bool Success, string? Error)> AssignStaffToServicesAsync(
        string staffId, List<string> serviceTypeIds, string actorId, string actorRole, string? actorBranchId)
    {
        var staffUser = await db.Users
            .Include(u => u.StaffServiceTypes)
            .FirstOrDefaultAsync(u => u.Id == staffId && u.Role == UserRole.Staff);

        if (staffUser is null)
            return (false, "Staff member not found.");

        // Branch scoping
        if (actorRole == nameof(UserRole.BranchManager) && staffUser.BranchId != actorBranchId)
            return (false, "Cannot manage staff outside your branch.");

        // Validate all service types exist and belong to the staff's branch
        foreach (var serviceTypeId in serviceTypeIds)
        {
            var serviceType = await db.ServiceTypes.AsNoTracking()
                .FirstOrDefaultAsync(s => s.Id == serviceTypeId && s.BranchId == staffUser.BranchId);
            if (serviceType is null)
                return (false, $"Service type '{serviceTypeId}' not found in the staff's branch.");
        }

        // Add only new assignments
        var existingIds = staffUser.StaffServiceTypes.Select(sst => sst.ServiceTypeId).ToHashSet();
        var added = new List<string>();
        foreach (var serviceTypeId in serviceTypeIds)
        {
            if (existingIds.Contains(serviceTypeId))
                continue;

            db.StaffServiceTypes.Add(new StaffServiceType
            {
                StaffId = staffId,
                ServiceTypeId = serviceTypeId
            });
            added.Add(serviceTypeId);
        }

        if (added.Count > 0)
        {
            await db.SaveChangesAsync();
            await auditLog.LogAsync(actorId, actorRole, "STAFF_ASSIGNMENT_CHANGED", "USER", staffId,
                new { action = "assigned", service_type_ids = added });
        }

        return (true, null);
    }

    public async Task<(bool Success, string? Error)> UnassignStaffFromServiceAsync(
        string staffId, string serviceTypeId, string actorId, string actorRole, string? actorBranchId)
    {
        var staffUser = await db.Users.AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == staffId && u.Role == UserRole.Staff);
        if (staffUser is null)
            return (false, "Staff member not found.");

        // Branch scoping
        if (actorRole == nameof(UserRole.BranchManager) && staffUser.BranchId != actorBranchId)
            return (false, "Cannot manage staff outside your branch.");

        var assignment = await db.StaffServiceTypes
            .FirstOrDefaultAsync(sst => sst.StaffId == staffId && sst.ServiceTypeId == serviceTypeId);
        if (assignment is null)
            return (false, "Staff is not assigned to this service type.");

        db.StaffServiceTypes.Remove(assignment);
        await db.SaveChangesAsync();

        await auditLog.LogAsync(actorId, actorRole, "STAFF_ASSIGNMENT_CHANGED", "USER", staffId,
            new { action = "unassigned", service_type_id = serviceTypeId });

        return (true, null);
    }

    private static (int Page, int Size) NormalizePage(int page, int size)
    {
        var normalizedPage = page < 1 ? 1 : page;
        var normalizedSize = size < 1 ? 20 : (size > 200 ? 200 : size);
        return (normalizedPage, normalizedSize);
    }

    private static StaffResponse MapToStaffResponse(User u) => new(
        u.Id,
        u.Username,
        u.FullName,
        u.Email,
        u.Phone,
        u.BranchId,
        u.Branch?.Name,
        u.Role.ToString(),
        u.IsActive,
        u.StaffServiceTypes.Select(sst => sst.ServiceTypeId).ToList());
}
