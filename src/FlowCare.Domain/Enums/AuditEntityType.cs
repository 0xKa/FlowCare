namespace FlowCare.Domain.Enums;

public enum AuditEntityType
{
    Appointment,
    Slot,
    User,
    SystemSetting
}

public static class AuditEntityTypeExtensions
{
    public static string ToStorageMessage(this AuditEntityType entityType) => entityType switch
    {
        AuditEntityType.Appointment => "APPOINTMENT",
        AuditEntityType.Slot => "SLOT",
        AuditEntityType.User => "USER",
        AuditEntityType.SystemSetting => "SYSTEM_SETTING",
        _ => throw new ArgumentOutOfRangeException(nameof(entityType), entityType, "Unsupported audit entity type.")
    };
}