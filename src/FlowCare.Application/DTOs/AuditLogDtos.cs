namespace FlowCare.Application.DTOs;

public record AuditLogResponse(
    string Id,
    string ActorId,
    string ActorRole,
    string ActionType,
    string EntityType,
    string EntityId,
    DateTimeOffset Timestamp,
    string? Metadata);
