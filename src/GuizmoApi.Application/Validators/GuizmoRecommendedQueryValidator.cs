using FluentValidation;
using Microsoft.EntityFrameworkCore;
using GuizmoApi.Application.DTOs;
using GuizmoApi.Infrastructure.Data;

namespace GuizmoApi.Application.Validators;

public class GuizmoRecommendedQueryValidator : AbstractValidator<GuizmoRecommendedQuery>
{
    public GuizmoRecommendedQueryValidator(AppDbContext context)
    {
        When(x => x.GuizmoId.HasValue, () =>
        {
            RuleFor(x => x.GuizmoId)
                .Must(id => id!.Value > 0)
                .WithMessage("GuizmoId must be greater than zero.");

            RuleFor(x => x.GuizmoId)
                .MustAsync(async (id, ct) => await context.Guizmos.AnyAsync(g => g.Id == id!.Value, ct))
                .WithMessage("GuizmoId does not exist.")
                .When(x => x.GuizmoId!.Value > 0);
        });
    }
}
