namespace UniApp.Mobile.Models;

/// <summary>Resposta de login/registro/refresh da API.</summary>
public class AuthResponse
{
    public string Token { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public int ExpiresIn { get; set; }
    public UsuarioDto Usuario { get; set; } = new();
}

public class UsuarioDto
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string EmailInstitucional { get; set; } = string.Empty;
    public string Curso { get; set; } = string.Empty;
    public double Reputacao { get; set; }
}

public class MaterialDto
{
    public int Id { get; set; }
    public string Titulo { get; set; } = string.Empty;
    public string Descricao { get; set; } = string.Empty;
    public string Categoria { get; set; } = string.Empty;
    public string CursoRelacionado { get; set; } = string.Empty;
    public string EstadoConservacao { get; set; } = string.Empty;
    public string Modalidade { get; set; } = string.Empty;
    public decimal Preco { get; set; }
    public string ImagemUrl { get; set; } = string.Empty;
    public UsuarioDto? Usuario { get; set; }

    public string PrecoFormatado => Modalidade == "Doação" ? "GRÁTIS" : $"R$ {Preco:F2}";
    public string BadgeTexto => Modalidade == "Doação" ? "GRÁTIS" : Modalidade.ToUpper();
    public Color BadgeCor => Modalidade switch
    {
        "Doação" => Color.FromArgb("#A3D9B1"),
        "Troca" => Color.FromArgb("#D4EDE1"),
        _ => Color.FromArgb("#F4B942")
    };
}

public class GrupoEstudoDto
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Disciplina { get; set; } = string.Empty;
    public string Curso { get; set; } = string.Empty;
    public string Horario { get; set; } = string.Empty;
    public string Formato { get; set; } = string.Empty;
    public int VagasDisponiveis { get; set; }
    public bool Recomendado { get; set; }
    public string VagasTexto => $"{VagasDisponiveis} vagas";
}
