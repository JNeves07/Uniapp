using System.ComponentModel.DataAnnotations;

namespace UniApp.Api.Domain.Entities;

/// <summary>
/// Grupo de estudo colaborativo. A quantidade de vagas é sempre gerenciada pelas regras de domínio.
/// </summary>
public class GrupoEstudo
{
    public int Id { get; set; }

    [Required]
    [MaxLength(160)]
    public string Nome { get; set; } = string.Empty;

    [Required]
    [MaxLength(160)]
    public string Disciplina { get; set; } = string.Empty;

    [MaxLength(120)]
    public string Curso { get; set; } = "ADS";

    [MaxLength(80)]
    public string Horario { get; set; } = "Quintas, 19h";

    [MaxLength(40)]
    public string Formato { get; set; } = "Online";

    [Range(0, int.MaxValue)]
    public int VagasDisponiveis { get; set; } = 5;

    public bool Recomendado { get; set; } = true;
}
