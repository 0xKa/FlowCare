using FlowCare.Application.DTOs;
using FlowCare.Application.Interfaces;
using FlowCare.Api.CustomWebModels;
using FlowCare.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FlowCare.Api.Controllers;

[ApiController]
[Route("api/audit-logs")]
[Authorize(Policy = "ManagerOrAdmin")]
public class AuditLogsController(
    IAuditLogQueryService auditLogQuery,
    IBranchAuthorizationService branchAuth) : ControllerBase
{
    /// <summary>
    /// List audit logs. Admin: all. Manager: branch-scoped.
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<PagedResponse<AuditLogResponse>>> ListAuditLogs(
        [FromQuery] PagedSearchQueryRequest query)
    {
        var role = branchAuth.GetRole(User);
        var branchId = branchAuth.GetBranchId(User);

        return Ok(await auditLogQuery.ListAsync(role, branchId, query.Page, query.Size, query.SearchTerm));
    }

    /// <summary>
    /// Export all audit logs as a CSV file. Admin only.
    /// </summary>
    [HttpGet("export")]
    [Produces("text/csv")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> ExportAuditLogs()
    {
        var csvStream = await auditLogQuery.ExportCsvAsync();
        return File(csvStream, "text/csv", "audit-logs.csv");
    }
}
