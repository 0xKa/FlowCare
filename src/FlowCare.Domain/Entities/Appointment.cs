using FlowCare.Domain.Enums;

namespace FlowCare.Domain.Entities;

public class Appointment : BaseEntity
{
    public string CustomerId { get; set; } = string.Empty;
    public string BranchId { get; set; } = string.Empty;
    public string ServiceTypeId { get; set; } = string.Empty;
    public string? SlotId { get; set; }
    public string? StaffId { get; set; }
    public AppointmentStatus Status { get; set; }
    public string? AttachmentPath { get; set; }
    public string? Notes { get; set; }

    public User Customer { get; set; } = null!;
    public Branch Branch { get; set; } = null!;
    public ServiceType ServiceType { get; set; } = null!;
    public Slot? Slot { get; set; }
    public User? Staff { get; set; }
}
