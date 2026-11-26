using Microsoft.Data.Sqlite;
using UniApp.Api.Infrastructure.Data;
using UniApp.Api.Infrastructure.Security;
using Xunit;

namespace UniApp.Backend.Tests;

public class DbInitializerTests
{
    [Fact]
    public async Task Initialize_AplicadoDuasVezes_NaoDuplicaSeed()
    {
        var name = $"uniapp-{Guid.NewGuid():N}";
        await using var connection = new SqliteConnection($"Data Source={name};Mode=Memory;Cache=Shared");
        await connection.OpenAsync();

        var options = new DbContextOptionsBuilder<AppDbContext>().UseSqlite(connection).Options;
        await using var db = new AppDbContext(options);
        var hasher = new PasswordHasher();

        await DbInitializer.InitializeAsync(db, hasher);
        await DbInitializer.InitializeAsync(db, hasher);

        Assert.Equal(3, await db.Usuarios.CountAsync());
        Assert.Equal(3, await db.Materiais.CountAsync());
        Assert.Equal(3, await db.GruposEstudo.CountAsync());

        var joao = await db.Usuarios.SingleAsync(u => u.EmailInstitucional == "joao.marcelo@unifacema.edu.br");
        Assert.True(hasher.Verify(joao.SenhaHash, "123456"));
    }
}
