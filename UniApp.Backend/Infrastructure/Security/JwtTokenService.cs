using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using UniApp.Api.Application.DTOs;
using UniApp.Api.Application.Interfaces;

namespace UniApp.Api.Infrastructure.Security;

/// <summary>
/// Emissao de access token (curto) e refresh token (longo), ambos assinados com HMAC-SHA256.
/// </summary>
public sealed class JwtTokenService(IOptions<JwtOptions> options) : ITokenService
{
    public const string AccessTokenType = "access";
    public const string RefreshTokenType = "refresh";
    private const string TypeClaim = "typ";

    private readonly JwtOptions _options = options.Value;
    private readonly SymmetricSecurityKey _key =
        new(Encoding.UTF8.GetBytes(options.Value.Key));

    public AuthResponse CriarResposta(UsuarioResponse usuario)
    {
        var access = CriarToken(usuario, AccessTokenType, DateTime.UtcNow.AddMinutes(_options.AccessTokenMinutes));
        var refresh = CriarToken(usuario, RefreshTokenType, DateTime.UtcNow.AddDays(_options.RefreshTokenDays));

        return new AuthResponse(access, refresh, _options.AccessTokenMinutes * 60, usuario);
    }

    public ClaimsPrincipal ValidarRefreshToken(string token)
    {
        var parameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = _options.Issuer,
            ValidateAudience = true,
            ValidAudience = _options.Audience,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = _key,
            ValidateLifetime = true,
            RequireExpirationTime = true,
            ValidAlgorithms = [SecurityAlgorithms.HmacSha256],
            ClockSkew = TimeSpan.FromSeconds(30),
            NameClaimType = ClaimTypes.Name,
            RoleClaimType = ClaimTypes.Role
        };

        var principal = new JwtSecurityTokenHandler().ValidateToken(token, parameters, out _);

        if (!string.Equals(principal.FindFirst(TypeClaim)?.Value, RefreshTokenType, StringComparison.Ordinal))
            throw new SecurityTokenException("Token nao e um refresh token.");

        return principal;
    }

    private string CriarToken(UsuarioResponse usuario, string tipo, DateTime expires)
    {
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
            new Claim(ClaimTypes.Email, usuario.EmailInstitucional),
            new Claim(ClaimTypes.Name, usuario.Nome),
            new Claim(TypeClaim, tipo),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString("N"))
        };

        var token = new JwtSecurityToken(
            issuer: _options.Issuer,
            audience: _options.Audience,
            claims: claims,
            notBefore: DateTime.UtcNow.AddMinutes(-1),
            expires: expires,
            signingCredentials: new SigningCredentials(_key, SecurityAlgorithms.HmacSha256));

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
