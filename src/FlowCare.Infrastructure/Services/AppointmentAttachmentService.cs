using System.Security.Claims;
using FlowCare.Application.Interfaces;
using FlowCare.Domain.Enums;
using FlowCare.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace FlowCare.Infrastructure.Services;

public class AppointmentAttachmentService(
    FlowCareDbContext db,
    IBranchAuthorizationService branchAuth,
    IFileStorageService fileStorage) : IAppointmentAttachmentService
{
    public async Task<(Stream Stream, string ContentType, string FileName)?> GetAttachmentAsync(
        string appointmentId,
        ClaimsPrincipal user)
    {
        var role = branchAuth.GetRole(user);
        var userId = branchAuth.GetUserId(user);
        var branchId = branchAuth.GetBranchId(user);

        var query = db.Appointments
            .AsNoTracking()
            .Where(a => a.Id == appointmentId);

        if (role == nameof(UserRole.Customer))
            query = query.Where(a => a.CustomerId == userId);
        else if (role == nameof(UserRole.BranchManager))
            query = query.Where(a => a.BranchId == branchId);
        else if (role == nameof(UserRole.Staff))
            query = query.Where(a => a.StaffId == userId);
        else if (role != nameof(UserRole.Admin))
            return null;

        var attachmentPath = await query
            .Select(a => a.AttachmentPath)
            .FirstOrDefaultAsync();

        if (string.IsNullOrWhiteSpace(attachmentPath))
            return null;

        var fileInfo = fileStorage.GetFile(attachmentPath);
        if (fileInfo is null)
            return null;

        var (fullPath, contentType) = fileInfo.Value;
        var stream = new FileStream(
            fullPath,
            FileMode.Open,
            FileAccess.Read,
            FileShare.Read,
            bufferSize: 4096,
            options: FileOptions.Asynchronous);

        return (stream, contentType, Path.GetFileName(fullPath));
    }
}