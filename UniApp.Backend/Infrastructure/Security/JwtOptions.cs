using System.ComponentModel.DataAnnotations;

namespace UniApp.Api.Infrastructure.Security;

/// <summary>
/// Configuracao de JWT. A chave deve vir de variavel de ambiente/user-secrets em producao.
/// </summary>
public sealed class JwtOptions
{
    public const string SectionName = "Jwt";

    [Required(AllowEmptyStrings = false)]
    [MinLength(32, ErrorMessage = "Jwt:Key deve ter ao menos 32 caracteres.")]
    public string Key { get; set; } = string.Empty;

    [Required(AllowEmptyStrings = false)]
    public string Issuer { get; set; } = "UniAppApi";

    [Required(AllowEmptyStrings = false)]
    public string Audience { get; set; } = "UniAppMobile";

    [Range(1, 1440)]
    public int AccessTokenMinutes { get; set; } = 60;

    [Range(1, 365)]
    public int RefreshTokenDays { get; set; } = 30;
}
