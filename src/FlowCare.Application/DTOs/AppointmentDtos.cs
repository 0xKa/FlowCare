namespace FlowCare.Application.DTOs;


public record BookAppointmentRequest(
    string BranchId,
    string ServiceTypeId,
    string SlotId);

public record RescheduleAppointmentRequest(
    string NewSlotId);

public record UpdateAppointmentStatusRequest(
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
