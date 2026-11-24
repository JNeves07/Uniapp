using UniApp.Mobile.ViewModels;

namespace UniApp.Mobile.Views;

public partial class HomePage : ContentPage
{
    private readonly HomeViewModel _vm;

    public HomePage(HomeViewModel vm)
    {
        InitializeComponent();
        BindingContext = _vm = vm;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _vm.CarregarMateriaisCommand.Execute(null);
    }
}
