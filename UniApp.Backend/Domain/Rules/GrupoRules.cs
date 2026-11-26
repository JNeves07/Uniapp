using UniApp.Api.Domain.Entities;
using UniApp.Api.Domain.Exceptions;

namespace UniApp.Api.Domain.Rules;

/// <summary>
/// Regras de domínio para grupos de estudo e inscrições.
/// </summary>
public static class GrupoRules
{
    public static readonly string[] Formatos = ["Online", "Presencial"];

    public static void Validate(string? nome, string? disciplina, string? formato, int vagas)
    {
        if (string.IsNullOrWhiteSpace(nome))
            throw new DomainException("Informe o nome do grupo.");

        if (string.IsNullOrWhiteSpace(disciplina))
            throw new DomainException("Informe a disciplina do grupo.");

        if (string.IsNullOrWhiteSpace(formato) ||
            !Formatos.Contains(formato, StringComparer.OrdinalIgnoreCase))
            throw new DomainException("Formato inválido. Use Online ou Presencial.");

        if (vagas < 0)
            throw new DomainException("A quantidade de vagas não pode ser negativa.");
    }

    public static void Apply(GrupoEstudo grupo)
    {
        grupo.Nome = grupo.Nome.Trim();
        grupo.Disciplina = grupo.Disciplina.Trim();
        grupo.Curso = string.IsNullOrWhiteSpace(grupo.Curso) ? "ADS" : grupo.Curso.Trim();
        grupo.Horario = string.IsNullOrWhiteSpace(grupo.Horario) ? "Quintas, 19h" : grupo.Horario.Trim();
        grupo.Formato = Formatos.FirstOrDefault(f => string.Equals(f, grupo.Formato, StringComparison.OrdinalIgnoreCase))
                        ?? grupo.Formato;

        if (grupo.VagasDisponiveis < 0)
            throw new DomainException("A quantidade de vagas não pode ser negativa.");
    }

    /// <summary>
    /// Reserva uma vaga e lanca DomainException quando o grupo esta lotado.
    /// </summary>
    public static void ReservarVaga(GrupoEstudo grupo)
    {
        if (grupo.VagasDisponiveis <= 0)
            throw new DomainException("Grupo lotado.");

        grupo.VagasDisponiveis--;
    }
}
