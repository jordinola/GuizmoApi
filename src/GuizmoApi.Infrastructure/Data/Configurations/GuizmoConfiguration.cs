using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using GuizmoApi.Domain.Entities;

namespace GuizmoApi.Infrastructure.Data.Configurations;

public class GuizmoConfiguration : IEntityTypeConfiguration<Guizmo>
{
    public void Configure(EntityTypeBuilder<Guizmo> builder)
    {
        builder.HasKey(g => g.Id);

        builder.Property(g => g.Id)
            .UseIdentityColumn();

        builder.Property(g => g.Name)
            .IsRequired()
            .HasMaxLength(250);

        builder.Property(g => g.Manufacturer)
            .IsRequired()
            .HasMaxLength(250);

        builder.Property(g => g.Description)
            .HasColumnType("text");

        builder.Property(g => g.Msrp)
            .HasColumnType("decimal(8,2)")
            .IsRequired();

        builder.Property(g => g.CategoryId).IsRequired();

        builder.HasOne(g => g.Category)
            .WithMany(c => c.Guizmos)
            .HasForeignKey(g => g.CategoryId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
