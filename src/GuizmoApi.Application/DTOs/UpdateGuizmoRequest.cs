namespace GuizmoApi.Application.DTOs;

public record UpdateGuizmoRequest(
    string Name,
    string Manufacturer,
    string? Description,
    decimal Msrp,
    int CategoryId
);
