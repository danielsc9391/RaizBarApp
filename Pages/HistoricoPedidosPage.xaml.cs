using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RaizBarApp.PageModels;

namespace RaizBarApp.Pages
{
    public partial class HistoricoPedidosPage : ContentPage
    {
        public HistoricoPedidosPage()
        {
            InitializeComponent();
            BindingContext = new HistoricoPedidosPageModel();
        }
    }
}