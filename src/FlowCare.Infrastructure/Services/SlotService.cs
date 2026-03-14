using FlowCare.Application.DTOs;
using FlowCare.Application.Interfaces;
using FlowCare.Domain.Entities;
using FlowCare.Domain.Enums;
using FlowCare.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace FlowCare.Infrastructure.Services;

public class SlotService(FlowCareDbContext db, IAuditLogService auditLog) : ISlotService
{
    public async Task<(SlotDetailResponse? Result, string? Error)> CreateSlotAsync(
        string branchId, CreateSlotRequest request, string actorId, string actorRole)
    {
        var branch = await db.Branches.AsNoTracking().FirstOrDefaultAsync(b => b.Id == branchId && b.IsActive);
        if (branch is null)
            return (null, "Branch not found.");

        var serviceType = await db.ServiceTypes.AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == request.ServiceTypeId && s.BranchId == branchId && s.IsActive);
        if (serviceType is null)
            return (null, "Service type not found in this branch.");

        if (request.StaffId is not null)
        {
            var staff = await db.Users.AsNoTracking()
                .FirstOrDefaultAsync(u => u.Id == request.StaffId && u.BranchId == branchId
                    && u.Role == UserRole.Staff && u.IsActive);
            if (staff is null)
                return (null, "Staff member not found in this branch.");
        }

        if (request.StartAt >= request.EndAt)
            return (null, "StartAt must be before EndAt.");

        if (request.Capacity < 1)
            return (null, "Capacity must be at least 1.");

        var slot = new Slot
        {
            Id = $"slot_{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}_{Guid.NewGuid():N}"[..32],
            BranchId = branchId,
            ServiceTypeId = request.ServiceTypeId,
            StaffId = request.StaffId,
            StartAt = request.StartAt.ToUniversalTime(),
            EndAt = request.EndAt.ToUniversalTime(),
            Capacity = request.Capacity,
            IsActive = true
        };

        db.Slots.Add(slot);
        await db.SaveChangesAsync();

        await auditLog.LogAsync(actorId, actorRole, AuditActionType.SlotCreated, "SLOT", slot.Id,
            new { branch_id = branchId, service_type_id = request.ServiceTypeId });

        return (await GetSlotDetailAsync(slot.Id), null);
    }

    public async Task<(List<SlotDetailResponse>? Results, string? Error)> CreateBulkSlotsAsync(
        string branchId, List<CreateSlotRequest> requests, string actorId, string actorRole)
    {
        var branch = await db.Branches.AsNoTracking().FirstOrDefaultAsync(b => b.Id == branchId && b.IsActive);
        if (branch is null)
            return (null, "Branch not found.");

        var results = new List<SlotDetailResponse>();
        foreach (var request in requests)
        {
            var (result, error) = await CreateSlotAsync(branchId, request, actorId, actorRole);
            if (error is not null)
                return (null, $"Error creating slot at index {results.Count}: {error}");
            results.Add(result!);
        }

        return (results, null);
    }

    public async Task<(SlotDetailResponse? Result, string? Error)> UpdateSlotAsync(
        string slotId, UpdateSlotRequest request, string actorId, string actorRole, string? actorBranchId)
    {
        var slot = await db.Slots.FirstOrDefaultAsync(s => s.Id == slotId);
        if (slot is null)
            return (null, "Slot not found.");

        // Branch scoping
        if (actorRole == nameof(UserRole.BranchManager) && slot.BranchId != actorBranchId)
            return (null, "Cannot update slots outside your branch.");

        if (request.ServiceTypeId is not null)
        {
            var serviceType = await db.ServiceTypes.AsNoTracking()
                .FirstOrDefaultAsync(s => s.Id == request.ServiceTypeId && s.BranchId == slot.BranchId && s.IsActive);
            if (serviceType is null)
                return (null, "Service type not found in this branch.");
            slot.ServiceTypeId = request.ServiceTypeId;
        }

        if (request.StaffId is not null)
        {
            var staff = await db.Users.AsNoTracking()
                .FirstOrDefaultAsync(u => u.Id == request.StaffId && u.BranchId == slot.BranchId
                    && u.Role == UserRole.Staff && u.IsActive);
            if (staff is null)
                return (null, "Staff member not found in this branch.");
            slot.StaffId = request.StaffId;
        }

        if (request.StartAt.HasValue)
            slot.StartAt = request.StartAt.Value.ToUniversalTime();

        if (request.EndAt.HasValue)
            slot.EndAt = request.EndAt.Value.ToUniversalTime();

        if (slot.StartAt >= slot.EndAt)
            return (null, "StartAt must be before EndAt.");

        if (request.Capacity.HasValue)
        {
            if (request.Capacity.Value < 1)
                return (null, "Capacity must be at least 1.");
            slot.Capacity = request.Capacity.Value;
        }

        if (request.IsActive.HasValue)
            slot.IsActive = request.IsActive.Value;

        await db.SaveChangesAsync();

        await auditLog.LogAsync(actorId, actorRole, AuditActionType.SlotUpdated, "SLOT", slotId, null);

        return (await GetSlotDetailAsync(slot.Id), null);
    }

    public async Task<(bool Success, string? Error)> SoftDeleteSlotAsync(
        string slotId, string actorId, string actorRole, string? actorBranchId)
    {
        // Use IgnoreQueryFilters to find already-soft-deleted slots too (idempotency)
        var slot = await db.Slots.IgnoreQueryFilters()
            .FirstOrDefaultAsync(s => s.Id == slotId);
        if (slot is null)
            return (false, "Slot not found.");

        if (slot.DeletedAt is not null)
            return (false, "Slot is already deleted.");

        // Branch scoping
        if (actorRole == nameof(UserRole.BranchManager) && slot.BranchId != actorBranchId)
            return (false, "Cannot delete slots outside your branch.");

        slot.DeletedAt = DateTimeOffset.UtcNow;
        slot.IsActive = false;
        await db.SaveChangesAsync();

        await auditLog.LogAsync(actorId, actorRole, AuditActionType.SlotDeleted, "SLOT", slotId,
            new { branch_id = slot.BranchId });

        return (true, null);
    }

    public async Task<PagedResponse<SlotDetailResponse>> ListSlotsAsync(
        string branchId,
        string actorRole,
        string? actorBranchId,
        bool includeDeleted,
        int page,
        int size,
        string? term)
    {
        (page, size) = NormalizePage(page, size);

        // Branch scoping for managers
        if (actorRole == nameof(UserRole.BranchManager) && branchId != actorBranchId)
            return PagedResponse<SlotDetailResponse>.Create([], 0, page, size);

        IQueryable<Slot> query;
        if (includeDeleted && actorRole == nameof(UserRole.Admin))
        {
            // Admin can see soft-deleted slots
            query = db.Slots.IgnoreQueryFilters()
                .Where(s => s.BranchId == branchId);
        }
        else
        {
            query = db.Slots.Where(s => s.BranchId == branchId);
        }

        if (!string.IsNullOrWhiteSpace(term))
        {
            var pattern = $"%{term.Trim()}%";
            query = query.Where(s =>
                EF.Functions.ILike(s.Id, pattern)
                || EF.Functions.ILike(s.ServiceType.Name, pattern)
                || (s.Staff != null && EF.Functions.ILike(s.Staff.FullName, pattern)));
        }

        var total = await query.CountAsync();

        var slots = await query
            .AsNoTracking()
            .Include(s => s.Branch)
            .Include(s => s.ServiceType)
            .Include(s => s.Staff)
            .OrderBy(s => s.StartAt)
            .Skip((page - 1) * size)
            .Take(size)
            .ToListAsync();

        return PagedResponse<SlotDetailResponse>.Create(
            [.. slots.Select(MapToDetail)],
            total,
            page,
            size);
    }

    private async Task<SlotDetailResponse> GetSlotDetailAsync(string slotId)
    {
        var slot = await db.Slots
            .AsNoTracking()
            .Include(s => s.Branch)
            .Include(s => s.ServiceType)
            .Include(s => s.Staff)
            .FirstAsync(s => s.Id == slotId);

        return MapToDetail(slot);
    }

    private static (int Page, int Size) NormalizePage(int page, int size)
    {
        var normalizedPage = page < 1 ? 1 : page;
        var normalizedSize = size < 1 ? 20 : (size > 200 ? 200 : size);
        return (normalizedPage, normalizedSize);
    }

    private static SlotDetailResponse MapToDetail(Slot s) => new(
        s.Id,
        s.BranchId,
        s.Branch?.Name,
        s.ServiceTypeId,
        s.ServiceType?.Name,
        s.StaffId,
        s.Staff?.FullName,
        s.StartAt,
        s.EndAt,
        s.Capacity,
        s.IsActive,
        s.DeletedAt,
        s.CreatedAt,
        s.UpdatedAt);
}
