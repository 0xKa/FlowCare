using FlowCare.Application.DTOs;
using FlowCare.Application.Interfaces;
using FlowCare.Api.CustomWebModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FlowCare.Api.Controllers;

[ApiController]
[Route("api/staff")]
[Authorize(Policy = "ManagerOrAdmin")]
public class StaffController(
    IStaffManagementService staffService,
    IBranchAuthorizationService branchAuth) : ControllerBase
{
    /// <summary>
    /// List staff members. Admin: all. Manager: branch-only.
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<PagedResponse<StaffResponse>>> ListStaff(
        [FromQuery] PagedSearchQueryRequest query)
    {
        var role = branchAuth.GetRole(User);
        var branchId = branchAuth.GetBranchId(User);

        return Ok(await staffService.ListStaffAsync(role, branchId, query.Page, query.Size, query.SearchTerm));
    }

    /// <summary>
    /// Assign staff to one or more service types.
    /// </summary>
    [HttpPost("{staffId}/services")]
    public async Task<IActionResult> AssignStaffToServices(
        string staffId, [FromBody] AssignStaffServicesRequest request)
    {
        var actorId = branchAuth.GetUserId(User);
        var actorRole = branchAuth.GetRole(User);
        var actorBranchId = branchAuth.GetBranchId(User);

        var (success, error) = await staffService.AssignStaffToServicesAsync(
            staffId, request.ServiceTypeIds, actorId, actorRole, actorBranchId);

        if (!success)
            return BadRequest(new { error });

        return Ok(new { message = "Staff assigned to services." });
    }

    /// <summary>
    /// Unassign staff from a service type.
    /// </summary>
    [HttpDelete("{staffId}/services/{serviceTypeId}")]
    public async Task<IActionResult> UnassignStaffFromService(string staffId, string serviceTypeId)
    {
        var actorId = branchAuth.GetUserId(User);
        var actorRole = branchAuth.GetRole(User);
        var actorBranchId = branchAuth.GetBranchId(User);

        var (success, error) = await staffService.UnassignStaffFromServiceAsync(
            staffId, serviceTypeId, actorId, actorRole, actorBranchId);

        if (!success)
            return BadRequest(new { error });

        return Ok(new { message = "Staff unassigned from service." });
    }
}
