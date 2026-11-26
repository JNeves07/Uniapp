using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using UniApp.Api.Application;
using UniApp.Api.Application.DTOs;
using UniApp.Api.Application.Interfaces;
using UniApp.Api.Application.Services;
using UniApp.Api.Application.Validation;
using UniApp.Api.Domain.Services;
using UniApp.Api.Infrastructure.Data;
using UniApp.Api.Infrastructure.Repositories;
using UniApp.Api.Infrastructure.Security;

namespace UniApp.Backend.Tests;

/// <summary>
/// Fabrica de dependencias reais (repositorios EF + validadores + servicos) para testes unitarios.
/// </summary>
public static class TestHelpers
{
    public const string JwtKey = "TEST-KEY-NAO-USAR-EM-PRODUCAO-0123456789abcdef";

    public static AppDbContext CreateInMemoryContext(string? name = null)
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(name ?? Guid.NewGuid().ToString("N"))
            .Options;
        return new AppDbContext(options);
    }

    public static (IAuthService Auth, IGrupoService Grupos, IMaterialService Materiais, AppDbContext Db) CreateServices(AppDbContext db)
    {
        var hasher = new PasswordHasher();
        var tokenService = new JwtTokenService(Options.Create(new JwtOptions
        {
            Key = JwtKey,
            Issuer = "UniAppApi",
            Audience = "UniAppMobile",
            AccessTokenMinutes = 60,
            RefreshTokenDays = 30
        }));

        var usuarios = new UsuarioRepository(db);
        var grupos = new GrupoRepository(db);
        var materiais = new MaterialRepository(db);

        IValidator<LoginRequest> loginValidator = new LoginRequestValidator();
        IValidator<RegisterRequest> registerValidator = new RegisterRequestValidator();
        IValidator<CriarGrupoRequest> grupoValidator = new CriarGrupoRequestValidator();
        IValidator<CriarMaterialRequest> materialValidator = new CriarMaterialRequestValidator();

        return (
            new AuthService(usuarios, hasher, tokenService, loginValidator, registerValidator),
            new GrupoService(grupos, grupoValidator),
            new MaterialService(materiais, usuarios, materialValidator),
            db
        );
    }

    public static (IAuthService Auth, AppDbContext Db) CreateAuth(AppDbContext db)
    {
        var services = CreateServices(db);
        return (services.Auth, db);
    }

    public static async Task<(IAuthService Auth, AppDbContext Db)> CreateSeededAuthAsync(string? name = null)
    {
        var db = CreateInMemoryContext(name);
        await DbInitializer.SeedAsync(db, new PasswordHasher());
        return CreateAuth(db);
    }
}
