using System.Security.Claims;

namespace FlowCare.Application.Interfaces;

public interface IAppointmentAttachmentService
{
    Task<(Stream Stream, string ContentType, string FileName)?> GetAttachmentAsync(
        string appointmentId,
        ClaimsPrincipal user);
}