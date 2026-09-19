using RaizBarApp.Models;
using RaizBarApp.PageModels;
using Microsoft.Extensions.DependencyInjection;

namespace RaizBarApp.Pages
{
    public partial class PedidoPage : ContentPage
    {
        public PedidoPage() : this(GetViewModel())
        {
        }

        public PedidoPage(PedidoViewModel model)
        {
            InitializeComponent();
            BindingContext = model;
        }

        private static PedidoViewModel GetViewModel()
        {
            return Application.Current!.Handler!.MauiContext!.Services.GetRequiredService<PedidoViewModel>();
        }
    }
}