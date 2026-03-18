namespace GuizmoApi.Application.DTOs;

public record GuizmoPagedQuery(
    int PageNumber = 1,
    int PageSize = 10,
    string SortOrder = "asc"
);
