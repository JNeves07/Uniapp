using System.Text.RegularExpressions;

namespace UniApp.Api.Domain.Rules;

/// <summary>
/// Regra institucional do UniApp: somente e-mails acadêmicos brasileiros (.edu.br) são aceitos.
/// </summary>
public static partial class InstitutionalEmail
{
    public const string InstitutionalSuffix = "edu.br";

    [GeneratedRegex(@"^[A-Za-z0-9._%+\-]+@(?:[A-Za-z0-9\-]+\.)+edu\.br$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
    private static partial Regex Pattern();

    /// <summary>
    /// Normaliza o e-mail para comparação (sem espaços e em minúsculas).
    /// </summary>
    public static string Normalize(string? email) => (email ?? string.Empty).Trim().ToLowerInvariant();

    /// <summary>
    /// Valida o formato e o domínio institucional. Exemplos válidos:
    /// aluno@unifacema.edu.br, joao.marcelo@unifacema.edu.br.
    /// </summary>
    public static bool IsValid(string? email)
    {
        var value = Normalize(email);
        return value.Length is > 8 and <= 180 && Pattern().IsMatch(value);
    }

    /// <summary>
    /// Mensagem amigável usada pelas camadas de aplicação e API.
    /// </summary>
    public const string InvalidMessage = "Utilize um e-mail institucional válido (.edu.br).";
}
