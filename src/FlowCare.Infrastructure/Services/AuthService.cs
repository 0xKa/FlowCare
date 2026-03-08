using FlowCare.Application.DTOs;
using FlowCare.Application.Interfaces;
using FlowCare.Domain.Entities;
using FlowCare.Domain.Enums;
using FlowCare.Infrastructure.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace FlowCare.Infrastructure.Services;

public class AuthService(FlowCareDbContext db, IFileStorageService fileStorage) : IAuthService
{
    private const long MaxImageSize = 5 * 1024 * 1024; // 5 MB

    public async Task<(LoginResponse? Result, string? Error, int StatusCode)> RegisterCustomerAsync(
        RegisterCustomerRequest request,
        Stream imageStream,
        string imageFileName,
        long imageLength)
    {
        if (imageLength == 0)
            return (null, "ID image is required.", StatusCodes.Status400BadRequest);

        if (imageLength > MaxImageSize)
            return (null, "ID image must be under 5 MB.", StatusCodes.Status400BadRequest);

        if (!fileStorage.IsValidImage(imageStream))
            return (null, "ID image must be a valid JPEG or PNG file.", StatusCodes.Status400BadRequest);

        if (await db.Users.AnyAsync(u => u.Username == request.Username))
            return (null, "Username already taken.", StatusCodes.Status409Conflict);

        if (await db.Users.AnyAsync(u => u.Email == request.Email))
            return (null, "Email already registered.", StatusCodes.Status409Conflict);

        var userId = $"usr_cust_{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}";
        var ext = Path.GetExtension(imageFileName).ToLowerInvariant();
        if (string.IsNullOrEmpty(ext)) ext = ".jpg";

        imageStream.Position = 0;
        var imagePath = await fileStorage.SaveFileAsync(imageStream, "id-images", $"{userId}{ext}");

        var user = new User
        {
            Id = userId,
            Username = request.Username,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            Role = UserRole.Customer,
            FullName = request.FullName,
            Email = request.Email,
            Phone = request.Phone,
            IsActive = true,
            IdImagePath = imagePath
        };

        db.Users.Add(user);
        await db.SaveChangesAsync();

        return (new LoginResponse(
            user.Id,
            user.Username,
            user.Role.ToString(),
            user.FullName,
            user.Email,
            user.Phone,
            user.BranchId), null, StatusCodes.Status201Created);
    }

    public async Task<(LoginResponse? Result, string? Error, int StatusCode)> LoginAsync(string userId)
    {
        var user = await db.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == userId);

        if (user is null)
            return (null, "User not found.", StatusCodes.Status401Unauthorized);

        return (new LoginResponse(
            user.Id,
            user.Username,
            user.Role.ToString(),
            user.FullName,
            user.Email,
            user.Phone,
            user.BranchId), null, StatusCodes.Status200OK);
    }
}
