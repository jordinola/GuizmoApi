using FluentValidation.TestHelper;
using GuizmoApi.Application.DTOs;
using GuizmoApi.Application.Validators;

namespace GuizmoApi.Application.Tests.Validators;

public class UpdateCategoryRequestValidatorTests
{
    private readonly UpdateCategoryRequestValidator _validator = new();

    [Fact]
    public void Should_pass_when_name_is_valid()
    {
        var req = new UpdateCategoryRequest("Electronics");
        var result = _validator.TestValidate(req);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void Should_fail_when_name_is_empty(string? name)
    {
        var req = new UpdateCategoryRequest(name!);
        var result = _validator.TestValidate(req);
        result.ShouldHaveValidationErrorFor(x => x.Name)
            .WithErrorMessage("Name is required.");
    }

    [Fact]
    public void Should_fail_when_name_exceeds_250_chars()
    {
        var req = new UpdateCategoryRequest(new string('A', 251));
        var result = _validator.TestValidate(req);
        result.ShouldHaveValidationErrorFor(x => x.Name)
            .WithErrorMessage("Name must not exceed 250 characters.");
    }

    [Fact]
    public void Should_pass_when_name_is_exactly_250_chars()
    {
        var req = new UpdateCategoryRequest(new string('A', 250));
        var result = _validator.TestValidate(req);
        result.ShouldNotHaveAnyValidationErrors();
    }
}
