namespace FlowCare.Domain.Entities;

public class StaffServiceType
{
    public Guid StaffId { get; set; }
    public Guid ServiceTypeId { get; set; }

    public User Staff { get; set; } = null!;
    public ServiceType ServiceType { get; set; } = null!;
}
