using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using GuizmoApi.Application.Interfaces;
using GuizmoApi.Application.Services;
using GuizmoApi.Application.Validators;

namespace GuizmoApi.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IGuizmoService, GuizmoService>();
        services.AddValidatorsFromAssemblyContaining<CreateGuizmoRequestValidator>();
        return services;
    }
}
