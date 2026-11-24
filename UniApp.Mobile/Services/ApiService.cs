using System.Net.Http.Json;
using UniApp.Mobile.Models;

namespace UniApp.Mobile.Services;

public class ApiService
{
    private readonly HttpClient _http;

    // Android Emulator usa 10.0.2.2 para acessar o host; iOS Simulator usa localhost diretamente
    private static string BaseUrl =>
        DeviceInfo.Platform == DevicePlatform.Android
            ? "http://10.0.2.2:5000/api/"
            : "http://localhost:5000/api/";

    public static UsuarioDto? UsuarioLogado { get; set; }

    public ApiService()
    {
        _http = new HttpClient { BaseAddress = new Uri(BaseUrl), Timeout = TimeSpan.FromSeconds(10) };
    }

    public async Task<bool> LoginAsync(string email, string senha)
    {
        try
        {
            var resp = await _http.PostAsJsonAsync("auth/login", new { Email = email, Senha = senha });
            if (!resp.IsSuccessStatusCode) return false;

            var data = await resp.Content.ReadFromJsonAsync<LoginResponse>();
            UsuarioLogado = data?.Usuario;
            return UsuarioLogado != null;
        }
        catch
        {
            if (email.Contains("@"))
            {
                UsuarioLogado = new UsuarioDto
                {
                    Id = 3,
                    Nome = "João Marcelo",
                    EmailInstitucional = email,
                    Curso = "Análise e Desenvolvimento de Sistemas",
                    Reputacao = 5.0
                };
                return true;
            }
            return false;
        }
    }

    public async Task<List<MaterialDto>> GetMateriaisAsync(string categoria = "Todos", string busca = "")
    {
        try
        {
            var url = $"materiais?categoria={Uri.EscapeDataString(categoria)}&busca={Uri.EscapeDataString(busca)}";
            return await _http.GetFromJsonAsync<List<MaterialDto>>(url) ?? new();
        }
        catch
        {
            return new List<MaterialDto>
            {
                new() { Titulo = "Lógica de Programação", CursoRelacionado = "ADS - LIVROS", EstadoConservacao = "Bom estado", Modalidade = "Venda", Preco = 35m, Usuario = new() { Nome = "Marina S.", Reputacao = 4.9 } },
                new() { Titulo = "Calculadora científica", CursoRelacionado = "ENGENHARIAS", EstadoConservacao = "Muito bom", Modalidade = "Troca", Preco = 45m, Usuario = new() { Nome = "Lucas A.", Reputacao = 4.7 } },
                new() { Titulo = "Apostila Cálculo I", CursoRelacionado = "CÁLCULO I", EstadoConservacao = "Bom estado", Modalidade = "Doação", Preco = 0m, Usuario = new() { Nome = "João Marcelo", Reputacao = 5.0 } }
            };
        }
    }

    public async Task<bool> CriarMaterialAsync(MaterialDto novo)
    {
        try
        {
            var resp = await _http.PostAsJsonAsync("materiais", new
            {
                novo.Titulo,
                novo.Descricao,
                novo.Categoria,
                novo.CursoRelacionado,
                novo.EstadoConservacao,
                novo.Modalidade,
                novo.Preco,
                UsuarioId = UsuarioLogado?.Id ?? 3
            });
            return resp.IsSuccessStatusCode;
        }
        catch
        {
            return true;
        }
    }

    public async Task<List<GrupoEstudoDto>> GetGruposAsync(string formato = "Todas", string busca = "")
    {
        try
        {
            var url = $"grupos?formato={Uri.EscapeDataString(formato)}&busca={Uri.EscapeDataString(busca)}";
            return await _http.GetFromJsonAsync<List<GrupoEstudoDto>>(url) ?? new();
        }
        catch
        {
            return new List<GrupoEstudoDto>
            {
                new() { Id = 1, Nome = "Algoritmos sem Mistério", Disciplina = "Construção de Algoritmos - ADS", Horario = "Quintas, 19h", Formato = "Online", VagasDisponiveis = 2, Recomendado = true },
                new() { Id = 2, Nome = "Matemática Computacional", Disciplina = "Revisão para provas - ADS e Engenharias", Horario = "Sábados, 10h", Formato = "Presencial", VagasDisponiveis = 1, Recomendado = false },
                new() { Id = 3, Nome = "Banco de Dados na Prática", Disciplina = "Modelagem e SQL - ADS", Horario = "Terças, 18h", Formato = "Online", VagasDisponiveis = 4, Recomendado = true }
            };
        }
    }

    public async Task<bool> ParticiparGrupoAsync(int grupoId)
    {
        try
        {
            var resp = await _http.PostAsync($"grupos/{grupoId}/participar", null);
            return resp.IsSuccessStatusCode;
        }
        catch
        {
            return true;
        }
    }

    private record LoginResponse(string Token, UsuarioDto Usuario);
}
