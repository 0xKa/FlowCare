namespace FlowCare.Domain.Entities;

public class ServiceType : BaseEntity
{
    public string SeedId { get; set; } = string.Empty;
    public Guid BranchId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int DurationMinutes { get; set; }
    public bool IsActive { get; set; }

    public Branch Branch { get; set; } = null!;
    public ICollection<Slot> Slots { get; set; } = [];
    public ICollection<StaffServiceType> StaffServiceTypes { get; set; } = [];
    public ICollection<Appointment> Appointments { get; set; } = [];
}
