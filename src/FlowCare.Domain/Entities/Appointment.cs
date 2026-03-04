using FlowCare.Domain.Enums;

namespace FlowCare.Domain.Entities;

public class Appointment : BaseEntity
{
    public string SeedId { get; set; } = string.Empty;
    public Guid CustomerId { get; set; }
    public Guid BranchId { get; set; }
    public Guid ServiceTypeId { get; set; }
    public Guid? SlotId { get; set; }
    public Guid? StaffId { get; set; }
    public AppointmentStatus Status { get; set; }
    public string? AttachmentPath { get; set; }
    public string? Notes { get; set; }

    public User Customer { get; set; } = null!;
    public Branch Branch { get; set; } = null!;
    public ServiceType ServiceType { get; set; } = null!;
    public Slot? Slot { get; set; }
    public User? Staff { get; set; }
}
