using Microsoft.EntityFrameworkCore;
using GuizmoApi.Domain.Entities;
using GuizmoApi.Infrastructure.Data.Configurations;

namespace GuizmoApi.Infrastructure.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Guizmo> Guizmos => Set<Guizmo>();
    public DbSet<Category> Categories => Set<Category>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new GuizmoConfiguration());
        modelBuilder.ApplyConfiguration(new CategoryConfiguration());
    }
}
