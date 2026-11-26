using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using UniApp.Mobile.Services;

namespace UniApp.Mobile.ViewModels;

public partial class LoginViewModel : ObservableObject
{
    private readonly ApiService _api;

    [ObservableProperty] private string email = "joao.marcelo@unifacema.edu.br";
    [ObservableProperty] private string senha = "123456";
    [ObservableProperty] private bool lembrarMe = true;
    [ObservableProperty] private bool carregando;

    public LoginViewModel(ApiService api) => _api = api;

    [RelayCommand]
    private async Task EntrarAsync()
    {
        if (string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Senha))
        {
            await Shell.Current.DisplayAlertAsync("Atenção", "Preencha seu e-mail institucional e senha.", "OK");
            return;
        }

        Carregando = true;
        var ok = await _api.LoginAsync(Email, Senha);
        Carregando = false;

        if (ok)
        {
            if (_api.ModoDemo)
                await Shell.Current.DisplayAlertAsync("Modo demonstração",
                    "A API não está disponível. Os dados exibidos são de exemplo.", "OK");

            await Shell.Current.GoToAsync("//HomePage");
        }
        else
        {
            var mensagem = string.IsNullOrWhiteSpace(_api.UltimoErro)
                ? "Credenciais inválidas."
                : _api.UltimoErro;
            await Shell.Current.DisplayAlertAsync("Erro", mensagem, "OK");
        }
    }
}
