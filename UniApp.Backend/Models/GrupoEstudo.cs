using System.ComponentModel.DataAnnotations;

namespace UniApp.Api.Models;

public class GrupoEstudo
{
    public int Id { get; set; }
    [Required] public string Nome { get; set; } = string.Empty;
    [Required] public string Disciplina { get; set; } = string.Empty;
    public string Curso { get; set; } = "ADS";
    public string Horario { get; set; } = "Quintas, 19h";
    public string Formato { get; set; } = "Online";
    public int VagasDisponiveis { get; set; } = 5;
    public bool Recomendado { get; set; } = true;
}
