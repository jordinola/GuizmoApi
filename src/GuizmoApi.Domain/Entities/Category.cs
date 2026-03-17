using GuizmoApi.Domain.Entities;

public class Category
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;

    public ICollection<Guizmo> Guizmos { get; set; } = new List<Guizmo>();
}