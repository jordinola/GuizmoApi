namespace GuizmoApi.Application.DTOs;

public record GuizmoDto(
    int Id,
    string Name,
    string Manufacturer,
    string? Description,
    decimal Msrp,
    int CategoryId,
    string CategoryName
);
