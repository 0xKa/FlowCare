using FlowCare.Application.DTOs;

namespace FlowCare.Application.Interfaces;

public interface IAuditLogQueryService
{
    Task<PagedResponse<AuditLogResponse>> ListAsync(
        string actorRole,
        string? actorBranchId,
        int page,
        int size,
        string? term);
    Task<Stream> ExportCsvAsync();
}
