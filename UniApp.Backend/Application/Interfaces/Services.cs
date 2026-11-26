using System.Security.Claims;
using UniApp.Api.Application.DTOs;

namespace UniApp.Api.Application.Interfaces;

public interface IAuthService
{
    Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default);
    Task<AuthResponse> RegistrarAsync(RegisterRequest request, CancellationToken cancellationToken = default);
    Task<AuthResponse> RefreshAsync(string refreshToken, CancellationToken cancellationToken = default);
}

public interface IGrupoService
{
    Task<List<GrupoResponse>> ListAsync(string? formato, string? busca, CancellationToken cancellationToken = default);
    Task<GrupoResponse> CriarAsync(CriarGrupoRequest request, CancellationToken cancellationToken = default);
    Task<GrupoResponse> ParticiparAsync(int id, CancellationToken cancellationToken = default);
}

public interface IMaterialService
{
    Task<List<MaterialResponse>> ListAsync(string? categoria, string? busca, CancellationToken cancellationToken = default);
    Task<MaterialResponse> CriarAsync(CriarMaterialRequest request, CancellationToken cancellationToken = default);
}

/// <summary>
/// Emissao e validacao de JWT. Implementado na camada de infraestrutura.
/// </summary>
public interface ITokenService
{
    AuthResponse CriarResposta(UsuarioResponse usuario);

    /// <summary>
    /// Valida assinatura, issuer, audience, expiracao e o tipo "refresh".
    /// </summary>
    ClaimsPrincipal ValidarRefreshToken(string token);
}
