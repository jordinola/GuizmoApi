using FluentValidation;
using GuizmoApi.Application.DTOs;

namespace GuizmoApi.Application.Validators;

public class GuizmoPagedQueryValidator : AbstractValidator<GuizmoPagedQuery>
{
    public GuizmoPagedQueryValidator()
    {
        RuleFor(x => x.PageNumber).GreaterThanOrEqualTo(1);
        RuleFor(x => x.PageSize).GreaterThanOrEqualTo(1);
        RuleFor(x => x.SortOrder)
            .Must(s => s.Equals("asc", StringComparison.OrdinalIgnoreCase) || s.Equals("desc", StringComparison.OrdinalIgnoreCase))
            .WithMessage("SortOrder must be either 'asc' or 'desc'.");
    }
}
