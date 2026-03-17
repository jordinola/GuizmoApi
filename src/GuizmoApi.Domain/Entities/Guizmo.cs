namespace GuizmoApi.Domain.Entities;

public class Guizmo
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Manufacturer { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal Msrp { get; set; }
    public int CategoryId { get; set; }

    public Category Category { get; set; } = null!;
}
