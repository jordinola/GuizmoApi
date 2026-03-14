using Microsoft.EntityFrameworkCore;
using GuizmoApi.Application.DTOs;
using GuizmoApi.Application.Interfaces;
using GuizmoApi.Domain.Entities;
using GuizmoApi.Infrastructure.Data;

namespace GuizmoApi.Application.Services;

public class GuizmoService : IGuizmoService
{
    private readonly AppDbContext _context;

    public GuizmoService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<GuizmoDto>> GetAllAsync(CancellationToken ct = default)
    {
        var guizmos = await _context.Guizmos.AsNoTracking().ToListAsync(ct);
        return guizmos.Select(ToDto);
    }

    public async Task<GuizmoDto?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        var guizmo = await _context.Guizmos.AsNoTracking()
            .FirstOrDefaultAsync(g => g.Id == id, ct);
        return guizmo is null ? null : ToDto(guizmo);
    }

    public async Task<GuizmoDto> CreateAsync(CreateGuizmoRequest request, CancellationToken ct = default)
    {
        var entity = new Guizmo
        {
            Name = request.Name,
            Manufacturer = request.Manufacturer,
            Description = request.Description,
            Msrp = request.Msrp
        };
        _context.Guizmos.Add(entity);
        await _context.SaveChangesAsync(ct);
        return ToDto(entity);
    }

    public async Task<GuizmoDto?> UpdateAsync(int id, UpdateGuizmoRequest request, CancellationToken ct = default)
    {
        var entity = await _context.Guizmos.FindAsync([id], ct);
        if (entity is null) return null;

        entity.Name = request.Name;
        entity.Manufacturer = request.Manufacturer;
        entity.Description = request.Description;
        entity.Msrp = request.Msrp;

        await _context.SaveChangesAsync(ct);
        return ToDto(entity);
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken ct = default)
    {
        var entity = await _context.Guizmos.FindAsync([id], ct);
        if (entity is null) return false;

        _context.Guizmos.Remove(entity);
        await _context.SaveChangesAsync(ct);
        return true;
    }

    private static GuizmoDto ToDto(Guizmo g) =>
        new(g.Id, g.Name, g.Manufacturer, g.Description, g.Msrp);
}
