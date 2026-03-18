using FluentValidation.TestHelper;
using GuizmoApi.Application.DTOs;
using GuizmoApi.Application.Validators;

namespace GuizmoApi.Application.Tests.Validators;

public class GuizmoPagedQueryValidatorTests
{
    private readonly GuizmoPagedQueryValidator _validator = new();

    [Fact]
    public void Should_pass_when_all_fields_are_valid()
    {
        var result = _validator.TestValidate(new GuizmoPagedQuery(1, 10, "asc"));
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Should_fail_when_page_number_is_below_one(int pageNumber)
    {
        var result = _validator.TestValidate(new GuizmoPagedQuery(PageNumber: pageNumber));
        result.ShouldHaveValidationErrorFor(x => x.PageNumber);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-5)]
    public void Should_fail_when_page_size_is_below_one(int pageSize)
    {
        var result = _validator.TestValidate(new GuizmoPagedQuery(PageSize: pageSize));
        result.ShouldHaveValidationErrorFor(x => x.PageSize);
    }
}
