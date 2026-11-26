using System.IdentityModel.Tokens.Jwt;
using FluentValidation;
using Microsoft.IdentityModel.Tokens;
using UniApp.Api.Application.DTOs;
using UniApp.Api.Domain.Exceptions;
using UniApp.Api.Infrastructure.Security;
using Xunit;

namespace UniApp.Backend.Tests;

public class AuthServiceTests
{
    [Fact]
    public async Task Registrar_ComEmailInstitucional_GravaHashERetornaToken()
    {
        var (auth, db) = TestHelpers.CreateAuth(TestHelpers.CreateInMemoryContext());

        var response = await auth.RegistrarAsync(new RegisterRequest(
            "Maria", "maria@unifacema.edu.br", "senha123", "ADS"));

        Assert.NotEmpty(response.Token);
        Assert.NotEmpty(response.RefreshToken);
        Assert.Equal(3600, response.ExpiresIn);
        Assert.Equal("maria@unifacema.edu.br", response.Usuario.EmailInstitucional);

        var usuario = await db.Usuarios.SingleAsync();
        Assert.StartsWith("$uniapp$", usuario.SenhaHash);
        Assert.DoesNotContain("senha123", usuario.SenhaHash);
    }

    [Fact]
    public async Task Registrar_ComEmailNaoInstitucional_Rejeita()
    {
        var (auth, _) = TestHelpers.CreateAuth(TestHelpers.CreateInMemoryContext());

        var ex = await Assert.ThrowsAsync<ValidationException>(() => auth.RegistrarAsync(
            new RegisterRequest("Maria", "maria@gmail.com", "senha123", "ADS")));

        Assert.Contains(".edu.br", ex.Errors.Single().ErrorMessage);
    }

    [Fact]
    public async Task Registrar_EmailDuplicado_Rejeita()
    {
        var (auth, _) = await TestHelpers.CreateSeededAuthAsync();

        var ex = await Assert.ThrowsAsync<DomainException>(() => auth.RegistrarAsync(
            new RegisterRequest("Outro", "joao.marcelo@unifacema.edu.br", "senha123", "ADS")));

        Assert.Equal("E-mail já cadastrado.", ex.Message);
    }

    [Fact]
    public async Task Login_ComCredenciaisCorretas_RetornaToken()
    {
        var (auth, _) = await TestHelpers.CreateSeededAuthAsync();

        var response = await auth.LoginAsync(new LoginRequest("joao.marcelo@unifacema.edu.br", "123456"));

        Assert.Equal("João Marcelo", response.Usuario.Nome);
        Assert.NotEmpty(response.Token);
    }

    [Fact]
    public async Task Login_ComSenhaErrada_RetornaNaoAutorizado()
        => await Assert.ThrowsAsync<UnauthorizedException>(async () =>
        {
            var (auth, _) = await TestHelpers.CreateSeededAuthAsync();
            await auth.LoginAsync(new LoginRequest("joao.marcelo@unifacema.edu.br", "errada"));
        });

    [Fact]
    public async Task Login_EmailComMaiusculasEEspacos_ECorrigido()
    {
        var (auth, _) = await TestHelpers.CreateSeededAuthAsync();
        var response = await auth.LoginAsync(new LoginRequest(" JOAO.MARCELO@unifacema.edu.br ", "123456"));
        Assert.Equal("joao.marcelo@unifacema.edu.br", response.Usuario.EmailInstitucional);
    }

    [Fact]
    public async Task Refresh_ComRefreshTokenValido_GeraNovoAccess()
    {
        var (auth, _) = await TestHelpers.CreateSeededAuthAsync();
        var login = await auth.LoginAsync(new LoginRequest("joao.marcelo@unifacema.edu.br", "123456"));

        var refreshed = await auth.RefreshAsync(login.RefreshToken);

        Assert.NotEmpty(refreshed.Token);
        Assert.Equal(login.Usuario.Id, refreshed.Usuario.Id);
    }

    [Fact]
    public async Task Refresh_ComAccessToken_Rejeita()
    {
        var (auth, _) = await TestHelpers.CreateSeededAuthAsync();
        var login = await auth.LoginAsync(new LoginRequest("joao.marcelo@unifacema.edu.br", "123456"));

        await Assert.ThrowsAsync<UnauthorizedException>(() => auth.RefreshAsync(login.Token));
    }

    [Fact]
    public async Task Refresh_ComTokenDeOutroIssuer_Rejeita()
    {
        var tokenService = new JwtTokenService(Microsoft.Extensions.Options.Options.Create(new JwtOptions
        {
            Key = TestHelpers.JwtKey,
            Issuer = "OutroIssuer",
            Audience = "UniAppMobile"
        }));

        var token = tokenService.CriarResposta(new UsuarioResponse(1, "Teste", "x@unifacema.edu.br", "ADS", 5, 1)).RefreshToken;
        var (auth, _) = await TestHelpers.CreateSeededAuthAsync();

        await Assert.ThrowsAsync<UnauthorizedException>(() => auth.RefreshAsync(token));
    }

    [Fact]
    public async Task Refresh_ComTokenExpirado_Rejeita()
    {
        var tokenService = new JwtTokenService(Microsoft.Extensions.Options.Options.Create(new JwtOptions
        {
            Key = TestHelpers.JwtKey,
            Issuer = "UniAppApi",
            Audience = "UniAppMobile",
            AccessTokenMinutes = 1,
            RefreshTokenDays = 1
        }));

        var usuario = new UsuarioResponse(1, "Teste", "x@unifacema.edu.br", "ADS", 5, 1);
        var token = new JwtSecurityTokenHandler().WriteToken(new JwtSecurityToken(
            issuer: "UniAppApi",
            audience: "UniAppMobile",
            claims: [
                new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.NameIdentifier, "1"),
                new System.Security.Claims.Claim("typ", "refresh")
            ],
            notBefore: DateTime.UtcNow.AddDays(-2),
            expires: DateTime.UtcNow.AddDays(-1),
            signingCredentials: new SigningCredentials(
                new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(TestHelpers.JwtKey)),
                SecurityAlgorithms.HmacSha256)));

        var (auth, _) = await TestHelpers.CreateSeededAuthAsync();
        await Assert.ThrowsAsync<UnauthorizedException>(() => auth.RefreshAsync(token));
    }
}
