using FluentValidation.TestHelper;
using GuizmoApi.Application.DTOs;
using GuizmoApi.Application.Validators;

namespace GuizmoApi.Application.Tests.Validators;

public class CreateGuizmoRequestValidatorTests
{
    private readonly CreateGuizmoRequestValidator _validator = new();

    [Fact]
    public void Should_pass_when_all_fields_are_valid()
    {
        var req = new CreateGuizmoRequest("Widget", "Acme", "A great widget", 9.99m);
        var result = _validator.TestValidate(req);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void Should_fail_when_name_is_empty(string? name)
    {
        var req = new CreateGuizmoRequest(name!, "Acme", null, 9.99m);
        var result = _validator.TestValidate(req);
        result.ShouldHaveValidationErrorFor(x => x.Name);
    }

    [Fact]
    public void Should_fail_when_name_exceeds_250_chars()
    {
        var req = new CreateGuizmoRequest(new string('A', 251), "Acme", null, 9.99m);
        var result = _validator.TestValidate(req);
        result.ShouldHaveValidationErrorFor(x => x.Name);
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void Should_fail_when_manufacturer_is_empty(string? manufacturer)
    {
        var req = new CreateGuizmoRequest("Widget", manufacturer!, null, 9.99m);
        var result = _validator.TestValidate(req);
        result.ShouldHaveValidationErrorFor(x => x.Manufacturer);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Should_fail_when_msrp_is_not_positive(decimal msrp)
    {
        var req = new CreateGuizmoRequest("Widget", "Acme", null, msrp);
        var result = _validator.TestValidate(req);
        result.ShouldHaveValidationErrorFor(x => x.Msrp);
    }

    [Fact]
    public void Description_is_optional()
    {
        var req = new CreateGuizmoRequest("Widget", "Acme", null, 9.99m);
        var result = _validator.TestValidate(req);
        result.ShouldNotHaveValidationErrorFor(x => x.Description);
    }
}
