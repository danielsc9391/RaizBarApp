using RaizBarApp.Models;
using RaizBarApp.PageModels;

namespace RaizBarApp.Pages
{
    public partial class PedidoPage : ContentPage
    {
        public PedidoPage(PedidoViewModel model)
        {
            InitializeComponent();
            BindingContext = model;
        }
    }
}