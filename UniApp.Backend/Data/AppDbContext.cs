using Microsoft.EntityFrameworkCore;
using UniApp.Api.Models;

namespace UniApp.Api.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Usuario> Usuarios => Set<Usuario>();
    public DbSet<Material> Materiais => Set<Material>();
    public DbSet<GrupoEstudo> GruposEstudo => Set<GrupoEstudo>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Usuario>().HasData(
            new Usuario { Id = 1, Nome = "Marina S.", EmailInstitucional = "marina@unifacema.edu.br", SenhaHash = "123456", Curso = "ADS", Reputacao = 4.9, AvaliacoesCount = 12 },
            new Usuario { Id = 2, Nome = "Lucas A.", EmailInstitucional = "lucas@unifacema.edu.br", SenhaHash = "123456", Curso = "Engenharias", Reputacao = 4.7, AvaliacoesCount = 8 },
            new Usuario { Id = 3, Nome = "João Marcelo", EmailInstitucional = "joao.marcelo@unifacema.edu.br", SenhaHash = "123456", Curso = "ADS", Reputacao = 5.0, AvaliacoesCount = 15 }
        );

        modelBuilder.Entity<Material>().HasData(
            new Material
            {
                Id = 1,
                Titulo = "Lógica de Programação",
                Descricao = "Livro didático completo para algoritmos e lógica.",
                Categoria = "Livros",
                CursoRelacionado = "ADS",
                EstadoConservacao = "Bom estado",
                Modalidade = "Venda",
                Preco = 35.00m,
                UsuarioId = 1,
                ContatoWhatsApp = "5599999999999"
            },
            new Material
            {
                Id = 2,
                Titulo = "Calculadora científica",
                Descricao = "Modelo Casio fx-82MS, ideal para cálculo e física. Aceito troca por livro de BD.",
                Categoria = "Calculadoras",
                CursoRelacionado = "Engenharias",
                EstadoConservacao = "Muito bom",
                Modalidade = "Troca",
                Preco = 45.00m,
                UsuarioId = 2,
                ContatoWhatsApp = "5599988888888"
            },
            new Material
            {
                Id = 3,
                Titulo = "Apostila Cálculo I",
                Descricao = "Material impresso com exercícios resolvidos de limites e derivadas.",
                Categoria = "Apostilas",
                CursoRelacionado = "Cálculo I",
                EstadoConservacao = "Bom estado",
                Modalidade = "Doação",
                Preco = 0.00m,
                UsuarioId = 3,
                ContatoWhatsApp = "5599977777777"
            }
        );

        modelBuilder.Entity<GrupoEstudo>().HasData(
            new GrupoEstudo { Id = 1, Nome = "Algoritmos sem Mistério", Disciplina = "Construção de Algoritmos", Curso = "ADS", Horario = "Quintas, 19h", Formato = "Online", VagasDisponiveis = 2, Recomendado = true },
            new GrupoEstudo { Id = 2, Nome = "Matemática Computacional", Disciplina = "Revisão para provas", Curso = "ADS e Engenharias", Horario = "Sábados, 10h", Formato = "Presencial", VagasDisponiveis = 1, Recomendado = false },
            new GrupoEstudo { Id = 3, Nome = "Banco de Dados na Prática", Disciplina = "Modelagem e SQL", Curso = "ADS", Horario = "Terças, 18h", Formato = "Online", VagasDisponiveis = 4, Recomendado = true }
        );
    }
}
