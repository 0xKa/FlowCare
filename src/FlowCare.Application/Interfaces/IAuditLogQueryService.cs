using FlowCare.Application.DTOs;

namespace FlowCare.Application.Interfaces;

public interface IAuditLogQueryService
{
    Task<List<AuditLogResponse>> ListAsync(string actorRole, string? actorBranchId);
    Task<Stream> ExportCsvAsync();
}
