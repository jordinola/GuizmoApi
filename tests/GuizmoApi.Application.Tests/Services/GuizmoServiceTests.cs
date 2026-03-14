using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using GuizmoApi.Application.DTOs;
using GuizmoApi.Application.Services;
using GuizmoApi.Domain.Entities;
using GuizmoApi.Infrastructure.Data;

namespace GuizmoApi.Application.Tests.Services;

public class GuizmoServiceTests
{
    private static AppDbContext CreateContext(string dbName) =>
        new(new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(dbName)
            .Options);

    [Fact]
    public async Task GetAllAsync_returns_all_mapped_dtos()
    {
        await using var context = CreateContext(nameof(GetAllAsync_returns_all_mapped_dtos));
        context.Guizmos.Add(new Guizmo { Name = "Widget", Manufacturer = "Acme", Description = "Nice", Msrp = 9.99m });
        await context.SaveChangesAsync();

        var service = new GuizmoService(context);
        var result = (await service.GetAllAsync()).ToList();

        result.Should().HaveCount(1);
        result[0].Name.Should().Be("Widget");
        result[0].Msrp.Should().Be(9.99m);
    }

    [Fact]
    public async Task GetByIdAsync_returns_dto_when_found()
    {
        await using var context = CreateContext(nameof(GetByIdAsync_returns_dto_when_found));
        context.Guizmos.Add(new Guizmo { Name = "Widget", Manufacturer = "Acme", Msrp = 9.99m });
        await context.SaveChangesAsync();
        var id = context.Guizmos.First().Id;

        var service = new GuizmoService(context);
        var result = await service.GetByIdAsync(id);

        result.Should().NotBeNull();
        result!.Id.Should().Be(id);
    }

    [Fact]
    public async Task GetByIdAsync_returns_null_when_not_found()
    {
        await using var context = CreateContext(nameof(GetByIdAsync_returns_null_when_not_found));

        var service = new GuizmoService(context);
        var result = await service.GetByIdAsync(999);

        result.Should().BeNull();
    }

    [Fact]
    public async Task CreateAsync_persists_and_returns_dto_with_id()
    {
        await using var context = CreateContext(nameof(CreateAsync_persists_and_returns_dto_with_id));

        var service = new GuizmoService(context);
        var result = await service.CreateAsync(new CreateGuizmoRequest("Widget", "Acme", "Desc", 9.99m));

        result.Id.Should().BeGreaterThan(0);
        result.Name.Should().Be("Widget");
        context.Guizmos.Should().HaveCount(1);
    }

    [Fact]
    public async Task UpdateAsync_modifies_existing_record()
    {
        await using var context = CreateContext(nameof(UpdateAsync_modifies_existing_record));
        context.Guizmos.Add(new Guizmo { Name = "Old", Manufacturer = "OldMfg", Msrp = 1m });
        await context.SaveChangesAsync();
        var id = context.Guizmos.First().Id;

        var service = new GuizmoService(context);
        var result = await service.UpdateAsync(id, new UpdateGuizmoRequest("New", "NewMfg", null, 2m));

        result.Should().NotBeNull();
        result!.Name.Should().Be("New");
        result.Msrp.Should().Be(2m);
    }

    [Fact]
    public async Task UpdateAsync_returns_null_when_not_found()
    {
        await using var context = CreateContext(nameof(UpdateAsync_returns_null_when_not_found));

        var service = new GuizmoService(context);
        var result = await service.UpdateAsync(999, new UpdateGuizmoRequest("X", "Y", null, 1m));

        result.Should().BeNull();
    }

    [Fact]
    public async Task DeleteAsync_removes_record_and_returns_true()
    {
        await using var context = CreateContext(nameof(DeleteAsync_removes_record_and_returns_true));
        context.Guizmos.Add(new Guizmo { Name = "Widget", Manufacturer = "Acme", Msrp = 1m });
        await context.SaveChangesAsync();
        var id = context.Guizmos.First().Id;

        var service = new GuizmoService(context);
        var deleted = await service.DeleteAsync(id);

        deleted.Should().BeTrue();
        context.Guizmos.Should().BeEmpty();
    }

    [Fact]
    public async Task DeleteAsync_returns_false_when_not_found()
    {
        await using var context = CreateContext(nameof(DeleteAsync_returns_false_when_not_found));

        var service = new GuizmoService(context);
        var result = await service.DeleteAsync(999);

        result.Should().BeFalse();
    }
}
