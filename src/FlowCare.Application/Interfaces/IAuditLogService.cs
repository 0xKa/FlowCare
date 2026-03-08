namespace FlowCare.Application.Interfaces;

public interface IAuditLogService
{
    Task LogAsync(string actorId, string actorRole, string actionType,
        string entityType, string entityId, object? metadata = null);
}
