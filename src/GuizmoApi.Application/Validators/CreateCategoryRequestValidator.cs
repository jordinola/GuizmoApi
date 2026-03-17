using FluentValidation;
using GuizmoApi.Application.DTOs;

namespace GuizmoApi.Application.Validators;

public class CreateCategoryRequestValidator : AbstractValidator<CreateCategoryRequest>
{
    public CreateCategoryRequestValidator()
    {
        RuleFor(x => x.Name)
        .NotEmpty().WithMessage("Name is required.")
        .MaximumLength(250).WithMessage("Name must not exceed 250 characters.");
    }
}
