using Microsoft.Extensions.DependencyInjection;

namespace RaizBarApp
{
    public partial class App : Application
    {
        public App()
        {
            try
            {
                InitializeComponent();
            }
            catch (Exception exception)
            {
                StartupDiagnostics.LogException("App.InitializeComponent", exception);
                throw;
            }
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            try
            {
                return new Window(new AppShell());
            }
            catch (Exception exception)
            {
                StartupDiagnostics.LogException("App.CreateWindow", exception);
                throw;
            }
        }
    }
}