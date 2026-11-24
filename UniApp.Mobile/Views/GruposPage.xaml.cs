using UniApp.Mobile.ViewModels;

namespace UniApp.Mobile.Views;

public partial class GruposPage : ContentPage
{
    private readonly GruposViewModel _vm;

    public GruposPage(GruposViewModel vm)
    {
        InitializeComponent();
        BindingContext = _vm = vm;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _vm.CarregarGruposCommand.Execute(null);
    }
}
