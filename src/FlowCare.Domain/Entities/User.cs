using FlowCare.Domain.Enums;

namespace FlowCare.Domain.Entities;

public class User : BaseEntity
{
    public string Username { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public UserRole Role { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? BranchId { get; set; }
    public bool IsActive { get; set; }
    public string? IdImagePath { get; set; }

    public Branch? Branch { get; set; }
    public ICollection<StaffServiceType> StaffServiceTypes { get; set; } = [];
    public ICollection<Slot> AssignedSlots { get; set; } = [];
    public ICollection<Appointment> CustomerAppointments { get; set; } = [];
    public ICollection<Appointment> StaffAppointments { get; set; } = [];
}
