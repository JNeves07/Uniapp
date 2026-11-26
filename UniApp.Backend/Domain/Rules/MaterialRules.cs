using UniApp.Api.Domain.Entities;
using UniApp.Api.Domain.Exceptions;

namespace UniApp.Api.Domain.Rules;

/// <summary>
/// Regras de domínio para anúncios do marketplace.
/// </summary>
public static class MaterialRules
{
    public static readonly string[] Modalidades = ["Venda", "Troca", "Doação"];

    public static readonly string[] Categorias = ["Livros", "Calculadoras", "Laboratório", "Apostilas"];

    public static void Validate(string? titulo, string? modalidade, decimal preco)
    {
        if (string.IsNullOrWhiteSpace(titulo))
            throw new DomainException("Informe o título do material.");

        if (titulo.Trim().Length > 180)
            throw new DomainException("O título do material é muito longo (máximo 180 caracteres).");

        if (string.IsNullOrWhiteSpace(modalidade) ||
            !Modalidades.Contains(modalidade, StringComparer.OrdinalIgnoreCase))
            throw new DomainException("Modalidade inválida. Use Venda, Troca ou Doação.");

        if (preco < 0)
            throw new DomainException("O preço não pode ser negativo.");
    }

    /// <summary>
    /// Aplica as regras de normalização: doações sempre têm preço zero.
    /// </summary>
    public static void Apply(Material material)
    {
        material.Titulo = material.Titulo.Trim();
        material.Descricao = (material.Descricao ?? string.Empty).Trim();
        material.Modalidade = NormalizarModalidade(material.Modalidade);
        material.DataCriacao = DateTime.UtcNow;

        if (material.Modalidade == "Doação")
            material.Preco = 0;

        if (material.Preco < 0)
            throw new DomainException("O preço não pode ser negativo.");
    }

    private static string NormalizarModalidade(string? modalidade)
        => Modalidades.FirstOrDefault(m => string.Equals(m, modalidade, StringComparison.OrdinalIgnoreCase))
           ?? modalidade ?? string.Empty;
}
