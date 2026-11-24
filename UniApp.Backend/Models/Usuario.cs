using System.ComponentModel.DataAnnotations;

namespace UniApp.Api.Models;

public class Usuario
{
    public int Id { get; set; }
    [Required] public string Nome { get; set; } = string.Empty;
    [Required, EmailAddress] public string EmailInstitucional { get; set; } = string.Empty;
    [Required] public string SenhaHash { get; set; } = string.Empty;
    public string Curso { get; set; } = "Análise e Desenvolvimento de Sistemas";
    public double Reputacao { get; set; } = 5.0;
    public int AvaliacoesCount { get; set; } = 1;
}
