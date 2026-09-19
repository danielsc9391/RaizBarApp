using RaizBarApp.PageModels;

namespace RaizBarApp.Pages
{
    public partial class ManageBebidasPage : ContentPage
    {
        private readonly ManageBebidasPageModel _viewModel;

        public ManageBebidasPage()
        {
            InitializeComponent();
            _viewModel = new ManageBebidasPageModel();
            BindingContext = _viewModel;
        }
    }
}