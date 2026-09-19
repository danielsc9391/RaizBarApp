using RaizBarApp.Models;
using RaizBarApp.PageModels;

namespace RaizBarApp.Pages
{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();
        }
        
        public MainPage(PedidoViewModel model) : this()
        {
            BindingContext = model;
        }
    }
}