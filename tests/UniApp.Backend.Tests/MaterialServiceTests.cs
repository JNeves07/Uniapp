using UniApp.Api.Application.DTOs;
using UniApp.Api.Domain.Entities;
using UniApp.Api.Domain.Exceptions;
using UniApp.Api.Infrastructure.Data;
using UniApp.Api.Infrastructure.Security;
using Xunit;

namespace UniApp.Backend.Tests;

public class MaterialServiceTests
{
    private static async Task<(AppDbContext Db, int UsuarioId)> ComUsuarioAsync()
    {
        var db = TestHelpers.CreateInMemoryContext();
        await DbInitializer.SeedAsync(db, new PasswordHasher());
        return (db, await db.Usuarios.Select(u => u.Id).FirstAsync());
    }

    private static CriarMaterialRequest NovoMaterial(int usuarioId, string titulo = "Livro de ADS", string modalidade = "Venda", decimal preco = 20m)
        => new(titulo, "Descricao", "Livros", "ADS", "Bom estado", modalidade, preco, null, null, usuarioId);

    [Fact]
    public async Task CriarMaterial_PersisteEAssociaUsuario()
    {
        var (db, usuarioId) = await ComUsuarioAsync();
        var services = TestHelpers.CreateServices(db);

        var material = await services.Materiais.CriarAsync(NovoMaterial(usuarioId));

        Assert.True(material.Id > 0);
        Assert.Equal(20m, material.Preco);
        Assert.NotNull(material.Usuario);
        Assert.Equal(usuarioId, (await db.Materiais.SingleAsync(m => m.Id == material.Id)).UsuarioId);
    }

    [Fact]
    public async Task CriarMaterial_Doacao_ForcaPrecoZero()
    {
        var (db, usuarioId) = await ComUsuarioAsync();
        var services = TestHelpers.CreateServices(db);

        var material = await services.Materiais.CriarAsync(NovoMaterial(usuarioId, "Apostila", "Doação", 50m));

        Assert.Equal(0m, material.Preco);
        Assert.Equal("Doação", material.Modalidade);
    }

    [Fact]
    public async Task CriarMaterial_UsuarioInexistente_LancaNotFound()
    {
        var (db, _) = await ComUsuarioAsync();
        var services = TestHelpers.CreateServices(db);

        await Assert.ThrowsAsync<NotFoundException>(() => services.Materiais.CriarAsync(NovoMaterial(9999)));
    }

    [Fact]
    public async Task Listar_FiltraPorCategoriaEBusca()
    {
        var (db, usuarioId) = await ComUsuarioAsync();
        var services = TestHelpers.CreateServices(db);
        await services.Materiais.CriarAsync(NovoMaterial(usuarioId, "Livro de ADS"));
        await services.Materiais.CriarAsync(new CriarMaterialRequest("Calculadora", null, "Calculadoras", "Engenharias", null, "Troca", 100m, null, null, usuarioId));

        var livros = await services.Materiais.ListAsync("Livros", null);
        Assert.Contains(livros, m => m.Titulo == "Livro de ADS");
        Assert.DoesNotContain(livros, m => m.Categoria != "Livros");

        var porBusca = await services.Materiais.ListAsync("Todos", "Calculadora");
        Assert.Contains(porBusca, m => m.Titulo == "Calculadora");
        Assert.DoesNotContain(porBusca, m => m.Categoria != "Calculadoras");
        Assert.All(porBusca, m => Assert.NotNull(m.Usuario));
    }

    [Fact]
    public async Task CriarMaterial_TituloVazio_Rejeita()
    {
        var (db, usuarioId) = await ComUsuarioAsync();
        var services = TestHelpers.CreateServices(db);

        await Assert.ThrowsAsync<FluentValidation.ValidationException>(() =>
            services.Materiais.CriarAsync(NovoMaterial(usuarioId, "")));
    }
}
