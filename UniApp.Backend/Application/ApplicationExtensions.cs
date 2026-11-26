using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using UniApp.Api.Application.Services;
using UniApp.Api.Application.Interfaces;
using UniApp.Api.Application.Validation;

namespace UniApp.Api.Application;

/// <summary>
/// Registro dos servicos da camada de aplicacao.
/// </summary>
public static class ApplicationExtensions
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddValidatorsFromAssemblyContaining<LoginRequestValidator>();

        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IGrupoService, GrupoService>();
        services.AddScoped<IMaterialService, MaterialService>();

        return services;
    }
}
