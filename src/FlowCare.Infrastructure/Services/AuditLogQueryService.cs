using System.Text;
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
        var logs = await db.AuditLogs
            .AsNoTracking()
            .OrderByDescending(a => a.Timestamp)
            .ToListAsync();

        var sb = new StringBuilder();
        sb.AppendLine("Id,ActorId,ActorRole,ActionType,EntityType,EntityId,Timestamp,Metadata");

        foreach (var log in logs)
        {
            sb.Append(EscapeCsv(log.Id)).Append(',');
            sb.Append(EscapeCsv(log.ActorId)).Append(',');
            sb.Append(EscapeCsv(log.ActorRole)).Append(',');
            sb.Append(EscapeCsv(log.ActionType)).Append(',');
            sb.Append(EscapeCsv(log.EntityType)).Append(',');
            sb.Append(EscapeCsv(log.EntityId)).Append(',');
            sb.Append(EscapeCsv(log.Timestamp.ToString("o"))).Append(',');
            sb.AppendLine(EscapeCsv(log.Metadata ?? ""));
        }

        return new MemoryStream(Encoding.UTF8.GetBytes(sb.ToString()));
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

    private static string EscapeCsv(string value)
    {
        if (value.Contains('"') || value.Contains(',') || value.Contains('\n') || value.Contains('\r'))
            return $"\"{value.Replace("\"", "\"\"")}\"";
        return value;
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
