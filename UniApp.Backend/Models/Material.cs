using System.ComponentModel.DataAnnotations;

namespace UniApp.Api.Models;

public class Material
{
    public int Id { get; set; }
    [Required] public string Titulo { get; set; } = string.Empty;
    public string Descricao { get; set; } = string.Empty;
    [Required] public string Categoria { get; set; } = "Livros";
    [Required] public string CursoRelacionado { get; set; } = "ADS";
    [Required] public string EstadoConservacao { get; set; } = "Bom estado";
    [Required] public string Modalidade { get; set; } = "Venda";
    public decimal Preco { get; set; }
    public string ImagemUrl { get; set; } = "https://images.unsplash.com/photo-1544716278-ca5e3f4abd8c?w=500";
    public string ContatoWhatsApp { get; set; } = string.Empty;
    public DateTime DataCriacao { get; set; } = DateTime.UtcNow;

    public int UsuarioId { get; set; }
    public Usuario? Usuario { get; set; }
}
