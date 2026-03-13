namespace FlowCare.Application.DTOs;

public record BranchResponse
{
    public string Id { get; init; } = null!;
    public string Name { get; init; } = null!;
    public string City { get; init; } = null!;
    public string Address { get; init; } = null!;
    public string Timezone { get; init; } = null!;
    public bool IsActive { get; init; }

    public BranchResponse(string id, string name, string city, string address, string timezone, bool isActive)
    {
        Id = id;
        Name = name;
        City = city;
        Address = address;
        Timezone = timezone;
        IsActive = isActive;
    }
}

public record ServiceTypeResponse
{
    public string Id { get; init; } = null!;
    public string BranchId { get; init; } = null!;
    public string Name { get; init; } = null!;
    public string Description { get; init; } = null!;
    public int DurationMinutes { get; init; }
    public bool IsActive { get; init; }

    public ServiceTypeResponse(
        string id,
        string branchId,
        string name,
        string description,
        int durationMinutes,
        bool isActive)
    {
        Id = id;
        BranchId = branchId;
        Name = name;
        Description = description;
        DurationMinutes = durationMinutes;
        IsActive = isActive;
    }
}

public record SlotResponse
{
    public string Id { get; init; } = null!;
    public string BranchId { get; init; } = null!;
    public string ServiceTypeId { get; init; } = null!;
    public string? StaffId { get; init; }
    public string? StaffName { get; init; }
    public DateTimeOffset StartAt { get; init; }
    public DateTimeOffset EndAt { get; init; }
    public int Capacity { get; init; }
    public bool IsAvailable { get; init; }

    public SlotResponse(
        string id,
        string branchId,
        string serviceTypeId,
        string? staffId,
        string? staffName,
        DateTimeOffset startAt,
        DateTimeOffset endAt,
        int capacity,
        bool isAvailable)
    {
        Id = id;
        BranchId = branchId;
        ServiceTypeId = serviceTypeId;
        StaffId = staffId;
        StaffName = staffName;
        StartAt = startAt;
        EndAt = endAt;
        Capacity = capacity;
        IsAvailable = isAvailable;
    }
}

public record QueueEntryResponse
{
    public int QueueNumber { get; init; }
    public string AppointmentId { get; init; } = null!;
    public string CustomerId { get; init; } = null!;
    public string? CustomerName { get; init; }
    public string ServiceTypeId { get; init; } = null!;
    public string? ServiceTypeName { get; init; }
    public string? SlotId { get; init; }
    public DateTimeOffset? SlotStartAt { get; init; }
    public DateTimeOffset CheckedInAt { get; init; }

    public QueueEntryResponse(
        int queueNumber,
        string appointmentId,
        string customerId,
        string? customerName,
        string serviceTypeId,
        string? serviceTypeName,
        string? slotId,
        DateTimeOffset? slotStartAt,
        DateTimeOffset checkedInAt)
    {
        QueueNumber = queueNumber;
        AppointmentId = appointmentId;
        CustomerId = customerId;
        CustomerName = customerName;
        ServiceTypeId = serviceTypeId;
        ServiceTypeName = serviceTypeName;
        SlotId = slotId;
        SlotStartAt = slotStartAt;
        CheckedInAt = checkedInAt;
    }
}

public record LiveQueueResponse
{
    public string BranchId { get; init; } = null!;
    public int TotalCheckedIn { get; init; }
    public List<QueueEntryResponse> Entries { get; init; }

    public LiveQueueResponse(string branchId, int totalCheckedIn, List<QueueEntryResponse> entries)
    {
        BranchId = branchId;
        TotalCheckedIn = totalCheckedIn;
        Entries = entries;
    }
}
