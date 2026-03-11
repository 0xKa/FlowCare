using FlowCare.Application.DTOs;
using FlowCare.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FlowCare.Api.Controllers;

[ApiController]
[Route("api/slots")]
[Authorize(Policy = "ManagerOrAdmin")]
public class SlotsController(
    ISlotService slotService,
    IBranchAuthorizationService branchAuth) : ControllerBase
{
    /// <summary>
    /// Update a slot.
    /// </summary>
    [HttpPut("{slotId}")]
    public async Task<ActionResult<SlotDetailResponse>> UpdateSlot(
        string slotId, [FromBody] UpdateSlotRequest request)
    {
        var actorId = branchAuth.GetUserId(User);
        var actorRole = branchAuth.GetRole(User);
        var actorBranchId = branchAuth.GetBranchId(User);

        var (result, error) = await slotService.UpdateSlotAsync(
            slotId, request, actorId, actorRole, actorBranchId);
        if (error is not null)
            return BadRequest(new { error });

        return Ok(result);
    }

    /// <summary>
    /// Soft-delete a slot.
    /// </summary>
    [HttpDelete("{slotId}")]
    public async Task<IActionResult> SoftDeleteSlot(string slotId)
    {
        var actorId = branchAuth.GetUserId(User);
        var actorRole = branchAuth.GetRole(User);
        var actorBranchId = branchAuth.GetBranchId(User);

        var (success, error) = await slotService.SoftDeleteSlotAsync(
            slotId, actorId, actorRole, actorBranchId);
        if (!success)
            return BadRequest(new { error });

        return Ok(new { message = "Slot soft-deleted." });
    }
}
