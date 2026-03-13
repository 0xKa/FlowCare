using FlowCare.Application.DTOs;
using FlowCare.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FlowCare.Api.Controllers;

[ApiController]
[Route("api/branches")]
[AllowAnonymous]
public class BranchesController(IBranchService branchService) : ControllerBase
{
    /// <summary>
    /// List all active branches.
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<PagedResponse<BranchResponse>>> ListBranches(
        [FromQuery] int page = 1,
        [FromQuery] int size = 20,
        [FromQuery] string? term = null)
    {
        return Ok(await branchService.ListBranchesAsync(page, size, term));
    }

    /// <summary>
    /// List active service types for a branch.
    /// </summary>
    [HttpGet("{branchId}/services")]
    public async Task<ActionResult<PagedResponse<ServiceTypeResponse>>> ListServices(
        string branchId,
        [FromQuery] int page = 1,
        [FromQuery] int size = 20,
        [FromQuery] string? term = null)
    {
        var result = await branchService.ListServicesAsync(branchId, page, size, term);
        if (result is null)
            return NotFound(new { error = "Branch not found." });

        return Ok(result);
    }

    /// <summary>
    /// List available slots for a branch and service type, with optional date filter.
    /// </summary>
    [HttpGet("{branchId}/services/{serviceTypeId}/slots")]
    public async Task<ActionResult<PagedResponse<SlotResponse>>> ListAvailableSlots(
        string branchId,
        string serviceTypeId,
        [FromQuery] DateOnly? date,
        [FromQuery] int page = 1,
        [FromQuery] int size = 20,
        [FromQuery] string? term = null)
    {
        var result = await branchService.ListAvailableSlotsAsync(
            branchId,
            serviceTypeId,
            date,
            page,
            size,
            term);
        if (result is null)
            return NotFound(new { error = "Branch or service type not found." });

        return Ok(result);
    }
}
