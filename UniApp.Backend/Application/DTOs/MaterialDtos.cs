using System.ComponentModel.DataAnnotations;

namespace UniApp.Api.Application.DTOs;

/// <summary>
/// Contrato de entrada compativel com o corpo anterior (entidade Material).
/// </summary>
public sealed record CriarMaterialRequest(
    [Required(AllowEmptyStrings = false), MaxLength(180)] string Titulo,
    [MaxLength(1000)] string? Descricao,
    [MaxLength(80)] string? Categoria,
    [MaxLength(120)] string? CursoRelacionado,
    [MaxLength(80)] string? EstadoConservacao,
    [MaxLength(40)] string? Modalidade,
    [Range(typeof(decimal), "0", "79228162514264337593543950335")] decimal? Preco,
    [MaxLength(500)] string? ImagemUrl,
    [MaxLength(40)] string? ContatoWhatsApp,
    [Range(1, int.MaxValue)] int UsuarioId);

public sealed record MaterialResponse(
    int Id,
    string Titulo,
    string Descricao,
    string Categoria,
    string CursoRelacionado,
    string EstadoConservacao,
    string Modalidade,
    decimal Preco,
    string ImagemUrl,
    string ContatoWhatsApp,
    DateTime DataCriacao,
    UsuarioResponse? Usuario);
