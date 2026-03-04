namespace FlowCare.Domain.Entities;

public class Branch : BaseEntity
{
    public string SeedId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string Timezone { get; set; } = string.Empty;
    public bool IsActive { get; set; }

    public ICollection<ServiceType> ServiceTypes { get; set; } = [];
    public ICollection<User> Users { get; set; } = [];
    public ICollection<Slot> Slots { get; set; } = [];
    public ICollection<Appointment> Appointments { get; set; } = [];
}
