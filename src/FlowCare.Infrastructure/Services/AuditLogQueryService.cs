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
    public async Task<List<AuditLogResponse>> ListAsync(string actorRole, string? actorBranchId)
    {
        var query = db.AuditLogs.AsNoTracking().AsQueryable();

        if (actorRole == nameof(UserRole.BranchManager) && actorBranchId is not null)
        {
            // For managers: filter to logs whose target entities belong to their branch.
            // We check Metadata for branch_id or match entity IDs against entities in the branch.
            var branchEntityIds = await GetBranchEntityIdsAsync(actorBranchId);
            query = query.Where(a => branchEntityIds.Contains(a.EntityId));
        }

        var logs = await query
            .OrderByDescending(a => a.Timestamp)
            .ToListAsync();

        return [.. logs.Select(MapToResponse)];
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
}
