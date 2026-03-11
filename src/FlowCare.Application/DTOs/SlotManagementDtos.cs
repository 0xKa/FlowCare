namespace FlowCare.Application.DTOs;

public record CreateSlotRequest(
    string ServiceTypeId,
    string? StaffId,
    DateTimeOffset StartAt,
    DateTimeOffset EndAt,
    int Capacity);

public record CreateBulkSlotsRequest(
    List<CreateSlotRequest> Slots);

public record UpdateSlotRequest(
    string? ServiceTypeId,
    string? StaffId,
    DateTimeOffset? StartAt,
    DateTimeOffset? EndAt,
    int? Capacity,
    bool? IsActive);

public record SlotDetailResponse(
    string Id,
    string BranchId,
    string? BranchName,
    string ServiceTypeId,
    string? ServiceTypeName,
    string? StaffId,
    string? StaffName,
    DateTimeOffset StartAt,
    DateTimeOffset EndAt,
    int Capacity,
    bool IsActive,
    DateTimeOffset? DeletedAt,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt);
