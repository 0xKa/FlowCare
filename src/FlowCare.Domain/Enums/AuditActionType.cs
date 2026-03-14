namespace FlowCare.Domain.Enums;

public enum AuditActionType
{
    AppointmentBooked,
    AppointmentCancelled,
    AppointmentRescheduled,
    AppointmentStatusUpdated,
    SlotCreated,
    SlotUpdated,
    SlotDeleted,
    StaffAssignmentChanged,
    RetentionPeriodUpdated,
    HardDelete,
    RateLimitsUpdated,
    CleanupWorkerToggled
}

public static class AuditActionTypeExtensions
{
    public static string ToStorageMessage(this AuditActionType actionType) => actionType switch
    {
        AuditActionType.AppointmentBooked => "APPOINTMENT_BOOKED",
        AuditActionType.AppointmentCancelled => "APPOINTMENT_CANCELLED",
        AuditActionType.AppointmentRescheduled => "APPOINTMENT_RESCHEDULED",
        AuditActionType.AppointmentStatusUpdated => "APPOINTMENT_STATUS_UPDATED",
        AuditActionType.SlotCreated => "SLOT_CREATED",
        AuditActionType.SlotUpdated => "SLOT_UPDATED",
        AuditActionType.SlotDeleted => "SLOT_DELETED",
        AuditActionType.StaffAssignmentChanged => "STAFF_ASSIGNMENT_CHANGED",
        AuditActionType.RetentionPeriodUpdated => "RETENTION_PERIOD_UPDATED",
        AuditActionType.HardDelete => "HARD_DELETE",
        AuditActionType.RateLimitsUpdated => "RATE_LIMITS_UPDATED",
        AuditActionType.CleanupWorkerToggled => "CLEANUP_WORKER_TOGGLED",
        _ => throw new ArgumentOutOfRangeException(nameof(actionType), actionType, "Unsupported audit action type.")
    };
}