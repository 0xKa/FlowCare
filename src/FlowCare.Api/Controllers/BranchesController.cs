using FlowCare.Application.DTOs;
using FlowCare.Application.Interfaces;
using FlowCare.Api.CustomWebModels;
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
        [FromQuery] PagedSearchQueryRequest query)
    {
        return Ok(await branchService.ListBranchesAsync(query.Page, query.Size, query.SearchTerm));
    }

    /// <summary>
    /// List active service types for a branch.
    /// </summary>
    [HttpGet("{branchId}/services")]
    public async Task<ActionResult<PagedResponse<ServiceTypeResponse>>> ListServices(
        string branchId,
        [FromQuery] PagedSearchQueryRequest query)
    {
        var result = await branchService.ListServicesAsync(branchId, query.Page, query.Size, query.SearchTerm);
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
        [FromQuery] PublicSlotsQueryRequest query)
    {
        var result = await branchService.ListAvailableSlotsAsync(
            branchId,
            serviceTypeId,
            query.Date,
            query.Page,
            query.Size,
            query.SearchTerm);
        if (result is null)
            return NotFound(new { error = "Branch or service type not found." });

        return Ok(result);
    }

    /// <summary>
    /// Get live queue for a branch based on checked-in appointments ordered by slot start time.
    /// </summary>
    [HttpGet("{branchId}/queue")]
    public async Task<ActionResult<LiveQueueResponse>> GetLiveQueue(string branchId)
    {
        var result = await branchService.GetLiveQueueAsync(branchId);
        if (result is null)
            return NotFound(new { error = "Branch not found." });

        return Ok(result);
    }
}
