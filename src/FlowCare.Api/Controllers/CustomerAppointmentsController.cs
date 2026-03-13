using FlowCare.Application.DTOs;
using FlowCare.Api.CustomWebModels;
using FlowCare.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FlowCare.Api.Controllers;

[ApiController]
[Route("api/customer/appointments")]
[Authorize(Policy = "CustomerOnly")]
public class CustomerAppointmentsController(
    IAppointmentService appointmentService,
    IAppointmentAttachmentService attachmentService,
    IBranchAuthorizationService branchAuth,
    IFileStorageService fileStorage) : ControllerBase
{
    private const long MaxAttachmentSize = 5 * 1024 * 1024;


    [HttpPost]
    [Consumes("multipart/form-data")]
    public async Task<ActionResult<AppointmentResponse>> Book([FromForm] BookAppointmentFormRequest request)
    {
        var customerId = branchAuth.GetUserId(User);

        Stream? attachmentStream = null;
        string? attachmentFileName = null;

        if (request.Attachment is not null)
        {
            if (request.Attachment.Length > MaxAttachmentSize)
                return BadRequest(new { error = "Attachment must be under 5 MB." });

            attachmentStream = request.Attachment.OpenReadStream();
            attachmentFileName = request.Attachment.FileName;

            if (!fileStorage.IsValidAttachment(attachmentStream))
            {
                await attachmentStream.DisposeAsync();
                return BadRequest(new { error = "Attachment must be a valid JPEG, PNG, or PDF file." });
            }
        }

        var appRequest = new BookAppointmentRequest(request.BranchId, request.ServiceTypeId, request.SlotId);

        try
        {
            var (result, error) = await appointmentService.BookAsync(
                customerId, appRequest, attachmentStream, attachmentFileName);

            if (error is not null)
                return BadRequest(new { error });

            return Created($"/api/customer/appointments/{result!.Id}", result);
        }
        finally
        {
            if (attachmentStream is not null)
                await attachmentStream.DisposeAsync();
        }
    }

    [HttpGet]
    public async Task<ActionResult<PagedResponse<AppointmentResponse>>> ListMyAppointments(
        [FromQuery] PagedSearchQueryRequest query)
    {
        var customerId = branchAuth.GetUserId(User);
        return Ok(await appointmentService.ListByCustomerAsync(customerId, query.Page, query.Size, query.SearchTerm));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<AppointmentResponse>> GetMyAppointment(string id)
    {
        var customerId = branchAuth.GetUserId(User);
        var appointment = await appointmentService.GetByIdForCustomerAsync(id, customerId);
        if (appointment is null)
            return NotFound(new { error = "Appointment not found." });

        return Ok(appointment);
    }

    [HttpGet("{id}/attachment")]
    public async Task<IActionResult> GetMyAppointmentAttachment(string id)
    {
        var file = await attachmentService.GetAttachmentAsync(id, User);
        if (file is null)
            return NotFound(new { error = "Attachment not found." });

        return File(file.Value.Stream, file.Value.ContentType, file.Value.FileName);
    }

    [HttpPut("{id}/cancel")]
    public async Task<IActionResult> CancelMyAppointment(string id)
    {
        var customerId = branchAuth.GetUserId(User);
        var role = branchAuth.GetRole(User);
        var (success, error) = await appointmentService.CancelAsync(id, customerId, role);

        if (!success)
            return BadRequest(new { error });

        return Ok(new { message = "Appointment cancelled." });
    }

    [HttpPut("{id}/reschedule")]
    public async Task<ActionResult<AppointmentResponse>> RescheduleMyAppointment(
        string id,
        [FromBody] RescheduleAppointmentRequest request)
    {
        var customerId = branchAuth.GetUserId(User);
        var role = branchAuth.GetRole(User);
        var (result, error) = await appointmentService.RescheduleAsync(
            id, customerId, request.NewSlotId, role);

        if (error is not null)
            return BadRequest(new { error });

        return Ok(result);
    }
}