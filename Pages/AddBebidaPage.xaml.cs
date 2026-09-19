using RaizBarApp.Models;
using RaizBarApp.PageModels;
using System.Globalization;

namespace RaizBarApp.Pages
{
    public partial class AddBebidaPage : ContentPage
    {
        private readonly PedidoViewModel _pedidoViewModel;
        private readonly ManageBebidasPageModel _manageBebidasViewModel;
        private readonly Bebida _editingBebida;
        private readonly BebidaRepository _bebidaRepository;

        public AddBebidaPage(PedidoViewModel viewModel)
        {
            InitializeComponent();
            _pedidoViewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
            BindingContext = _pedidoViewModel;
        }

        public AddBebidaPage(ManageBebidasPageModel viewModel, Bebida editingBebida = null)
        {
            InitializeComponent();
            _manageBebidasViewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
            _editingBebida = editingBebida;
            _bebidaRepository = new BebidaRepository();
            BindingContext = _manageBebidasViewModel;

            if (editingBebida != null)
            {
                NomeEntry.Text = editingBebida.Nome;
                PrecoEntry.Text = editingBebida.Preco.ToString("F2");
            }
        }

        private async void OnAddBebidaClicked(object sender, EventArgs e)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(NomeEntry.Text))
                {
                    await DisplayAlert("Erro", "Por favor, introduza o nome da bebida.", "OK");
                    return;
                }

                if (!decimal.TryParse(PrecoEntry.Text, NumberStyles.Currency, CultureInfo.GetCultureInfo("pt-PT"), out decimal preco))
                {
                    await DisplayAlert("Erro", "Por favor, introduza um preço válido.", "OK");
                    return;
                }

                var novaBebida = new Bebida
                {
                    Nome = NomeEntry.Text.Trim(),
                    Preco = preco,
                    Quantidade = 0
                };

                if (_pedidoViewModel != null)
                {
                    _pedidoViewModel.Bebidas.Add(novaBebida);
                }
                else if (_manageBebidasViewModel != null)
                {
                    if (_editingBebida != null)
                    {
                        // Update existing beverage
                        _editingBebida.Nome = novaBebida.Nome;
                        _editingBebida.Preco = novaBebida.Preco;
                        await _bebidaRepository.UpdateBebidaAsync(_editingBebida);
                    }
                    else
                    {
                        // Add new beverage
                        _manageBebidasViewModel.Bebidas.Add(novaBebida);
                        await _bebidaRepository.AddBebidaAsync(novaBebida);
                    }
                }
                
                await DisplayAlert("Sucesso", "Bebida adicionada com sucesso!", "OK");
                
                // Close the modal page
                await Navigation.PopModalAsync();
            }
            catch (Exception ex)
            {
                await DisplayAlert("Erro", $"Ocorreu um erro ao adicionar a bebida: {ex.Message}", "OK");
            }
        }

        private void OnCancelClicked(object sender, EventArgs e)
        {
            // Close the modal page
            Navigation.PopModalAsync();
        }

        private void OnPrecoEntryTextChanged(object sender, TextChangedEventArgs e)
        {
            // Format the text as currency when user types
            if (decimal.TryParse(e.NewTextValue, out decimal value))
            {
                PrecoEntry.Text = value.ToString("C", CultureInfo.GetCultureInfo("pt-PT"));
            }
        }
    }
}