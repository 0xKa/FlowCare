using FlowCare.Domain.Enums;

namespace FlowCare.Application.Interfaces;

public interface IAuditLogService
{
    Task LogAsync(string actorId, string actorRole, AuditActionType actionType,
        string entityType, string entityId, object? metadata = null);
}
