using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using UniApp.Mobile.Models;
using UniApp.Mobile.Services;

namespace UniApp.Mobile.ViewModels;

public partial class HomeViewModel : ObservableObject
{
    private readonly ApiService _api;

    public ObservableCollection<MaterialDto> Materiais { get; } = new();
    public List<string> Categorias { get; } = new() { "Todos", "Livros", "Calculadoras", "Laboratório", "Apostilas" };

    [ObservableProperty] private string categoriaSelecionada = "Todos";
    [ObservableProperty] private string textoBusca = string.Empty;

    public HomeViewModel(ApiService api) => _api = api;

    [RelayCommand]
    public async Task CarregarMateriaisAsync()
    {
        var lista = await _api.GetMateriaisAsync(CategoriaSelecionada, TextoBusca);
        Materiais.Clear();
        foreach (var item in lista)
            Materiais.Add(item);
    }

    [RelayCommand]
    private async Task FiltrarCategoriaAsync(string categoria)
    {
        CategoriaSelecionada = categoria;
        await CarregarMateriaisAsync();
    }
}
