using System.ComponentModel.DataAnnotations;

namespace FlowCare.Application.DTOs;

public record UpdateRetentionPeriodRequest
{
    [Required]
    [Range(0, int.MaxValue, ErrorMessage = "Retention period must be a non-negative integer.")]
    public int? Days { get; init; }
}

public record SoftDeleteSettingsResponse(
    int RetentionDays);

public record CleanupResultResponse(
    int DeletedSlots,
    int UpdatedAppointments);
