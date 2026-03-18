using FluentValidation.TestHelper;
using Microsoft.EntityFrameworkCore;
using GuizmoApi.Application.DTOs;
using GuizmoApi.Application.Validators;
using GuizmoApi.Domain.Entities;
using GuizmoApi.Infrastructure.Data;

namespace GuizmoApi.Application.Tests.Validators;

public class GuizmoRecommendedQueryValidatorTests
{
    private static AppDbContext CreateContext(string dbName) =>
        new(new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(dbName)
            .Options);

    [Fact]
    public async Task Should_pass_when_guizmoId_is_null()
    {
        await using var context = CreateContext(nameof(Should_pass_when_guizmoId_is_null));
        var validator = new GuizmoRecommendedQueryValidator(context);

        var result = await validator.TestValidateAsync(new GuizmoRecommendedQuery(null));

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public async Task Should_pass_when_guizmoId_is_positive_and_exists_in_db()
    {
        await using var context = CreateContext(nameof(Should_pass_when_guizmoId_is_positive_and_exists_in_db));
        var category = new Category { Name = "Electronics" };
        context.Categories.Add(category);
        await context.SaveChangesAsync();
        context.Guizmos.Add(new Guizmo { Name = "Widget", Manufacturer = "Acme", Msrp = 1m, CategoryId = category.Id });
        await context.SaveChangesAsync();
        var id = context.Guizmos.First().Id;

        var validator = new GuizmoRecommendedQueryValidator(context);
        var result = await validator.TestValidateAsync(new GuizmoRecommendedQuery(id));

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task Should_fail_when_guizmoId_is_not_positive(int guizmoId)
    {
        await using var context = CreateContext($"{nameof(Should_fail_when_guizmoId_is_not_positive)}_{guizmoId}");
        var validator = new GuizmoRecommendedQueryValidator(context);

        var result = await validator.TestValidateAsync(new GuizmoRecommendedQuery(guizmoId));

        result.ShouldHaveValidationErrorFor(x => x.GuizmoId);
    }

    [Fact]
    public async Task Should_fail_when_guizmoId_does_not_exist_in_db()
    {
        await using var context = CreateContext(nameof(Should_fail_when_guizmoId_does_not_exist_in_db));
        var validator = new GuizmoRecommendedQueryValidator(context);

        var result = await validator.TestValidateAsync(new GuizmoRecommendedQuery(99999));

        result.ShouldHaveValidationErrorFor(x => x.GuizmoId);
    }
}
