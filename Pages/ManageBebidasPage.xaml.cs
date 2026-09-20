using RaizBarApp.PageModels;
using Microsoft.Extensions.DependencyInjection;

namespace RaizBarApp.Pages
{
    public partial class ManageBebidasPage : ContentPage
    {
        private readonly ManageBebidasPageModel _viewModel;

        public ManageBebidasPage()
        {
            InitializeComponent();
            _viewModel = Application.Current!.Handler!.MauiContext!.Services.GetRequiredService<ManageBebidasPageModel>();
            BindingContext = _viewModel;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await _viewModel.MostrarMigracoesCategoriasAsync();
        }

        private void PrecoEntry_TextChanged(object? sender, TextChangedEventArgs e)
        {
            if (sender is not Entry entry || string.IsNullOrEmpty(e.NewTextValue)) return;

            var filtered = new string(e.NewTextValue
                .Replace('.', ',')
                .Where(character => char.IsDigit(character) || character == ',')
                .ToArray());
            var separatorIndex = filtered.IndexOf(',');
            if (separatorIndex >= 0)
            {
                filtered = filtered[..(separatorIndex + 1)] +
                    new string(filtered[(separatorIndex + 1)..].Replace(",", string.Empty).ToCharArray());
            }

            if (entry.Text != filtered)
            {
                entry.Text = filtered;
                entry.CursorPosition = filtered.Length;
            }
        }
    }
}