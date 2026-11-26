using System.Security.Claims;
using FluentValidation;
using Microsoft.IdentityModel.Tokens;
using UniApp.Api.Application.DTOs;
using UniApp.Api.Application.Interfaces;
using UniApp.Api.Application.Mapping;
using UniApp.Api.Application.Validation;
using UniApp.Api.Domain.Entities;
using UniApp.Api.Domain.Exceptions;
using UniApp.Api.Domain.Rules;
using UniApp.Api.Domain.Services;

namespace UniApp.Api.Application.Services;

/// <summary>
/// Regras de aplicacao para autenticacao, cadastro institucional e refresh de token.
/// </summary>
public sealed class AuthService(
    IUsuarioRepository usuarios,
    IPasswordHasher hasher,
    ITokenService tokenService,
    IValidator<LoginRequest> loginValidator,
    IValidator<RegisterRequest> registerValidator) : IAuthService
{
    public async Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        await loginValidator.ValidateAndThrowAsync(request, cancellationToken);

        var email = InstitutionalEmail.Normalize(request.Email);
        var usuario = await usuarios.GetByEmailAsync(email, cancellationToken);

        if (usuario is null || !hasher.Verify(usuario.SenhaHash, request.Senha))
            throw new UnauthorizedException("E-mail institucional ou senha inválidos.");

        return tokenService.CriarResposta(DtoMapper.ToResponse(usuario));
    }

    public async Task<AuthResponse> RegistrarAsync(RegisterRequest request, CancellationToken cancellationToken = default)
    {
        await registerValidator.ValidateAndThrowAsync(request, cancellationToken);

        var email = InstitutionalEmail.Normalize(request.Email);
        if (!InstitutionalEmail.IsValid(email))
            throw new DomainException(InstitutionalEmail.InvalidMessage);

        if (await usuarios.ExistsEmailAsync(email, cancellationToken))
            throw new DomainException("E-mail já cadastrado.");

        var usuario = new Usuario
        {
            Nome = request.Nome.Trim(),
            EmailInstitucional = email,
            SenhaHash = hasher.Hash(request.Senha),
            Curso = string.IsNullOrWhiteSpace(request.Curso)
                ? "Análise e Desenvolvimento de Sistemas"
                : request.Curso.Trim()
        };

        await usuarios.AddAsync(usuario, cancellationToken);

        if (usuario.Id == 0)
            throw new DomainException("Não foi possível concluir o cadastro. Tente novamente.");

        return tokenService.CriarResposta(DtoMapper.ToResponse(usuario));
    }

    public async Task<AuthResponse> RefreshAsync(string refreshToken, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(refreshToken))
            throw new DomainException("Refresh token inválido.");

        ClaimsPrincipal principal;
        try
        {
            principal = tokenService.ValidarRefreshToken(refreshToken);
        }
        catch (SecurityTokenException)
        {
            throw new UnauthorizedException("Sua sessão expirou. Faça login novamente.");
        }
        catch (ArgumentException)
        {
            throw new UnauthorizedException("Sua sessão expirou. Faça login novamente.");
        }

        var idTexto = principal.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (!int.TryParse(idTexto, out var id))
            throw new UnauthorizedException("Sua sessão expirou. Faça login novamente.");

        var usuario = await usuarios.GetByIdAsync(id, cancellationToken)
                      ?? throw new UnauthorizedException("Sua sessão expirou. Faça login novamente.");

        return tokenService.CriarResposta(DtoMapper.ToResponse(usuario));
    }
}
