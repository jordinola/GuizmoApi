using FluentValidation.TestHelper;
using GuizmoApi.Application.DTOs;
using GuizmoApi.Application.Validators;

namespace GuizmoApi.Application.Tests.Validators;

public class UpdateGuizmoRequestValidatorTests
{
    private readonly UpdateGuizmoRequestValidator _validator = new();

    [Fact]
    public void Should_pass_when_all_fields_are_valid()
    {
        var req = new UpdateGuizmoRequest("Widget", "Acme", "A great widget", 9.99m, 1);
        var result = _validator.TestValidate(req);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void Should_fail_when_name_is_empty(string? name)
    {
        var req = new UpdateGuizmoRequest(name!, "Acme", null, 9.99m, 1);
        var result = _validator.TestValidate(req);
        result.ShouldHaveValidationErrorFor(x => x.Name);
    }

    [Fact]
    public void Should_fail_when_name_exceeds_250_chars()
    {
        var req = new UpdateGuizmoRequest(new string('A', 251), "Acme", null, 9.99m, 1);
        var result = _validator.TestValidate(req);
        result.ShouldHaveValidationErrorFor(x => x.Name);
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void Should_fail_when_manufacturer_is_empty(string? manufacturer)
    {
        var req = new UpdateGuizmoRequest("Widget", manufacturer!, null, 9.99m, 1);
        var result = _validator.TestValidate(req);
        result.ShouldHaveValidationErrorFor(x => x.Manufacturer);
    }

    [Fact]
    public void Should_fail_when_manufacturer_exceeds_250_chars()
    {
        var req = new UpdateGuizmoRequest("Widget", new string('A', 251), null, 9.99m, 1);
        var result = _validator.TestValidate(req);
        result.ShouldHaveValidationErrorFor(x => x.Manufacturer);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Should_fail_when_msrp_is_not_positive(decimal msrp)
    {
        var req = new UpdateGuizmoRequest("Widget", "Acme", null, msrp, 1);
        var result = _validator.TestValidate(req);
        result.ShouldHaveValidationErrorFor(x => x.Msrp);
    }

    [Fact]
    public void Description_is_optional()
    {
        var req = new UpdateGuizmoRequest("Widget", "Acme", null, 9.99m, 1);
        var result = _validator.TestValidate(req);
        result.ShouldNotHaveValidationErrorFor(x => x.Description);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Should_fail_when_category_id_is_not_positive(int categoryId)
    {
        var req = new UpdateGuizmoRequest("Widget", "Acme", null, 9.99m, categoryId);
        var result = _validator.TestValidate(req);
        result.ShouldHaveValidationErrorFor(x => x.CategoryId)
            .WithErrorMessage("CategoryId must be greater than zero.");
    }

    [Fact]
    public void Should_pass_when_category_id_is_valid()
    {
        var req = new UpdateGuizmoRequest("Widget", "Acme", null, 9.99m, 1);
        var result = _validator.TestValidate(req);
        result.ShouldNotHaveValidationErrorFor(x => x.CategoryId);
    }
}
