namespace FlowCare.Domain.Entities;

public class Slot : BaseEntity
{
    public string BranchId { get; set; } = string.Empty;
    public string ServiceTypeId { get; set; } = string.Empty;
    public string? StaffId { get; set; }
    public DateTimeOffset StartAt { get; set; }
    public DateTimeOffset EndAt { get; set; }
    public int Capacity { get; set; }
    public bool IsActive { get; set; }
    public DateTimeOffset? DeletedAt { get; set; }

    public Branch Branch { get; set; } = null!;
    public ServiceType ServiceType { get; set; } = null!;
    public User? Staff { get; set; }
    public ICollection<Appointment> Appointments { get; set; } = [];
}
