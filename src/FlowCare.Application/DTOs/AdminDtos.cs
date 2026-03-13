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

public record UpdateRateLimitsRequest
{
    [Required]
    [Range(1, int.MaxValue, ErrorMessage = "Customer booking limit per day must be at least 1.")]
    public int? CustomerBookingsPerDay { get; init; }

    [Required]
    [Range(0, int.MaxValue, ErrorMessage = "Max reschedules per appointment must be a non-negative integer.")]
    public int? MaxReschedulesPerAppointment { get; init; }
}

public record RateLimitSettingsResponse
{
    public int CustomerBookingsPerDay { get; init; }
    public int MaxReschedulesPerAppointment { get; init; }

    public RateLimitSettingsResponse(int customerBookingsPerDay, int maxReschedulesPerAppointment)
    {
        CustomerBookingsPerDay = customerBookingsPerDay;
        MaxReschedulesPerAppointment = maxReschedulesPerAppointment;
    }
}
