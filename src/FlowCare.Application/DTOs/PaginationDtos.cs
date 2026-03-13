namespace FlowCare.Application.DTOs;

public record PagedResponse<T>(
    List<T> Results,
    int Total);
