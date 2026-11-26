using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using UniApp.Mobile.Services;

namespace UniApp.Mobile.ViewModels;

public partial class PerfilViewModel : ObservableObject
{
    private readonly ApiService _api;

    [ObservableProperty] private string nome = "João Marcelo";
    [ObservableProperty] private string email = "joao.marcelo@unifacema.edu.br";
    [ObservableProperty] private string curso = "Análise e Desenvolvimento de Sistemas";
    [ObservableProperty] private string reputacaoTexto = "5.0 / 5.0 (15 avaliações)";

    public PerfilViewModel(ApiService api) => _api = api;

    public void AtualizarDados()
    {
        if (ApiService.UsuarioLogado != null)
        {
            Nome = ApiService.UsuarioLogado.Nome;
            Email = ApiService.UsuarioLogado.EmailInstitucional;
            Curso = ApiService.UsuarioLogado.Curso;
            ReputacaoTexto = $"{ApiService.UsuarioLogado.Reputacao:F1} / 5.0";
        }
    }

    [RelayCommand]
    private async Task SairAsync()
    {
        await _api.LogoutAsync();
        await Shell.Current.GoToAsync("//LoginPage");
    }
}
