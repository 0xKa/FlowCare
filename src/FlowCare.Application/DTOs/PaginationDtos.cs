namespace FlowCare.Application.DTOs;

public record PaginationMetadata(
    int TotalCount,
    int TotalPages,
    int CurrentPage,
    int PageSize,
    bool HasPreviousPage,
    bool HasNextPage);

public record PagedResponse<T>(
    List<T> Data,
    PaginationMetadata Meta)
{
    // static factory method to create a paged response
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
