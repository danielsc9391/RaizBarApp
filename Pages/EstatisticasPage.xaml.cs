using Microsoft.Extensions.DependencyInjection;

namespace RaizBarApp.Pages;

public partial class EstatisticasPage : ContentPage
{
    private readonly EstatisticasPageModel _viewModel;

    public EstatisticasPage()
    {
        InitializeComponent();
        _viewModel = Application.Current!.Handler!.MauiContext!.Services.GetRequiredService<EstatisticasPageModel>();
        BindingContext = _viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.CarregarAsync();
    }
}
