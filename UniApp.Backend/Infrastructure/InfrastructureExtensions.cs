using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;
using UniApp.Api.Application.Interfaces;
using UniApp.Api.Domain.Services;
using UniApp.Api.Infrastructure.Data;
using UniApp.Api.Infrastructure.Repositories;
using UniApp.Api.Infrastructure.Security;

namespace UniApp.Api.Infrastructure;

/// <summary>
/// Registro de EF Core, repositorios e seguranca. Suporte a MySQL (padrao) e SQLite (testes/demo).
/// </summary>
public static class InfrastructureExtensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var provider = configuration["Database:Provider"] ?? "MySql";
        var connectionString = configuration.GetConnectionString("DefaultConnection")
                               ?? throw new InvalidOperationException("ConnectionStrings:DefaultConnection nao configurada.");

        services.AddDbContext<AppDbContext>(options =>
        {
            if (string.Equals(provider, "Sqlite", StringComparison.OrdinalIgnoreCase))
            {
                options.UseSqlite(connectionString);
                return;
            }

            // Versao explicita evita conexao durante o design-time das migracoes.
            var serverVersion = configuration["Database:ServerVersion"] is { Length: > 0 } versionText
                ? ServerVersion.Parse(versionText)
                : ServerVersion.Create(new Version(8, 0, 36), ServerType.MySql);

            options.UseMySql(connectionString, serverVersion);
        });

        services.AddScoped<IUsuarioRepository, UsuarioRepository>();
        services.AddScoped<IGrupoRepository, GrupoRepository>();
        services.AddScoped<IMaterialRepository, MaterialRepository>();

        services.AddSingleton<IPasswordHasher, PasswordHasher>();
        services.AddSingleton<ITokenService, JwtTokenService>();

        services.AddOptions<JwtOptions>()
                .Bind(configuration.GetSection(JwtOptions.SectionName))
                .ValidateDataAnnotations()
                .Validate(o => System.Text.Encoding.UTF8.GetByteCount(o.Key) >= 32,
                    "Jwt:Key deve ter ao menos 32 bytes.");

        return services;
    }
}
