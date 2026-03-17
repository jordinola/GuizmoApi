using FluentValidation;
using GuizmoApi.Application.DTOs;

namespace GuizmoApi.Application.Validators;

public class CreateGuizmoRequestValidator : AbstractValidator<CreateGuizmoRequest>
{
    public CreateGuizmoRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required.")
            .MaximumLength(250).WithMessage("Name must not exceed 250 characters.");

        RuleFor(x => x.Manufacturer)
            .NotEmpty().WithMessage("Manufacturer is required.")
            .MaximumLength(250).WithMessage("Manufacturer must not exceed 250 characters.");

        RuleFor(x => x.Msrp)
            .GreaterThan(0).WithMessage("MSRP must be greater than zero.");

        RuleFor(x => x.CategoryId)
            .NotEmpty().WithMessage("CategoryId is required.")
            .GreaterThan(0).WithMessage("CategoryId must be greater than zero.");
    }
}
