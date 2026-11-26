using System.ComponentModel.DataAnnotations;

namespace UniApp.Api.Domain.Entities;

/// <summary>
/// Material acadêmico anunciado no marketplace (venda, troca ou doação).
/// </summary>
public class Material
{
    public int Id { get; set; }

    [Required]
    [MaxLength(180)]
    public string Titulo { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string Descricao { get; set; } = string.Empty;

    [Required]
    [MaxLength(80)]
    public string Categoria { get; set; } = "Livros";

    [Required]
    [MaxLength(120)]
    public string CursoRelacionado { get; set; } = "ADS";

    [Required]
    [MaxLength(80)]
    public string EstadoConservacao { get; set; } = "Bom estado";

    [Required]
    [MaxLength(40)]
    public string Modalidade { get; set; } = "Venda";

    [Range(typeof(decimal), "0", "79228162514264337593543950335")]
    public decimal Preco { get; set; }

    [MaxLength(500)]
    public string ImagemUrl { get; set; } = "https://images.unsplash.com/photo-1544716278-ca5e3f4abd8c?w=500";

    [MaxLength(40)]
    public string ContatoWhatsApp { get; set; } = string.Empty;

    public DateTime DataCriacao { get; set; } = DateTime.UtcNow;

    public int UsuarioId { get; set; }

    public Usuario? Usuario { get; set; }
}
