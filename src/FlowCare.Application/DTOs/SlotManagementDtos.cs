using System.ComponentModel.DataAnnotations;

namespace FlowCare.Application.DTOs;

public record CreateSlotRequest(
    [property: Required]
    [property: StringLength(50, MinimumLength = 1)]
    string ServiceTypeId,
    [property: StringLength(50, MinimumLength = 1)]
    string? StaffId,
    DateTimeOffset StartAt,
    DateTimeOffset EndAt,
    [property: Range(1, int.MaxValue)]
    int Capacity);

public record CreateBulkSlotsRequest(
    [property: Required]
    [property: MinLength(1)]
    List<CreateSlotRequest> Slots);

public record UpdateSlotRequest(
    [property: StringLength(50, MinimumLength = 1)]
    string? ServiceTypeId,
    [property: StringLength(50, MinimumLength = 1)]
    string? StaffId,
    DateTimeOffset? StartAt,
    DateTimeOffset? EndAt,
    [property: Range(1, int.MaxValue)]
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
