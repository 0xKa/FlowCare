using FlowCare.Application.DTOs;
using FlowCare.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FlowCare.Api.Controllers;

[ApiController]
[Route("api/settings")]
[Authorize(Policy = "AdminOnly")]
public class SettingsController(
    IAdminMaintenanceService adminMaintenanceService,
    IBranchAuthorizationService branchAuth) : ControllerBase
{
    [HttpPut("retention-period")]
    public async Task<ActionResult<SoftDeleteSettingsResponse>> UpdateRetentionPeriod(
        [FromBody] UpdateRetentionPeriodRequest request)
    {
        var actorId = branchAuth.GetUserId(User);
        var actorRole = branchAuth.GetRole(User);

        var result = await adminMaintenanceService.SetRetentionDaysAsync(
            request.Days!.Value, actorId, actorRole);

        return Ok(result);
    }

    [HttpPut("rate-limits")]
    public async Task<ActionResult<RateLimitSettingsResponse>> UpdateRateLimits(
        [FromBody] UpdateRateLimitsRequest request)
    {
        var actorId = branchAuth.GetUserId(User);
        var actorRole = branchAuth.GetRole(User);

        var result = await adminMaintenanceService.SetRateLimitsAsync(
            request.CustomerBookingsPerDay!.Value,
            request.MaxReschedulesPerAppointment!.Value,
            actorId,
            actorRole);

        return Ok(result);
    }
}
