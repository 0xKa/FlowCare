using FlowCare.Application.DTOs;
using FlowCare.Application.Interfaces;
using FlowCare.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FlowCare.Api.Controllers;

[ApiController]
[Route("api/branches/{branchId}/slots")]
[Authorize(Policy = "ManagerOrAdmin")]
public class BranchSlotsController(
    ISlotService slotService,
    IBranchAuthorizationService branchAuth) : ControllerBase
{
    /// <summary>
    /// List all slots for a branch. Admin can include soft-deleted slots with ?includeDeleted=true.
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<List<SlotDetailResponse>>> ListSlots(
        string branchId, [FromQuery] bool includeDeleted = false)
    {
        var role = branchAuth.GetRole(User);
        var actorBranchId = branchAuth.GetBranchId(User);

        return Ok(await slotService.ListSlotsAsync(branchId, role, actorBranchId, includeDeleted));
    }

    /// <summary>
    /// Create a single slot for a branch.
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<SlotDetailResponse>> CreateSlot(
        string branchId, [FromBody] CreateSlotRequest request)
    {
        var actorId = branchAuth.GetUserId(User);
        var actorRole = branchAuth.GetRole(User);

        if (actorRole == nameof(UserRole.BranchManager) && !branchAuth.CanAccessBranch(User, branchId))
            return Forbid();

        var (result, error) = await slotService.CreateSlotAsync(branchId, request, actorId, actorRole);
        if (error is not null)
            return BadRequest(new { error });

        return Created($"/api/branches/{branchId}/slots", result);
    }

    /// <summary>
    /// Create multiple slots for a branch in bulk.
    /// </summary>
    [HttpPost("bulk")]
    public async Task<ActionResult<List<SlotDetailResponse>>> CreateBulkSlots(
        string branchId, [FromBody] CreateBulkSlotsRequest request)
    {
        var actorId = branchAuth.GetUserId(User);
        var actorRole = branchAuth.GetRole(User);

        if (actorRole == nameof(UserRole.BranchManager) && !branchAuth.CanAccessBranch(User, branchId))
            return Forbid();

        var (results, error) = await slotService.CreateBulkSlotsAsync(
            branchId, request.Slots, actorId, actorRole);
        if (error is not null)
            return BadRequest(new { error });

        return Created($"/api/branches/{branchId}/slots", results);
    }
}
