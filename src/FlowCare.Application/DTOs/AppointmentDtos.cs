using System.ComponentModel.DataAnnotations;

namespace FlowCare.Application.DTOs;


public record BookAppointmentRequest(
    [property: Required]
    [property: StringLength(50, MinimumLength = 1)]
    string BranchId,
    [property: Required]
    [property: StringLength(50, MinimumLength = 1)]
    string ServiceTypeId,
    [property: Required]
    [property: StringLength(50, MinimumLength = 1)]
    string SlotId);

public record RescheduleAppointmentRequest(
    [property: Required]
    [property: StringLength(50, MinimumLength = 1)]
    string NewSlotId);

public record UpdateAppointmentStatusRequest(
    [property: Required]
    [property: StringLength(20)]
    [property: RegularExpression("^(CheckedIn|NoShow|Completed)$",
        ErrorMessage = "Status must be one of: CheckedIn, NoShow, Completed.")]
    string Status);


public record AppointmentResponse(
    string Id,
    string CustomerId,
    string? CustomerName,
    string BranchId,
    string? BranchName,
    string ServiceTypeId,
    string? ServiceTypeName,
    string? SlotId,
    string? StaffId,
    string? StaffName,
    string Status,
    bool HasAttachment,
    string? Notes,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt);
