using FlowCare.Application.DTOs;
using FlowCare.Application.Interfaces;
using FlowCare.Api.CustomWebModels;
using FlowCare.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FlowCare.Api.Controllers;

[ApiController]
[Route("api/staff/appointments")]
[Authorize(Policy = "StaffOrAbove")]
public class StaffAppointmentsController(
    IAppointmentService appointmentService,
    IAppointmentAttachmentService attachmentService,
    IBranchAuthorizationService branchAuth) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<PagedResponse<AppointmentResponse>>> ListAppointments(
        [FromQuery] PagedSearchQueryRequest query)
    {
        var role = branchAuth.GetRole(User);
        var userId = branchAuth.GetUserId(User);
        var branchId = branchAuth.GetBranchId(User);

        return Ok(await appointmentService.ListAsync(
            role,
            userId,
            branchId,
            query.Page,
            query.Size,
            query.SearchTerm));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<AppointmentResponse>> GetAppointment(string id)
    {
        var appointment = await appointmentService.GetByIdAsync(id);
        if (appointment is null)
            return NotFound(new { error = "Appointment not found." });

        var role = branchAuth.GetRole(User);
        if (role == nameof(UserRole.BranchManager))
        {
            var branchId = branchAuth.GetBranchId(User);
            if (appointment.BranchId != branchId)
                return Forbid();
        }
        else if (role == nameof(UserRole.Staff))
        {
            var userId = branchAuth.GetUserId(User);
            if (appointment.StaffId != userId)
                return Forbid();
        }

        return Ok(appointment);
    }

    [HttpPut("{id}/status")]
    public async Task<IActionResult> UpdateAppointmentStatus(
        string id,
        [FromBody] UpdateAppointmentStatusRequest request)
    {
        var actorId = branchAuth.GetUserId(User);
        var actorRole = branchAuth.GetRole(User);
        var actorBranchId = branchAuth.GetBranchId(User);

        var (success, error) = await appointmentService.UpdateStatusAsync(
            id, request.Status, actorId, actorRole, actorBranchId);

        if (!success)
            return BadRequest(new { error });

        return Ok(new { message = $"Appointment status updated to '{request.Status}'." });
    }

    [HttpGet("{id}/attachment")]
    public async Task<IActionResult> GetAppointmentAttachment(string id)
    {
        var file = await attachmentService.GetAttachmentAsync(id, User);
        if (file is null)
            return NotFound(new { error = "Attachment not found." });

        return File(file.Value.Stream, file.Value.ContentType, file.Value.FileName);
    }
}