namespace GuizmoApi.Application.DTOs;

public record CreateGuizmoRequest(
    string Name,
    string Manufacturer,
    string? Description,
    decimal Msrp,
    int CategoryId
);
