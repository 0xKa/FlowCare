namespace FlowCare.Domain.Entities;

public abstract class BaseDomain
{
    public Guid Id { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
}
