using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using UniApp.Mobile.Models;
using UniApp.Mobile.Services;

namespace UniApp.Mobile.ViewModels;

public partial class GruposViewModel : ObservableObject
{
    private readonly ApiService _api;

    public ObservableCollection<GrupoEstudoDto> Grupos { get; } = new();
    public List<string> Formatos { get; } = new() { "Todas", "Online", "Presencial" };

    [ObservableProperty] private string formatoSelecionado = "Todas";
    [ObservableProperty] private string buscaDisciplina = string.Empty;

    public GruposViewModel(ApiService api) => _api = api;

    [RelayCommand]
    public async Task CarregarGruposAsync()
    {
        var lista = await _api.GetGruposAsync(FormatoSelecionado, BuscaDisciplina);
        Grupos.Clear();
        foreach (var g in lista)
            Grupos.Add(g);
    }

    [RelayCommand]
    private async Task FiltrarFormatoAsync(string formato)
    {
        FormatoSelecionado = formato;
        await CarregarGruposAsync();
    }

    [RelayCommand]
    private async Task ParticiparGrupoAsync(GrupoEstudoDto grupo)
    {
        var ok = await _api.ParticiparGrupoAsync(grupo.Id);
        if (ok)
        {
            await Shell.Current.DisplayAlertAsync("Inscrição Confirmada", $"Você entrou no grupo '{grupo.Nome}' ({grupo.Horario}).", "OK");
            await CarregarGruposAsync();
        }
        else
        {
            var mensagem = string.IsNullOrWhiteSpace(_api.UltimoErro)
                ? "Não há vagas disponíveis neste grupo."
                : _api.UltimoErro;
            await Shell.Current.DisplayAlertAsync("Aviso", mensagem, "OK");
        }
    }
}
