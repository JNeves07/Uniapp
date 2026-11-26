using System.ComponentModel.DataAnnotations;

namespace UniApp.Api.Application.DTOs;

/// <summary>
/// Contrato de entrada compativel com o corpo anterior (entidade GrupoEstudo).
/// </summary>
public sealed record CriarGrupoRequest(
    [Required(AllowEmptyStrings = false), MaxLength(160)] string Nome,
    [Required(AllowEmptyStrings = false), MaxLength(160)] string Disciplina,
    [MaxLength(120)] string? Curso,
    [MaxLength(80)] string? Horario,
    [MaxLength(40)] string? Formato,
    [Range(0, int.MaxValue)] int? VagasDisponiveis,
    bool? Recomendado);

public sealed record GrupoResponse(
    int Id,
    string Nome,
    string Disciplina,
    string Curso,
    string Horario,
    string Formato,
    int VagasDisponiveis,
    bool Recomendado);
