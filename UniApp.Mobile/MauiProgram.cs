using UniApp.Mobile.Services;
using UniApp.Mobile.ViewModels;
using UniApp.Mobile.Views;

namespace UniApp.Mobile;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder.UseMauiApp<App>();

        builder.Services.AddSingleton<ApiService>();
        builder.Services.AddTransient<LoginViewModel>();
        builder.Services.AddTransient<HomeViewModel>();
        builder.Services.AddTransient<AnunciarViewModel>();
        builder.Services.AddTransient<GruposViewModel>();
        builder.Services.AddTransient<PerfilViewModel>();

        builder.Services.AddTransient<LoginPage>();
        builder.Services.AddTransient<HomePage>();
        builder.Services.AddTransient<AnunciarPage>();
        builder.Services.AddTransient<GruposPage>();
        builder.Services.AddTransient<PerfilPage>();

        return builder.Build();
    }
}
