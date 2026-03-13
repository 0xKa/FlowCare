using FlowCare.Application.DTOs;
using FlowCare.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FlowCare.Api.Controllers;

[ApiController]
[Route("api/admin")]
[Authorize(Policy = "AdminOnly")]
public class AdminController(
    IAdminMaintenanceService adminMaintenanceService,
    IBranchAuthorizationService branchAuth) : ControllerBase
{
    [HttpPost("cleanup")]
    public async Task<ActionResult<CleanupResultResponse>> CleanupSoftDeletedSlots()
    {
        var actorId = branchAuth.GetUserId(User);
        var actorRole = branchAuth.GetRole(User);

        var result = await adminMaintenanceService.CleanupSoftDeletedSlotsAsync(actorId, actorRole);
        return Ok(result);
    }
}
