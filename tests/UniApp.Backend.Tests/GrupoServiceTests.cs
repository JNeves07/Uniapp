using UniApp.Api.Application.DTOs;
using UniApp.Api.Domain.Exceptions;
using Xunit;

namespace UniApp.Backend.Tests;

public class GrupoServiceTests
{
    private static CriarGrupoRequest NovoGrupo(string nome = "Estudo de SQL", string formato = "Online", int vagas = 4)
        => new(nome, "Banco de Dados", "ADS", "Terças, 18h", formato, vagas, true);

    [Fact]
    public async Task CriarGrupo_PersisteERetornaResponse()
    {
        var services = TestHelpers.CreateServices(TestHelpers.CreateInMemoryContext());

        var grupo = await services.Grupos.CriarAsync(NovoGrupo());

        Assert.True(grupo.Id > 0);
        Assert.Equal("Estudo de SQL", grupo.Nome);
        Assert.Equal(4, grupo.VagasDisponiveis);
        Assert.Single(await services.Db.GruposEstudo.ToListAsync());
    }

    [Fact]
    public async Task Listar_FiltraPorFormatoEBusca()
    {
        var services = TestHelpers.CreateServices(TestHelpers.CreateInMemoryContext());
        await services.Grupos.CriarAsync(NovoGrupo("Grupo Online SQL", "Online"));
        await services.Grupos.CriarAsync(NovoGrupo("Grupo Presencial C", "Presencial"));

        var online = await services.Grupos.ListAsync("Online", null);
        Assert.Single(online);
        Assert.Equal("Grupo Online SQL", online[0].Nome);

        var porBusca = await services.Grupos.ListAsync("Todas", "Presencial");
        Assert.Single(porBusca);
    }

    [Fact]
    public async Task Participar_DecrementaVagas()
    {
        var services = TestHelpers.CreateServices(TestHelpers.CreateInMemoryContext());
        var criado = await services.Grupos.CriarAsync(NovoGrupo(vagas: 2));

        var atualizado = await services.Grupos.ParticiparAsync(criado.Id);

        Assert.Equal(1, atualizado.VagasDisponiveis);
        Assert.Equal(1, (await services.Db.GruposEstudo.SingleAsync()).VagasDisponiveis);
    }

    [Fact]
    public async Task Participar_GrupoLotado_LancaDomainException()
    {
        var services = TestHelpers.CreateServices(TestHelpers.CreateInMemoryContext());
        var criado = await services.Grupos.CriarAsync(NovoGrupo(vagas: 0));

        await Assert.ThrowsAsync<DomainException>(() => services.Grupos.ParticiparAsync(criado.Id));
    }

    [Fact]
    public async Task Participar_GrupoInexistente_LancaNotFound()
        => await Assert.ThrowsAsync<NotFoundException>(async () =>
        {
            var services = TestHelpers.CreateServices(TestHelpers.CreateInMemoryContext());
            await services.Grupos.ParticiparAsync(999);
        });

    [Fact]
    public async Task CriarGrupo_FormatoInvalido_Rejeita()
        => await Assert.ThrowsAsync<FluentValidation.ValidationException>(async () =>
        {
            var services = TestHelpers.CreateServices(TestHelpers.CreateInMemoryContext());
            await services.Grupos.CriarAsync(NovoGrupo(formato: "Híbrido"));
        });
}
