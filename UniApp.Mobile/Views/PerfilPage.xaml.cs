using UniApp.Mobile.ViewModels;

namespace UniApp.Mobile.Views;

public partial class PerfilPage : ContentPage
{
    private readonly PerfilViewModel _vm;

    public PerfilPage(PerfilViewModel vm)
    {
        InitializeComponent();
        BindingContext = _vm = vm;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _vm.AtualizarDados();
    }
}
