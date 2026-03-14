using Microsoft.EntityFrameworkCore;
using GuizmoApi.Domain.Entities;
using GuizmoApi.Infrastructure.Data.Configurations;

namespace GuizmoApi.Infrastructure.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Guizmo> Guizmos => Set<Guizmo>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new GuizmoConfiguration());
    }
}
