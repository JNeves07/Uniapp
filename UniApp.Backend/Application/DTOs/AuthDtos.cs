using System.ComponentModel.DataAnnotations;

namespace UniApp.Api.Application.DTOs;

// Observacao: em records posicionais as anotacoes devem estar nos parametros do construtor,
// e nao com alvo [property:], para evitar erro de metadata no MVC.
public sealed record LoginRequest(
    [Required(AllowEmptyStrings = false), EmailAddress, MaxLength(180)] string Email,
    [Required(AllowEmptyStrings = false), MaxLength(200)] string Senha);

public sealed record RegisterRequest(
    [Required(AllowEmptyStrings = false), MaxLength(120)] string Nome,
    [Required(AllowEmptyStrings = false), EmailAddress, MaxLength(180)] string Email,
    [Required(AllowEmptyStrings = false), MinLength(6), MaxLength(200)] string Senha,
    [MaxLength(120)] string? Curso);

public sealed record RefreshRequest(
    [Required(AllowEmptyStrings = false)] string RefreshToken);

/// <summary>
/// Dados do usuario devolvidos pela API. Nunca inclui o hash da senha.
/// </summary>
public sealed record UsuarioResponse(
    int Id,
    string Nome,
    string EmailInstitucional,
    string Curso,
    double Reputacao,
    int AvaliacoesCount);

/// <summary>
/// Resposta de login/registro/refresh. Mantem os campos token e usuario usados pelo app MAUI.
/// </summary>
public sealed record AuthResponse(
    string Token,
    string RefreshToken,
    int ExpiresIn,
    UsuarioResponse Usuario);
