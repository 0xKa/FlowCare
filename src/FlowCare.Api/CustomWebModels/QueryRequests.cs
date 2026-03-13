using System.ComponentModel.DataAnnotations;

namespace FlowCare.Api.CustomWebModels;

public class PagedSearchQueryRequest
{
    [Range(1, int.MaxValue)]
    public int Page { get; init; } = 1;

    [Range(1, 200)]
    public int Size { get; init; } = 20;

    public string? SearchTerm { get; init; }
}

public class PublicSlotsQueryRequest : PagedSearchQueryRequest
{
    public DateOnly? Date { get; init; }
}

public class BranchSlotsQueryRequest : PagedSearchQueryRequest
{
    public bool IncludeDeleted { get; init; } = false;
}
