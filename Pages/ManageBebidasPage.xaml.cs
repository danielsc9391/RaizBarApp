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
    }
}