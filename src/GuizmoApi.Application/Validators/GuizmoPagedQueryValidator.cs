using FluentValidation;
using GuizmoApi.Application.DTOs;

namespace GuizmoApi.Application.Validators;

public class GuizmoPagedQueryValidator : AbstractValidator<GuizmoPagedQuery>
{
    public GuizmoPagedQueryValidator()
    {
        RuleFor(x => x.PageNumber).GreaterThanOrEqualTo(1);
        RuleFor(x => x.PageSize).GreaterThanOrEqualTo(1);
    }
}
