namespace FlowCare.Application.DTOs;

public record CustomerResponse(
    string Id,
    string Username,
    string FullName,
    string Email,
    string? Phone,
    bool IsActive,
    bool HasIdImage,
    DateTimeOffset CreatedAt);
