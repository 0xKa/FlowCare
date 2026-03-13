using System.ComponentModel.DataAnnotations;
using FlowCare.Domain.Enums;

namespace FlowCare.Application.DTOs;


public record BookAppointmentRequest
{
    [Required]
    [StringLength(50, MinimumLength = 1)]
    public string BranchId { get; init; } = null!;

    [Required]
    [StringLength(50, MinimumLength = 1)]
    public string ServiceTypeId { get; init; } = null!;

    [Required]
    [StringLength(50, MinimumLength = 1)]
    public string SlotId { get; init; } = null!;

    public BookAppointmentRequest(string branchId, string serviceTypeId, string slotId)
    {
        BranchId = branchId;
        ServiceTypeId = serviceTypeId;
        SlotId = slotId;
    }
}

public record RescheduleAppointmentRequest
{
    [Required]
    [StringLength(50, MinimumLength = 1)]
    public string NewSlotId { get; init; } = null!;

    public RescheduleAppointmentRequest(string newSlotId)
    {
        NewSlotId = newSlotId;
    }
}

public record UpdateAppointmentStatusRequest
{
    [Required]
    [StringLength(20)]
    [EnumDataType(typeof(AppointmentStatus), ErrorMessage = $"Invalid status value.")]
    public string Status { get; init; } = null!;
}

public record AppointmentResponse
{
    public string Id { get; init; } = null!;
    public string CustomerId { get; init; } = null!;
    public string? CustomerName { get; init; }
    public string BranchId { get; init; } = null!;
    public string? BranchName { get; init; }
    public string ServiceTypeId { get; init; } = null!;
    public string? ServiceTypeName { get; init; }
    public string? SlotId { get; init; }
    public string? StaffId { get; init; }
    public string? StaffName { get; init; }
    public string Status { get; init; } = null!;
    public bool HasAttachment { get; init; }
    public string? Notes { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset? UpdatedAt { get; init; }

    public AppointmentResponse(
        string id,
        string customerId,
        string? customerName,
        string branchId,
        string? branchName,
        string serviceTypeId,
        string? serviceTypeName,
        string? slotId,
        string? staffId,
        string? staffName,
        string status,
        bool hasAttachment,
        string? notes,
        DateTimeOffset createdAt,
        DateTimeOffset? updatedAt)
    {
        Id = id;
        CustomerId = customerId;
        CustomerName = customerName;
        BranchId = branchId;
        BranchName = branchName;
        ServiceTypeId = serviceTypeId;
        ServiceTypeName = serviceTypeName;
        SlotId = slotId;
        StaffId = staffId;
        StaffName = staffName;
        Status = status;
        HasAttachment = hasAttachment;
        Notes = notes;
        CreatedAt = createdAt;
        UpdatedAt = updatedAt;
    }
}
