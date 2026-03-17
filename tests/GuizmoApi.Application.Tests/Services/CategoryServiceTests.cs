using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using GuizmoApi.Application.DTOs;
using GuizmoApi.Application.Services;
using GuizmoApi.Domain.Entities;
using GuizmoApi.Infrastructure.Data;

namespace GuizmoApi.Application.Tests.Services;

public class CategoryServiceTests
{
    private static AppDbContext CreateContext(string dbName) =>
        new(new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase($"Category_{dbName}")
            .Options);

    [Fact]
    public async Task GetAllAsync_returns_all_categories_as_dtos()
    {
        await using var context = CreateContext(nameof(GetAllAsync_returns_all_categories_as_dtos));
        context.Categories.AddRange(
            new Category { Name = "Electronics" },
            new Category { Name = "Toys" });
        await context.SaveChangesAsync();

        var service = new CategoryService(context);
        var result = (await service.GetAllAsync()).ToList();

        result.Should().HaveCount(2);
        result.Select(c => c.Name).Should().BeEquivalentTo(["Electronics", "Toys"]);
    }

    [Fact]
    public async Task GetAllAsync_returns_empty_when_no_categories_exist()
    {
        await using var context = CreateContext(nameof(GetAllAsync_returns_empty_when_no_categories_exist));

        var service = new CategoryService(context);
        var result = await service.GetAllAsync();

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task CreateAsync_persists_and_returns_dto_with_id()
    {
        await using var context = CreateContext(nameof(CreateAsync_persists_and_returns_dto_with_id));

        var service = new CategoryService(context);
        var result = await service.CreateAsync(new CreateCategoryRequest("Electronics"));

        result.Id.Should().BeGreaterThan(0);
        result.Name.Should().Be("Electronics");
        context.Categories.Should().HaveCount(1);
    }

    [Fact]
    public async Task UpdateAsync_modifies_existing_record()
    {
        await using var context = CreateContext(nameof(UpdateAsync_modifies_existing_record));
        context.Categories.Add(new Category { Name = "Old Name" });
        await context.SaveChangesAsync();
        var id = context.Categories.First().Id;

        var service = new CategoryService(context);
        var result = await service.UpdateAsync(id, new UpdateCategoryRequest("Updated Name"));

        result.Should().NotBeNull();
        result!.Id.Should().Be(id);
        result.Name.Should().Be("Updated Name");
    }

    [Fact]
    public async Task UpdateAsync_returns_null_when_not_found()
    {
        await using var context = CreateContext(nameof(UpdateAsync_returns_null_when_not_found));

        var service = new CategoryService(context);
        var result = await service.UpdateAsync(999, new UpdateCategoryRequest("Updated"));

        result.Should().BeNull();
    }

    [Fact]
    public async Task DeleteAsync_removes_record_and_returns_true()
    {
        await using var context = CreateContext(nameof(DeleteAsync_removes_record_and_returns_true));
        context.Categories.Add(new Category { Name = "Electronics" });
        await context.SaveChangesAsync();
        var id = context.Categories.First().Id;

        var service = new CategoryService(context);
        var deleted = await service.DeleteAsync(id);

        deleted.Should().BeTrue();
        context.Categories.Should().BeEmpty();
    }

    [Fact]
    public async Task DeleteAsync_returns_false_when_not_found()
    {
        await using var context = CreateContext(nameof(DeleteAsync_returns_false_when_not_found));

        var service = new CategoryService(context);
        var result = await service.DeleteAsync(999);

        result.Should().BeFalse();
    }
}
