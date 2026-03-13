using System.ComponentModel.DataAnnotations;

namespace FlowCare.Application.DTOs;

public record UpdateRetentionPeriodRequest
{
    [Required]
    [Range(0, int.MaxValue, ErrorMessage = "Retention period must be a non-negative integer.")]
    public int? Days { get; init; }
}

public record SoftDeleteSettingsResponse
{
    public int RetentionDays { get; init; }

    public SoftDeleteSettingsResponse(int retentionDays)
    {
        RetentionDays = retentionDays;
    }
}

public record CleanupResultResponse
{
    public int DeletedSlots { get; init; }
    public int UpdatedAppointments { get; init; }

    public CleanupResultResponse(int deletedSlots, int updatedAppointments)
    {
        DeletedSlots = deletedSlots;
        UpdatedAppointments = updatedAppointments;
    }
}
