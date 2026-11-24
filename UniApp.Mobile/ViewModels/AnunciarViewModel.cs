using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using UniApp.Mobile.Models;
using UniApp.Mobile.Services;

namespace UniApp.Mobile.ViewModels;

public partial class AnunciarViewModel : ObservableObject
{
    private readonly ApiService _api;

    [ObservableProperty] private string titulo = string.Empty;
    [ObservableProperty] private string descricao = string.Empty;
    [ObservableProperty] private string categoria = "Livros";
    [ObservableProperty] private string curso = "ADS";
    [ObservableProperty] private string estado = "Bom estado";
    [ObservableProperty] private string modalidade = "Venda";
    [ObservableProperty] private string precoTexto = "0,00";

    public List<string> Categorias { get; } = new() { "Livros", "Calculadoras", "Laboratório", "Apostilas" };
    public List<string> Modalidades { get; } = new() { "Venda", "Troca", "Doação" };
    public List<string> Estados { get; } = new() { "Novo", "Muito bom", "Bom estado", "Usado" };

    public AnunciarViewModel(ApiService api) => _api = api;

    [RelayCommand]
    private async Task PublicarAsync()
    {
        if (string.IsNullOrWhiteSpace(Titulo))
        {
            await Shell.Current.DisplayAlert("Aviso", "Informe o título do material.", "OK");
            return;
        }

        decimal.TryParse(PrecoTexto.Replace(",", "."), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out var preco);

        var sucesso = await _api.CriarMaterialAsync(new MaterialDto
        {
            Titulo = Titulo,
            Descricao = Descricao,
            Categoria = Categoria,
            CursoRelacionado = Curso,
            EstadoConservacao = Estado,
            Modalidade = Modalidade,
            Preco = preco
        });

        if (sucesso)
        {
            await Shell.Current.DisplayAlert("Sucesso", "Material publicado na rede universitária!", "OK");
            Titulo = Descricao = string.Empty;
            PrecoTexto = "0,00";
            await Shell.Current.GoToAsync("//HomePage");
        }
    }
}
