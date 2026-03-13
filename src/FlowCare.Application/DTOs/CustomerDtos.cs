namespace FlowCare.Application.DTOs;

public record CustomerResponse
{
    public string Id { get; init; } = null!;
    public string Username { get; init; } = null!;
    public string FullName { get; init; } = null!;
    public string Email { get; init; } = null!;
    public string? Phone { get; init; }
    public bool IsActive { get; init; }
    public bool HasIdImage { get; init; }
    public DateTimeOffset CreatedAt { get; init; }

    public CustomerResponse(
        string id,
        string username,
        string fullName,
        string email,
        string? phone,
        bool isActive,
        bool hasIdImage,
        DateTimeOffset createdAt)
    {
        Id = id;
        Username = username;
        FullName = fullName;
        Email = email;
        Phone = phone;
        IsActive = isActive;
        HasIdImage = hasIdImage;
        CreatedAt = createdAt;
    }
}
