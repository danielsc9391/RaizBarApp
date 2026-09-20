namespace RaizBarApp
{
    public partial class App : Application
    {
        private static int _errorAlertShowing;

        public App()
        {
            AppDomain.CurrentDomain.UnhandledException += (_, args) => HandleUnhandledException("AppDomain", args.ExceptionObject as Exception);
            TaskScheduler.UnobservedTaskException += (_, args) =>
            {
                args.SetObserved();
                HandleUnhandledException("TaskScheduler", args.Exception);
            };
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

        private static void HandleUnhandledException(string source, Exception? exception)
        {
            exception ??= new Exception("Exceção não identificada.");
            StartupDiagnostics.LogException(source, exception);
            if (Interlocked.Exchange(ref _errorAlertShowing, 1) != 0) return;

            MainThread.BeginInvokeOnMainThread(async () =>
            {
                try
                {
                    if (Shell.Current is not null)
                        await Shell.Current.DisplayAlertAsync("Erro", "Ocorreu um erro inesperado", "OK");
                }
                catch (Exception alertException)
                {
                    StartupDiagnostics.LogException("Error alert", alertException);
                }
                finally
                {
                    Interlocked.Exchange(ref _errorAlertShowing, 0);
                }
            });
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