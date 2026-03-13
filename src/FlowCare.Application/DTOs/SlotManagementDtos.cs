using System.ComponentModel.DataAnnotations;

namespace FlowCare.Application.DTOs;

public record CreateSlotRequest
{
    [Required]
    [StringLength(50, MinimumLength = 1)]
    public string ServiceTypeId { get; init; } = null!;

    [StringLength(50, MinimumLength = 1)]
    public string? StaffId { get; init; }

    public DateTimeOffset StartAt { get; init; }
    public DateTimeOffset EndAt { get; init; }

    [Range(1, int.MaxValue)]
    public int Capacity { get; init; }

    public CreateSlotRequest(
        string serviceTypeId,
        string? staffId,
        DateTimeOffset startAt,
        DateTimeOffset endAt,
        int capacity)
    {
        ServiceTypeId = serviceTypeId;
        StaffId = staffId;
        StartAt = startAt;
        EndAt = endAt;
        Capacity = capacity;
    }
}

public record CreateBulkSlotsRequest
{
    [Required]
    [MinLength(1)]
    public List<CreateSlotRequest> Slots { get; init; }

    public CreateBulkSlotsRequest(List<CreateSlotRequest> slots)
    {
        Slots = slots;
    }
}

public record UpdateSlotRequest
{
    [StringLength(50, MinimumLength = 1)]
    public string? ServiceTypeId { get; init; }

    [StringLength(50, MinimumLength = 1)]
    public string? StaffId { get; init; }

    public DateTimeOffset? StartAt { get; init; }
    public DateTimeOffset? EndAt { get; init; }

    [Range(1, int.MaxValue)]
    public int? Capacity { get; init; }

    public bool? IsActive { get; init; }

    public UpdateSlotRequest(
        string? serviceTypeId,
        string? staffId,
        DateTimeOffset? startAt,
        DateTimeOffset? endAt,
        int? capacity,
        bool? isActive)
    {
        ServiceTypeId = serviceTypeId;
        StaffId = staffId;
        StartAt = startAt;
        EndAt = endAt;
        Capacity = capacity;
        IsActive = isActive;
    }
}

public record SlotDetailResponse
{
    public string Id { get; init; } = null!;
    public string BranchId { get; init; } = null!;
    public string? BranchName { get; init; }
    public string ServiceTypeId { get; init; } = null!;
    public string? ServiceTypeName { get; init; }
    public string? StaffId { get; init; }
    public string? StaffName { get; init; }
    public DateTimeOffset StartAt { get; init; }
    public DateTimeOffset EndAt { get; init; }
    public int Capacity { get; init; }
    public bool IsActive { get; init; }
    public DateTimeOffset? DeletedAt { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset? UpdatedAt { get; init; }

    public SlotDetailResponse(
        string id,
        string branchId,
        string? branchName,
        string serviceTypeId,
        string? serviceTypeName,
        string? staffId,
        string? staffName,
        DateTimeOffset startAt,
        DateTimeOffset endAt,
        int capacity,
        bool isActive,
        DateTimeOffset? deletedAt,
        DateTimeOffset createdAt,
        DateTimeOffset? updatedAt)
    {
        Id = id;
        BranchId = branchId;
        BranchName = branchName;
        ServiceTypeId = serviceTypeId;
        ServiceTypeName = serviceTypeName;
        StaffId = staffId;
        StaffName = staffName;
        StartAt = startAt;
        EndAt = endAt;
        Capacity = capacity;
        IsActive = isActive;
        DeletedAt = deletedAt;
        CreatedAt = createdAt;
        UpdatedAt = updatedAt;
    }
}
