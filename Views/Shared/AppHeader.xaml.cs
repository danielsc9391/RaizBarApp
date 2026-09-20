using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaizBarApp.Views.Shared
{
    public partial class AppHeader : ContentView
    {
        public static readonly BindableProperty ShowHamburgerProperty =
            BindableProperty.Create(nameof(ShowHamburger), typeof(bool), typeof(AppHeader), true);

        public bool ShowHamburger
        {
            get => (bool)GetValue(ShowHamburgerProperty);
            set => SetValue(ShowHamburgerProperty, value);
        }

        public AppHeader()
        {
            InitializeComponent();
        }

        private void OnMenuButtonClicked(object sender, EventArgs e)
        {
            if (Shell.Current is not null)
            {
                Shell.Current.FlyoutIsPresented = true;
            }
        }
    }
}