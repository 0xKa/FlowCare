namespace FlowCare.Application.DTOs;

public record BranchResponse(
    string Id,
    string Name,
    string City,
    string Address,
    string Timezone,
    bool IsActive);

public record ServiceTypeResponse(
    string Id,
    string BranchId,
    string Name,
    string Description,
    int DurationMinutes,
    bool IsActive);

public record SlotResponse(
    string Id,
    string BranchId,
    string ServiceTypeId,
    string? StaffId,
    string? StaffName,
    DateTimeOffset StartAt,
    DateTimeOffset EndAt,
    int Capacity,
    bool IsAvailable);

public record QueueEntryResponse(
    int QueueNumber,
    string AppointmentId,
    string CustomerId,
    string? CustomerName,
    string ServiceTypeId,
    string? ServiceTypeName,
    string? SlotId,
    DateTimeOffset? SlotStartAt,
    DateTimeOffset CheckedInAt);

public record LiveQueueResponse(
    string BranchId,
    int TotalCheckedIn,
    List<QueueEntryResponse> Entries);
