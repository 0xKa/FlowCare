using System.Text.Json.Serialization;

namespace FlowCare.Infrastructure.Data.Seed;

public class SeedData
{
    [JsonPropertyName("users")]
    public SeedUsers Users { get; set; } = new();

    [JsonPropertyName("branches")]
    public List<SeedBranch> Branches { get; set; } = [];

    [JsonPropertyName("service_types")]
    public List<SeedServiceType> ServiceTypes { get; set; } = [];

    [JsonPropertyName("staff_service_types")]
    public List<SeedStaffServiceType> StaffServiceTypes { get; set; } = [];

    [JsonPropertyName("slots")]
    public List<SeedSlot> Slots { get; set; } = [];

    [JsonPropertyName("appointments")]
    public List<SeedAppointment> Appointments { get; set; } = [];

    [JsonPropertyName("audit_logs")]
    public List<SeedAuditLog> AuditLogs { get; set; } = [];
}

public class SeedUsers
{
    [JsonPropertyName("admin")]
    public List<SeedUser> Admin { get; set; } = [];

    [JsonPropertyName("branch_managers")]
    public List<SeedUser> BranchManagers { get; set; } = [];

    [JsonPropertyName("staff")]
    public List<SeedUser> Staff { get; set; } = [];

    [JsonPropertyName("customers")]
    public List<SeedUser> Customers { get; set; } = [];
}

public class SeedUser
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("username")]
    public string Username { get; set; } = string.Empty;

    [JsonPropertyName("password")]
    public string Password { get; set; } = string.Empty;

    [JsonPropertyName("role")]
    public string Role { get; set; } = string.Empty;

    [JsonPropertyName("full_name")]
    public string FullName { get; set; } = string.Empty;

    [JsonPropertyName("email")]
    public string Email { get; set; } = string.Empty;

    [JsonPropertyName("phone")]
    public string? Phone { get; set; }

    [JsonPropertyName("branch_id")]
    public string? BranchId { get; set; }

    [JsonPropertyName("is_active")]
    public bool IsActive { get; set; }
}

public class SeedBranch
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("city")]
    public string City { get; set; } = string.Empty;

    [JsonPropertyName("address")]
    public string Address { get; set; } = string.Empty;

    [JsonPropertyName("timezone")]
    public string Timezone { get; set; } = string.Empty;

    [JsonPropertyName("is_active")]
    public bool IsActive { get; set; }
}

public class SeedServiceType
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("branch_id")]
    public string BranchId { get; set; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;

    [JsonPropertyName("duration_minutes")]
    public int DurationMinutes { get; set; }

    [JsonPropertyName("is_active")]
    public bool IsActive { get; set; }
}

public class SeedStaffServiceType
{
    [JsonPropertyName("staff_id")]
    public string StaffId { get; set; } = string.Empty;

    [JsonPropertyName("service_type_id")]
    public string ServiceTypeId { get; set; } = string.Empty;
}

public class SeedSlot
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("branch_id")]
    public string BranchId { get; set; } = string.Empty;

    [JsonPropertyName("service_type_id")]
    public string ServiceTypeId { get; set; } = string.Empty;

    [JsonPropertyName("staff_id")]
    public string? StaffId { get; set; }

    [JsonPropertyName("start_at")]
    public DateTimeOffset StartAt { get; set; }

    [JsonPropertyName("end_at")]
    public DateTimeOffset EndAt { get; set; }

    [JsonPropertyName("capacity")]
    public int Capacity { get; set; }

    [JsonPropertyName("is_active")]
    public bool IsActive { get; set; }
}

public class SeedAppointment
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("customer_id")]
    public string CustomerId { get; set; } = string.Empty;

    [JsonPropertyName("branch_id")]
    public string BranchId { get; set; } = string.Empty;

    [JsonPropertyName("service_type_id")]
    public string ServiceTypeId { get; set; } = string.Empty;

    [JsonPropertyName("slot_id")]
    public string SlotId { get; set; } = string.Empty;

    [JsonPropertyName("staff_id")]
    public string? StaffId { get; set; }

    [JsonPropertyName("status")]
    public string Status { get; set; } = string.Empty;

    [JsonPropertyName("created_at")]
    public DateTimeOffset CreatedAt { get; set; }
}

public class SeedAuditLog
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("actor_id")]
    public string ActorId { get; set; } = string.Empty;

    [JsonPropertyName("actor_role")]
    public string ActorRole { get; set; } = string.Empty;

    [JsonPropertyName("action_type")]
    public string ActionType { get; set; } = string.Empty;

    [JsonPropertyName("entity_type")]
    public string EntityType { get; set; } = string.Empty;

    [JsonPropertyName("entity_id")]
    public string EntityId { get; set; } = string.Empty;

    [JsonPropertyName("timestamp")]
    public DateTimeOffset Timestamp { get; set; }

    [JsonPropertyName("metadata")]
    public Dictionary<string, object>? Metadata { get; set; }
}
