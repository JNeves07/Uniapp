using UniApp.Mobile.ViewModels;

namespace UniApp.Mobile.Views;

public partial class AnunciarPage : ContentPage
{
    public AnunciarPage(AnunciarViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }
}
