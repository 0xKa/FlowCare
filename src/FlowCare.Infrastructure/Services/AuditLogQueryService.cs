using System.Text;
using System.Globalization;
using CsvHelper;
using FlowCare.Application.DTOs;
using FlowCare.Application.Interfaces;
using FlowCare.Domain.Entities;
using FlowCare.Domain.Enums;
using FlowCare.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace FlowCare.Infrastructure.Services;

public class AuditLogQueryService(FlowCareDbContext db) : IAuditLogQueryService
{
    public async Task<PagedResponse<AuditLogResponse>> ListAsync(
        string actorRole,
        string? actorBranchId,
        int page,
        int size,
        string? term)
    {
        (page, size) = NormalizePage(page, size);

        var query = db.AuditLogs.AsNoTracking().AsQueryable();

        if (actorRole == nameof(UserRole.BranchManager) && actorBranchId is not null)
        {
            // For managers: filter to logs whose target entities belong to their branch.
            // We check Metadata for branch_id or match entity IDs against entities in the branch.
            var branchEntityIds = await GetBranchEntityIdsAsync(actorBranchId);
            query = query.Where(a => branchEntityIds.Contains(a.EntityId));
        }

        if (!string.IsNullOrWhiteSpace(term))
        {
            var pattern = $"%{term.Trim()}%";
            query = query.Where(a =>
                EF.Functions.ILike(a.Id, pattern)
                || EF.Functions.ILike(a.ActorId, pattern)
                || EF.Functions.ILike(a.ActorRole, pattern)
                || EF.Functions.ILike(a.ActionType, pattern)
                || EF.Functions.ILike(a.EntityType, pattern)
                || EF.Functions.ILike(a.EntityId, pattern)
                || (a.Metadata != null && EF.Functions.ILike(a.Metadata, pattern)));
        }

        var total = await query.CountAsync();

        var logs = await query
            .OrderByDescending(a => a.Timestamp)
            .Skip((page - 1) * size)
            .Take(size)
            .ToListAsync();

        return new PagedResponse<AuditLogResponse>(
            [.. logs.Select(MapToResponse)],
            total);
    }

    public async Task<Stream> ExportCsvAsync()
    {
        var stream = new MemoryStream();
        var preamble = Encoding.UTF8.GetPreamble();
        await stream.WriteAsync(preamble);

        await using var writer = new StreamWriter(stream, new UTF8Encoding(false), leaveOpen: true);
        await using var csv = new CsvWriter(writer, CultureInfo.InvariantCulture);

        csv.WriteField("Id");
        csv.WriteField("ActorId");
        csv.WriteField("ActorRole");
        csv.WriteField("ActionType");
        csv.WriteField("EntityType");
        csv.WriteField("EntityId");
        csv.WriteField("Timestamp");
        csv.WriteField("Metadata");
        await csv.NextRecordAsync();

        var logs = db.AuditLogs
            .AsNoTracking()
            .OrderByDescending(a => a.Timestamp)
            .AsAsyncEnumerable();

        await foreach (var log in logs)
        {
            csv.WriteField(log.Id);
            csv.WriteField(log.ActorId);
            csv.WriteField(log.ActorRole);
            csv.WriteField(log.ActionType);
            csv.WriteField(log.EntityType);
            csv.WriteField(log.EntityId);
            csv.WriteField(log.Timestamp.ToString("o"));
            csv.WriteField(log.Metadata);
            await csv.NextRecordAsync();
        }

        await writer.FlushAsync();
        stream.Position = 0;
        return stream;
    }

    private async Task<HashSet<string>> GetBranchEntityIdsAsync(string branchId)
    {
        var ids = new HashSet<string> { branchId };

        // Slots in this branch
        var slotIds = await db.Slots.IgnoreQueryFilters()
            .Where(s => s.BranchId == branchId)
            .Select(s => s.Id)
            .ToListAsync();
        ids.UnionWith(slotIds);

        // Appointments in this branch
        var appointmentIds = await db.Appointments
            .Where(a => a.BranchId == branchId)
            .Select(a => a.Id)
            .ToListAsync();
        ids.UnionWith(appointmentIds);

        // Service types in this branch
        var serviceTypeIds = await db.ServiceTypes
            .Where(s => s.BranchId == branchId)
            .Select(s => s.Id)
            .ToListAsync();
        ids.UnionWith(serviceTypeIds);

        // Users (staff) in this branch
        var userIds = await db.Users
            .Where(u => u.BranchId == branchId)
            .Select(u => u.Id)
            .ToListAsync();
        ids.UnionWith(userIds);

        return ids;
    }

    private static AuditLogResponse MapToResponse(AuditLog a) => new(
        a.Id,
        a.ActorId,
        a.ActorRole,
        a.ActionType,
        a.EntityType,
        a.EntityId,
        a.Timestamp,
        a.Metadata);

    private static (int Page, int Size) NormalizePage(int page, int size)
    {
        var normalizedPage = page < 1 ? 1 : page;
        var normalizedSize = size < 1 ? 20 : (size > 200 ? 200 : size);
        return (normalizedPage, normalizedSize);
    }
}
