namespace FlowCare.Domain.Entities;

public class StaffServiceType
{
    public string StaffId { get; set; } = string.Empty;
    public string ServiceTypeId { get; set; } = string.Empty;

    public User Staff { get; set; } = null!;
    public ServiceType ServiceType { get; set; } = null!;
}
