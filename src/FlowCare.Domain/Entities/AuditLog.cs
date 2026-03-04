namespace FlowCare.Domain.Entities;

public class AuditLog : BaseDomain
{
    public string SeedId { get; set; } = string.Empty;
    public Guid ActorId { get; set; }
    public string ActorRole { get; set; } = string.Empty;
    public string ActionType { get; set; } = string.Empty;
    public string EntityType { get; set; } = string.Empty;
    public string EntityId { get; set; } = string.Empty;
    public DateTimeOffset Timestamp { get; set; }
    public string? Metadata { get; set; }
}
