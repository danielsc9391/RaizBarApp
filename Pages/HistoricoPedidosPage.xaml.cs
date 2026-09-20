using Microsoft.Extensions.DependencyInjection;
using RaizBarApp.PageModels;
using System.Diagnostics;

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
            try
            {
                Debug.WriteLine("Iniciando carregamento do histórico de pedidos na página");
                await _viewModel.CarregarAsync();
                Debug.WriteLine("Carregamento do histórico concluído com sucesso");
                PedidosCollectionView.ItemsSource = _viewModel.PedidosHistoricos;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Erro ao carregar histórico na página: {ex.Message}");
                Debug.WriteLine($"Detalhes do erro: {ex}");
                // Still set the data context to prevent crashes
                PedidosCollectionView.ItemsSource = _viewModel.PedidosHistoricos;
            }
        }
    }
}