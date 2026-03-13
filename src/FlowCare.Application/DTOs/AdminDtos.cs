using System.ComponentModel.DataAnnotations;

namespace FlowCare.Application.DTOs;

public record UpdateRetentionPeriodRequest(
    [property: Range(0, 3650)] int Days);

public record SoftDeleteSettingsResponse(
    int RetentionDays);

public record CleanupResultResponse(
    int DeletedSlots,
    int UpdatedAppointments);
