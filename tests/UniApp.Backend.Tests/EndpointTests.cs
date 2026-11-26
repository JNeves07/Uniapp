using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using UniApp.Api;
using Xunit;

namespace UniApp.Backend.Tests;

/// <summary>
/// Testes de integracao dos endpoints atuais usando SQLite em arquivo temporario.
/// </summary>
public sealed class EndpointTests : IClassFixture<EndpointTests.ApiFactory>
{
    private readonly ApiFactory _factory;

    public EndpointTests(ApiFactory factory) => _factory = factory;

    [Fact]
    public async Task Health_RetornaOk()
    {
        var client = _factory.CreateClient();
        var response = await client.GetAsync("/health");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Registrar_EmailNaoInstitucional_Retorna400ComMensagem()
    {
        var client = _factory.CreateClient();
        var response = await client.PostAsJsonAsync("/api/auth/registrar", new
        {
            nome = "Teste", email = "teste@gmail.com", senha = "senha123", curso = "ADS"
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var json = await response.Content.ReadAsStringAsync();
        Assert.Contains(".edu.br", json);
        Assert.Contains("mensagem", json);
    }

    [Fact]
    public async Task Login_RetornaTokenEUsuario()
    {
        var client = _factory.CreateClient();
        var response = await client.PostAsJsonAsync("/api/auth/login", new
        {
            email = "joao.marcelo@unifacema.edu.br", senha = "123456"
        });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        Assert.False(string.IsNullOrWhiteSpace(json.RootElement.GetProperty("token").GetString()));
        Assert.Equal("João Marcelo", json.RootElement.GetProperty("usuario").GetProperty("nome").GetString());
        Assert.True(json.RootElement.TryGetProperty("refreshToken", out _));
    }

    [Fact]
    public async Task Login_SenhaErrada_Retorna401()
    {
        var client = _factory.CreateClient();
        var response = await client.PostAsJsonAsync("/api/auth/login", new
        {
            email = "joao.marcelo@unifacema.edu.br", senha = "errada"
        });
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Me_SemToken_Retorna401_ComToken_RetornaOk()
    {
        var client = _factory.CreateClient();
        var semToken = await client.GetAsync("/api/auth/me");
        Assert.Equal(HttpStatusCode.Unauthorized, semToken.StatusCode);

        var login = await client.PostAsJsonAsync("/api/auth/login", new
        {
            email = "joao.marcelo@unifacema.edu.br", senha = "123456"
        });
        using var json = JsonDocument.Parse(await login.Content.ReadAsStringAsync());
        var token = json.RootElement.GetProperty("token").GetString();

        client.DefaultRequestHeaders.Authorization = new("Bearer", token);
        var response = await client.GetAsync("/api/auth/me");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Refresh_ComRefreshToken_RetornaNovoToken()
    {
        var client = _factory.CreateClient();
        var login = await client.PostAsJsonAsync("/api/auth/login", new
        {
            email = "joao.marcelo@unifacema.edu.br", senha = "123456"
        });
        using var json = JsonDocument.Parse(await login.Content.ReadAsStringAsync());
        var refresh = json.RootElement.GetProperty("refreshToken").GetString();

        var response = await client.PostAsJsonAsync("/api/auth/refresh", new { refreshToken = refresh });
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Grupos_CriarListarEParticipar()
    {
        var client = _factory.CreateClient();
        var criacao = await client.PostAsJsonAsync("/api/grupos", new
        {
            nome = $"Grupo Integração {Guid.NewGuid():N}",
            disciplina = "SQL",
            curso = "ADS",
            horario = "Quintas, 19h",
            formato = "Online",
            vagasDisponiveis = 2,
            recomendado = true
        });
        Assert.Equal(HttpStatusCode.Created, criacao.StatusCode);

        using var criado = JsonDocument.Parse(await criacao.Content.ReadAsStringAsync());
        var id = criado.RootElement.GetProperty("id").GetInt32();

        var lista = await client.GetFromJsonAsync<JsonElement>("/api/grupos?formato=Online&busca=SQL");
        Assert.Contains(lista.EnumerateArray(), g => g.GetProperty("id").GetInt32() == id);

        var participacao = await client.PostAsync($"/api/grupos/{id}/participar", null);
        Assert.Equal(HttpStatusCode.OK, participacao.StatusCode);
        using var atualizado = JsonDocument.Parse(await participacao.Content.ReadAsStringAsync());
        Assert.Equal(1, atualizado.RootElement.GetProperty("vagasDisponiveis").GetInt32());
    }

    [Fact]
    public async Task Grupos_ParticiparInexistente_Retorna404()
    {
        var client = _factory.CreateClient();
        var response = await client.PostAsync("/api/grupos/999999/participar", null);
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Materiais_CriarEListar_SemExporSenha()
    {
        var client = _factory.CreateClient();
        var usuarios = await client.GetFromJsonAsync<JsonElement>("/api/grupos");
        Assert.True(usuarios.GetArrayLength() > 0);

        var criacao = await client.PostAsJsonAsync("/api/materiais", new
        {
            titulo = $"Livro Teste {Guid.NewGuid():N}",
            descricao = "Descricao",
            categoria = "Livros",
            cursoRelacionado = "ADS",
            estadoConservacao = "Bom estado",
            modalidade = "Venda",
            preco = 12.5m,
            usuarioId = 1
        });
        Assert.Equal(HttpStatusCode.Created, criacao.StatusCode);

        var lista = await client.GetFromJsonAsync<JsonElement>("/api/materiais?categoria=Livros");
        Assert.Contains(lista.EnumerateArray(), m => m.GetProperty("preco").GetDecimal() == 12.5m);
        Assert.DoesNotContain("senhaHash", (await criacao.Content.ReadAsStringAsync()).ToLowerInvariant());
    }

    public sealed class ApiFactory : WebApplicationFactory<Program>
    {
        private readonly string _databasePath = Path.Combine(Path.GetTempPath(), $"uniapp-it-{Guid.NewGuid():N}.db");

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("Testing");
            builder.UseSetting("Database:Provider", "Sqlite");
            builder.UseSetting("ConnectionStrings:DefaultConnection", $"Data Source={_databasePath}");
            builder.UseSetting("Jwt:Key", TestHelpers.JwtKey);
            builder.UseSetting("Jwt:Issuer", "UniAppApi");
            builder.UseSetting("Jwt:Audience", "UniAppMobile");
            builder.UseSetting("RateLimiting:Enabled", "false");
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (!disposing) return;
            try { File.Delete(_databasePath); } catch (IOException) { /* arquivo em uso pelo provider */ }
        }
    }
}
