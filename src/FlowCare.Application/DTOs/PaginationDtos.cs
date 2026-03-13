namespace FlowCare.Application.DTOs;

public record PaginationMetadata
{
    public int TotalCount { get; init; }
    public int TotalPages { get; init; }
    public int CurrentPage { get; init; }
    public int PageSize { get; init; }
    public bool HasPreviousPage { get; init; }
    public bool HasNextPage { get; init; }

    public PaginationMetadata(
        int totalCount,
        int totalPages,
        int currentPage,
        int pageSize,
        bool hasPreviousPage,
        bool hasNextPage)
    {
        TotalCount = totalCount;
        TotalPages = totalPages;
        CurrentPage = currentPage;
        PageSize = pageSize;
        HasPreviousPage = hasPreviousPage;
        HasNextPage = hasNextPage;
    }
}

public record PagedResponse<T>
{
    public List<T> Data { get; init; }
    public PaginationMetadata Meta { get; init; }

    public PagedResponse(List<T> data, PaginationMetadata meta)
    {
        Data = data;
        Meta = meta;
    }

    public static PagedResponse<T> Create(List<T> data, int totalCount, int currentPage, int pageSize)
    {
        var normalizedPageSize = pageSize < 1 ? 1 : pageSize;
        var normalizedCurrentPage = currentPage < 1 ? 1 : currentPage;
        var totalPages = totalCount == 0 ? 0 : (int)Math.Ceiling(totalCount / (double)normalizedPageSize);

        var meta = new PaginationMetadata(
            totalCount,
            totalPages,
            normalizedCurrentPage,
            normalizedPageSize,
            normalizedCurrentPage > 1,
            normalizedCurrentPage < totalPages);

        return new PagedResponse<T>(data, meta);
    }
}
