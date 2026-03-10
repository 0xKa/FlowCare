using System.Text.Json;
using FlowCare.Application.Interfaces;
using FlowCare.Domain.Entities;
using FlowCare.Infrastructure.Data;

namespace FlowCare.Infrastructure.Services;

public class AuditLogService(FlowCareDbContext db) : IAuditLogService
{
    public async Task LogAsync(string actorId, string actorRole, string actionType,
        string entityType, string entityId, object? metadata = null)
    {
        var auditLog = new AuditLog
        {
            Id = GenerateId(),
            ActorId = actorId,
            ActorRole = actorRole,
            ActionType = actionType,
            EntityType = entityType,
            EntityId = entityId,
            Timestamp = DateTimeOffset.UtcNow,
            Metadata = metadata is not null ? JsonSerializer.Serialize(metadata) : null
        };

        db.AuditLogs.Add(auditLog);
        await db.SaveChangesAsync();
    }

    private static string GenerateId() => $"aud_{Guid.NewGuid():N}";
}
