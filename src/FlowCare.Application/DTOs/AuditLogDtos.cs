namespace FlowCare.Application.DTOs;

public record AuditLogResponse
{
    public string Id { get; init; } = null!;
    public string ActorId { get; init; } = null!;
    public string ActorRole { get; init; } = null!;
    public string ActionType { get; init; } = null!;
    public string EntityType { get; init; } = null!;
    public string EntityId { get; init; } = null!;
    public DateTimeOffset Timestamp { get; init; }
    public string? Metadata { get; init; }

    public AuditLogResponse(
        string id,
        string actorId,
        string actorRole,
        string actionType,
        string entityType,
        string entityId,
        DateTimeOffset timestamp,
        string? metadata)
    {
        Id = id;
        ActorId = actorId;
        ActorRole = actorRole;
        ActionType = actionType;
        EntityType = entityType;
        EntityId = entityId;
        Timestamp = timestamp;
        Metadata = metadata;
    }
}
