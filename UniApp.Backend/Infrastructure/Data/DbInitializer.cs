using Microsoft.EntityFrameworkCore;
using UniApp.Api.Domain.Entities;
using UniApp.Api.Domain.Services;

namespace UniApp.Api.Infrastructure.Data;

/// <summary>
/// Aplica migracoes e garante o seed de demonstracao de forma idempotente.
/// </summary>
public static class DbInitializer
{
    private static readonly string[] SeedEmails =
    [
        "marina@unifacema.edu.br",
        "lucas@unifacema.edu.br",
        "joao.marcelo@unifacema.edu.br"
    ];

    private const string SenhaDemo = "123456";

    public static async Task InitializeAsync(AppDbContext context, IPasswordHasher hasher, CancellationToken cancellationToken = default)
    {
        if (context.Database.IsRelational())
        {
            // SQLite e usado apenas em testes/demo; o schema e criado diretamente pelo provider.
            if (context.Database.ProviderName?.Contains("Sqlite", StringComparison.OrdinalIgnoreCase) == true)
            {
                await context.Database.EnsureCreatedAsync(cancellationToken);
            }
            else
            {
                // Bancos criados pela versao anterior (EnsureCreated) recebem um baseline
                // para que a migracao inicial nao tente recriar as tabelas existentes.
                try
                {
                    await BaselinesBancoLegadoAsync(context, cancellationToken);
                }
                catch (System.Data.Common.DbException)
                {
                    // Banco ainda nao existe: MigrateAsync o cria.
                }

                await context.Database.MigrateAsync(cancellationToken);
            }
        }

        await SeedAsync(context, hasher, cancellationToken);
    }

    private static async Task BaselinesBancoLegadoAsync(AppDbContext context, CancellationToken cancellationToken)
    {
        const string tabelaCriada = """
            SELECT COUNT(*) FROM information_schema.tables
            WHERE table_schema = DATABASE() AND table_name = 'Usuarios'
            """;
        const string historicoCriado = """
            SELECT COUNT(*) FROM information_schema.tables
            WHERE table_schema = DATABASE() AND table_name = '__EFMigrationsHistory'
            """;

        var existeTabela = await context.Database.SqlQueryRaw<int>(tabelaCriada).SingleAsync(cancellationToken) > 0;
        if (!existeTabela)
            return;

        var existeHistorico = await context.Database.SqlQueryRaw<int>(historicoCriado).SingleAsync(cancellationToken) > 0;
        if (existeHistorico)
            return;

        var migracaoInicial = context.Database.GetMigrations().FirstOrDefault();
        if (migracaoInicial is null)
            return;

        await context.Database.ExecuteSqlRawAsync("""
            CREATE TABLE IF NOT EXISTS `__EFMigrationsHistory` (
                `MigrationId` varchar(150) NOT NULL,
                `ProductVersion` varchar(32) NOT NULL,
                PRIMARY KEY (`MigrationId`)
            ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4
            """, cancellationToken);

        await context.Database.ExecuteSqlRawAsync(
            "INSERT IGNORE INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`) VALUES ({0}, {1})",
            [migracaoInicial, "8.0.10"], cancellationToken);
    }

    /// <summary>
    /// Insere os registros de exemplo apenas quando ainda nao existem. Pode ser chamado varias vezes.
    /// </summary>
    public static async Task SeedAsync(AppDbContext context, IPasswordHasher hasher, CancellationToken cancellationToken = default)
    {
        foreach (var email in SeedEmails)
        {
            if (await context.Usuarios.AnyAsync(u => u.EmailInstitucional == email, cancellationToken))
                continue;

            var usuario = new Usuario
            {
                Nome = email.StartsWith("marina", StringComparison.Ordinal) ? "Marina S."
                    : email.StartsWith("lucas", StringComparison.Ordinal) ? "Lucas A." : "João Marcelo",
                EmailInstitucional = email,
                SenhaHash = hasher.Hash(SenhaDemo),
                Curso = email.StartsWith("lucas", StringComparison.Ordinal) ? "Engenharias" : "ADS",
                Reputacao = email.StartsWith("marina", StringComparison.Ordinal) ? 4.9
                    : email.StartsWith("lucas", StringComparison.Ordinal) ? 4.7 : 5.0,
                AvaliacoesCount = email.StartsWith("marina", StringComparison.Ordinal) ? 12
                    : email.StartsWith("lucas", StringComparison.Ordinal) ? 8 : 15
            };

            context.Usuarios.Add(usuario);
            await context.SaveChangesAsync(cancellationToken);
        }

        await SeedMateriaisAsync(context, cancellationToken);
        await SeedGruposAsync(context, cancellationToken);
    }

    private static async Task SeedMateriaisAsync(AppDbContext context, CancellationToken cancellationToken)
    {
        var marina = await context.Usuarios.FirstAsync(u => u.EmailInstitucional == SeedEmails[0], cancellationToken);
        var lucas = await context.Usuarios.FirstAsync(u => u.EmailInstitucional == SeedEmails[1], cancellationToken);
        var joao = await context.Usuarios.FirstAsync(u => u.EmailInstitucional == SeedEmails[2], cancellationToken);

        await SeedMaterialAsync(context, marina.Id, "Lógica de Programação",
            "Livro didático completo para algoritmos e lógica.", "Livros", "ADS", "Bom estado", "Venda", 35.00m,
            "5599999999999", cancellationToken);

        await SeedMaterialAsync(context, lucas.Id, "Calculadora científica",
            "Modelo Casio fx-82MS, ideal para cálculo e física. Aceito troca por livro de BD.", "Calculadoras", "Engenharias", "Muito bom", "Troca", 45.00m,
            "5599988888888", cancellationToken);

        await SeedMaterialAsync(context, joao.Id, "Apostila Cálculo I",
            "Material impresso com exercícios resolvidos de limites e derivadas.", "Apostilas", "Cálculo I", "Bom estado", "Doação", 0m,
            "5599977777777", cancellationToken);
    }

    private static async Task SeedMaterialAsync(
        AppDbContext context, int usuarioId, string titulo, string descricao, string categoria,
        string curso, string estado, string modalidade, decimal preco, string whatsapp,
        CancellationToken cancellationToken)
    {
        if (await context.Materiais.AnyAsync(m => m.Titulo == titulo && m.UsuarioId == usuarioId, cancellationToken))
            return;

        context.Materiais.Add(new Material
        {
            Titulo = titulo,
            Descricao = descricao,
            Categoria = categoria,
            CursoRelacionado = curso,
            EstadoConservacao = estado,
            Modalidade = modalidade,
            Preco = preco,
            UsuarioId = usuarioId,
            ContatoWhatsApp = whatsapp,
            DataCriacao = DateTime.UtcNow
        });

        await context.SaveChangesAsync(cancellationToken);
    }

    private static async Task SeedGruposAsync(AppDbContext context, CancellationToken cancellationToken)
    {
        await SeedGrupoAsync(context, "Algoritmos sem Mistério", "Construção de Algoritmos", "ADS",
            "Quintas, 19h", "Online", 2, true, cancellationToken);
        await SeedGrupoAsync(context, "Matemática Computacional", "Revisão para provas", "ADS e Engenharias",
            "Sábados, 10h", "Presencial", 1, false, cancellationToken);
        await SeedGrupoAsync(context, "Banco de Dados na Prática", "Modelagem e SQL", "ADS",
            "Terças, 18h", "Online", 4, true, cancellationToken);
    }

    private static async Task SeedGrupoAsync(
        AppDbContext context, string nome, string disciplina, string curso, string horario,
        string formato, int vagas, bool recomendado, CancellationToken cancellationToken)
    {
        if (await context.GruposEstudo.AnyAsync(g => g.Nome == nome, cancellationToken))
            return;

        context.GruposEstudo.Add(new GrupoEstudo
        {
            Nome = nome,
            Disciplina = disciplina,
            Curso = curso,
            Horario = horario,
            Formato = formato,
            VagasDisponiveis = vagas,
            Recomendado = recomendado
        });

        await context.SaveChangesAsync(cancellationToken);
    }
}
