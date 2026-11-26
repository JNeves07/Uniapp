using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using UniApp.Mobile.Models;

namespace UniApp.Mobile.Services;

/// <summary>
/// Cliente HTTP da API. Mantem as assinaturas usadas pelos ViewModels MVVM e adiciona:
/// timeout, armazenamento seguro do token, refresh automatico em 401 e mensagens amigaveis.
/// </summary>
public class ApiService
{
    private const string AccessTokenKey = "uniapp_access_token";
    private const string RefreshTokenKey = "uniapp_refresh_token";
    private const string BaseUrlKey = "uniapp_api_base_url";

    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    // Android Emulator usa 10.0.2.2 para acessar o host; iOS Simulator usa localhost diretamente.
    private static string BaseUrl =>
        Preferences.Default.Get(BaseUrlKey,
            DeviceInfo.Platform == DevicePlatform.Android
                ? "http://10.0.2.2:5000/api/"
                : "http://localhost:5000/api/");

    private readonly HttpClient _http;
    private readonly SemaphoreSlim _refreshLock = new(1, 1);

    private string? _accessToken;
    private string? _refreshToken;
    private bool _tokensCarregados;

    /// <summary>Usuario autenticado (compativel com PerfilViewModel).</summary>
    public static UsuarioDto? UsuarioLogado { get; set; }

    /// <summary>Ultima mensagem de erro amigavel gerada pelo servico.</summary>
    public string? UltimoErro { get; private set; }

    /// <summary>Indica que os dados exibidos vieram do modo demonstracao (API indisponivel).</summary>
    public bool ModoDemo { get; private set; }

    public ApiService()
    {
        var url = BaseUrl.EndsWith('/') ? BaseUrl : BaseUrl + "/";
        _http = new HttpClient { BaseAddress = new Uri(url), Timeout = TimeSpan.FromSeconds(15) };
        _http.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
    }

    /// <summary>Permite substituir a URL da API (util para dispositivo fisico na mesma rede).</summary>
    public static void DefinirBaseUrl(string url) => Preferences.Default.Set(BaseUrlKey, url);

    public void LimparErro() => UltimoErro = null;

    /// <summary>Encerra a sessao removendo os tokens armazenados.</summary>
    public async Task LogoutAsync()
    {
        await LimparTokensAsync();
        UsuarioLogado = null;
        ModoDemo = false;
        LimparErro();
    }

    public async Task<bool> LoginAsync(string email, string senha)
    {
        LimparErro();

        try
        {
            using var resp = await _http.PostAsJsonAsync("auth/login", new { Email = email, Senha = senha }, JsonOptions);

            if (resp.IsSuccessStatusCode)
            {
                var data = await resp.Content.ReadFromJsonAsync<AuthResponse>(JsonOptions);
                if (data?.Usuario is null || string.IsNullOrWhiteSpace(data.Token))
                {
                    UltimoErro = "A API retornou uma resposta inesperada. Tente novamente.";
                    return false;
                }

                await SalvarTokensAsync(data.Token, data.RefreshToken);
                UsuarioLogado = data.Usuario;
                ModoDemo = false;
                return true;
            }

            UsuarioLogado = null;
            UltimoErro = resp.StatusCode == HttpStatusCode.Unauthorized
                ? "E-mail institucional ou senha inválidos."
                : await MensagemErroAsync(resp);
            return false;
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException or TimeoutException)
        {
            // Mantem o modo demonstracao do app quando a API nao esta disponivel.
            ModoDemo = true;
            UsuarioLogado = new UsuarioDto
            {
                Id = 3,
                Nome = "João Marcelo",
                EmailInstitucional = email,
                Curso = "Análise e Desenvolvimento de Sistemas",
                Reputacao = 5.0
            };
            UltimoErro = "API indisponível. Você está no modo demonstração com dados de exemplo.";
            return true;
        }
        catch (Exception ex)
        {
            UltimoErro = $"Não foi possível entrar: {ex.Message}";
            return false;
        }
    }

    public async Task<List<MaterialDto>> GetMateriaisAsync(string categoria = "Todos", string busca = "")
    {
        LimparErro();

        try
        {
            var url = $"materiais?categoria={Uri.EscapeDataString(categoria)}&busca={Uri.EscapeDataString(busca)}";
            using var resp = await EnviarComRefreshAsync<object>(HttpMethod.Get, url);

            if (!resp.IsSuccessStatusCode)
            {
                UltimoErro = await MensagemErroAsync(resp);
                return MateriaisExemplo();
            }

            var lista = await resp.Content.ReadFromJsonAsync<List<MaterialDto>>(JsonOptions);
            if (lista is null)
            {
                UltimoErro = "Não foi possível interpretar a lista de materiais.";
                return MateriaisExemplo();
            }

            ModoDemo = false;
            return lista;
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException or TimeoutException)
        {
            ModoDemo = true;
            UltimoErro = "Sem conexão com a API. Exibindo materiais de exemplo.";
            return MateriaisExemplo();
        }
        catch (Exception ex)
        {
            UltimoErro = $"Erro ao carregar materiais: {ex.Message}";
            return new List<MaterialDto>();
        }
    }

    public async Task<bool> CriarMaterialAsync(MaterialDto novo)
    {
        LimparErro();

        try
        {
            using var resp = await EnviarComRefreshAsync(HttpMethod.Post, "materiais", new
            {
                novo.Titulo,
                novo.Descricao,
                novo.Categoria,
                novo.CursoRelacionado,
                novo.EstadoConservacao,
                novo.Modalidade,
                novo.Preco,
                UsuarioId = UsuarioLogado?.Id ?? 1
            });

            if (resp.IsSuccessStatusCode) return true;

            UltimoErro = await MensagemErroAsync(resp);
            return false;
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException or TimeoutException)
        {
            UltimoErro = "Não foi possível publicar: sem conexão com a API. Tente novamente.";
            return false;
        }
        catch (Exception ex)
        {
            UltimoErro = $"Não foi possível publicar: {ex.Message}";
            return false;
        }
    }

    public async Task<List<GrupoEstudoDto>> GetGruposAsync(string formato = "Todas", string busca = "")
    {
        LimparErro();

        try
        {
            var url = $"grupos?formato={Uri.EscapeDataString(formato)}&busca={Uri.EscapeDataString(busca)}";
            using var resp = await EnviarComRefreshAsync<object>(HttpMethod.Get, url);

            if (!resp.IsSuccessStatusCode)
            {
                UltimoErro = await MensagemErroAsync(resp);
                return GruposExemplo();
            }

            var lista = await resp.Content.ReadFromJsonAsync<List<GrupoEstudoDto>>(JsonOptions);
            if (lista is null)
            {
                UltimoErro = "Não foi possível interpretar a lista de grupos.";
                return GruposExemplo();
            }

            ModoDemo = false;
            return lista;
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException or TimeoutException)
        {
            ModoDemo = true;
            UltimoErro = "Sem conexão com a API. Exibindo grupos de exemplo.";
            return GruposExemplo();
        }
        catch (Exception ex)
        {
            UltimoErro = $"Erro ao carregar grupos: {ex.Message}";
            return new List<GrupoEstudoDto>();
        }
    }

    public async Task<bool> ParticiparGrupoAsync(int grupoId)
    {
        LimparErro();

        try
        {
            using var resp = await EnviarComRefreshAsync<object>(HttpMethod.Post, $"grupos/{grupoId}/participar");
            if (resp.IsSuccessStatusCode) return true;

            UltimoErro = resp.StatusCode == HttpStatusCode.NotFound
                ? "Este grupo não existe mais."
                : await MensagemErroAsync(resp);
            return false;
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException or TimeoutException)
        {
            UltimoErro = "Não foi possível confirmar a inscrição: sem conexão com a API.";
            return false;
        }
        catch (Exception ex)
        {
            UltimoErro = $"Não foi possível confirmar a inscrição: {ex.Message}";
            return false;
        }
    }

    // ------------------------------- Token ---------------------------------

    private async Task CarregarTokensAsync()
    {
        if (_tokensCarregados) return;

        try
        {
            _accessToken = await SecureStorage.Default.GetAsync(AccessTokenKey);
            _refreshToken = await SecureStorage.Default.GetAsync(RefreshTokenKey);
        }
        catch
        {
            // SecureStorage pode nao estar disponivel em alguns emuladores.
            _accessToken = _refreshToken = null;
        }

        _tokensCarregados = true;
        AplicarToken();
    }

    private void AplicarToken()
        => _http.DefaultRequestHeaders.Authorization = string.IsNullOrWhiteSpace(_accessToken)
            ? null
            : new AuthenticationHeaderValue("Bearer", _accessToken);

    private async Task SalvarTokensAsync(string accessToken, string? refreshToken)
    {
        _accessToken = accessToken;
        if (!string.IsNullOrWhiteSpace(refreshToken)) _refreshToken = refreshToken;

        AplicarToken();

        try
        {
            await SecureStorage.Default.SetAsync(AccessTokenKey, accessToken);
            if (!string.IsNullOrWhiteSpace(refreshToken))
                await SecureStorage.Default.SetAsync(RefreshTokenKey, refreshToken);
        }
        catch
        {
            // Sem armazenamento seguro a sessao dura apenas enquanto o app esta em memoria.
        }
    }

    private async Task LimparTokensAsync()
    {
        _accessToken = _refreshToken = null;
        AplicarToken();

        try
        {
            SecureStorage.Default.Remove(AccessTokenKey);
            SecureStorage.Default.Remove(RefreshTokenKey);
        }
        catch
        {
            // Ignorado: falha ao limpar nao impede o logout na memoria.
        }

        await Task.CompletedTask;
    }

    private async Task<bool> RenovarTokenAsync()
    {
        if (string.IsNullOrWhiteSpace(_refreshToken)) return false;

        await _refreshLock.WaitAsync();
        try
        {
            using var resp = await _http.PostAsJsonAsync(
                "auth/refresh",
                new { RefreshToken = _refreshToken },
                JsonOptions);

            if (!resp.IsSuccessStatusCode)
            {
                await LimparTokensAsync();
                return false;
            }

            var data = await resp.Content.ReadFromJsonAsync<AuthResponse>(JsonOptions);
            if (data is null || string.IsNullOrWhiteSpace(data.Token)) return false;

            await SalvarTokensAsync(data.Token, data.RefreshToken);
            return true;
        }
        catch
        {
            return false;
        }
        finally
        {
            _refreshLock.Release();
        }
    }

    private async Task<HttpResponseMessage> EnviarComRefreshAsync<T>(HttpMethod method, string url, T? corpo = default)
    {
        await CarregarTokensAsync();

        var resp = await EnviarAsync(method, url, corpo);
        if (resp.StatusCode != HttpStatusCode.Unauthorized || string.IsNullOrWhiteSpace(_refreshToken))
            return resp;

        resp.Dispose();
        return await RenovarTokenAsync()
            ? await EnviarAsync(method, url, corpo)
            : new HttpResponseMessage(HttpStatusCode.Unauthorized);
    }

    private async Task<HttpResponseMessage> EnviarAsync<T>(HttpMethod method, string url, T? corpo)
    {
        using var request = new HttpRequestMessage(method, url);
        if (corpo is not null)
            request.Content = JsonContent.Create(corpo, options: JsonOptions);

        return await _http.SendAsync(request);
    }

    private static async Task<string> MensagemErroAsync(HttpResponseMessage resposta)
    {
        var conteudo = await resposta.Content.ReadAsStringAsync();

        if (!string.IsNullOrWhiteSpace(conteudo))
        {
            try
            {
                using var json = JsonDocument.Parse(conteudo);
                var raiz = json.RootElement;

                foreach (var propriedade in new[] { "mensagem", "detail", "title", "message" })
                {
                    if (raiz.TryGetProperty(propriedade, out var valor) && valor.ValueKind == JsonValueKind.String)
                    {
                        var texto = valor.GetString();
                        if (!string.IsNullOrWhiteSpace(texto)) return texto!;
                    }
                }
            }
            catch (JsonException)
            {
                // Conteudo nao e JSON: usa mensagem por status code.
            }
        }

        return resposta.StatusCode switch
        {
            HttpStatusCode.Unauthorized => "Sua sessão expirou. Faça login novamente.",
            HttpStatusCode.Forbidden => "Você não tem permissão para esta operação.",
            HttpStatusCode.NotFound => "O recurso solicitado não foi encontrado.",
            HttpStatusCode.BadRequest => "Verifique os dados informados e tente novamente.",
            HttpStatusCode.TooManyRequests => "Muitas tentativas. Aguarde um momento e tente novamente.",
            HttpStatusCode.InternalServerError => "O servidor está temporariamente indisponível.",
            _ => $"Não foi possível concluir a operação ({(int)resposta.StatusCode})."
        };
    }

    // --------------------------- Dados de exemplo --------------------------

    private static List<MaterialDto> MateriaisExemplo() =>
    [
        new() { Titulo = "Lógica de Programação", CursoRelacionado = "ADS - LIVROS", EstadoConservacao = "Bom estado", Modalidade = "Venda", Preco = 35m, Usuario = new() { Nome = "Marina S.", Reputacao = 4.9 } },
        new() { Titulo = "Calculadora científica", CursoRelacionado = "ENGENHARIAS", EstadoConservacao = "Muito bom", Modalidade = "Troca", Preco = 45m, Usuario = new() { Nome = "Lucas A.", Reputacao = 4.7 } },
        new() { Titulo = "Apostila Cálculo I", CursoRelacionado = "CÁLCULO I", EstadoConservacao = "Bom estado", Modalidade = "Doação", Preco = 0m, Usuario = new() { Nome = "João Marcelo", Reputacao = 5.0 } }
    ];

    private static List<GrupoEstudoDto> GruposExemplo() =>
    [
        new() { Id = 1, Nome = "Algoritmos sem Mistério", Disciplina = "Construção de Algoritmos - ADS", Horario = "Quintas, 19h", Formato = "Online", VagasDisponiveis = 2, Recomendado = true },
        new() { Id = 2, Nome = "Matemática Computacional", Disciplina = "Revisão para provas - ADS e Engenharias", Horario = "Sábados, 10h", Formato = "Presencial", VagasDisponiveis = 1, Recomendado = false },
        new() { Id = 3, Nome = "Banco de Dados na Prática", Disciplina = "Modelagem e SQL - ADS", Horario = "Terças, 18h", Formato = "Online", VagasDisponiveis = 4, Recomendado = true }
    ];
}
