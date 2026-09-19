using Microsoft.Extensions.DependencyInjection;
using RaizBarApp.PageModels;

namespace RaizBarApp.Pages
{
    public partial class HistoricoPedidosPage : ContentPage
    {
        private readonly HistoricoPedidosPageModel _viewModel;

        public HistoricoPedidosPage()
        {
            InitializeComponent();
            _viewModel = Application.Current!.Handler!.MauiContext!.Services.GetRequiredService<HistoricoPedidosPageModel>();
            BindingContext = _viewModel;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await _viewModel.CarregarAsync();
            PedidosCollectionView.ItemsSource = _viewModel.PedidosHistoricos;
        }
    }
}