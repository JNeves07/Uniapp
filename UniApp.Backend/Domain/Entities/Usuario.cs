using System.ComponentModel.DataAnnotations;

namespace UniApp.Api.Domain.Entities;

/// <summary>
/// Usuário da comunidade acadêmica. <see cref="SenhaHash"/> nunca deve expor a senha em claro.
/// </summary>
public class Usuario
{
    public int Id { get; set; }

    [Required]
    [MaxLength(120)]
    public string Nome { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [MaxLength(180)]
    public string EmailInstitucional { get; set; } = string.Empty;

    [Required]
    [MaxLength(300)]
    public string SenhaHash { get; set; } = string.Empty;

    [MaxLength(120)]
    public string Curso { get; set; } = "Análise e Desenvolvimento de Sistemas";

    public double Reputacao { get; set; } = 5.0;

    public int AvaliacoesCount { get; set; } = 1;

    public ICollection<Material> Materiais { get; set; } = new List<Material>();
}
