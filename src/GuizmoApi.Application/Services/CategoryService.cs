using Microsoft.EntityFrameworkCore;
using GuizmoApi.Application.DTOs;
using GuizmoApi.Application.Interfaces;
using GuizmoApi.Domain.Entities;
using GuizmoApi.Infrastructure.Data;

namespace GuizmoApi.Application.Services;

public class CategoryService : ICategoryService
{
    private readonly AppDbContext _context;

    public CategoryService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<CategoryDto> CreateAsync(CreateCategoryRequest request, CancellationToken ct = default)
    {
        var entity = new Category
        {
            Name = request.Name
        };
        _context.Categories.Add(entity);
        await _context.SaveChangesAsync(ct);
        return new CategoryDto(entity.Id, entity.Name);
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken ct = default)
    {
        var entity = await _context.Categories.FindAsync([id], ct);
        if (entity is null) return false;

        _context.Categories.Remove(entity);
        await _context.SaveChangesAsync(ct);
        return true;

    }

    public async Task<IEnumerable<CategoryDto>> GetAllAsync(CancellationToken ct = default)
    {
        var categories = await _context.Categories.AsNoTracking().ToListAsync(ct);
        return categories.Select(c => new CategoryDto(c.Id, c.Name));
    }

    public async Task<CategoryDto?> UpdateAsync(int id, UpdateCategoryRequest request, CancellationToken ct = default)
    {
        var entity = await _context.Categories.FindAsync([id], ct);
        if (entity is null) return null;

        entity.Name = request.Name;
        _context.Categories.Update(entity);
        await _context.SaveChangesAsync(ct);
        return new CategoryDto(entity.Id, entity.Name);
    }
}
