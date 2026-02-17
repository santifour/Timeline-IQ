using System.Diagnostics;
using System.IO;
using System.Windows;
using ProjectTimeEstimator.Services;
using ProjectTimeEstimator.ViewModels;
using ProjectTimeEstimator.Views;

namespace ProjectTimeEstimator;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    private readonly DatabaseService _databaseService;

    public App()
    {
        // Global exception handling
        DispatcherUnhandledException += App_DispatcherUnhandledException;
        AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

        _databaseService = new DatabaseService();
    }

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        try
        {
            // Initialize Database
            _databaseService.Initialize();

            // Create Main Window with ViewModel
            var mainViewModel = new MainViewModel(_databaseService);
            var mainWindow = new MainWindow
            {
                DataContext = mainViewModel
            };

            mainWindow.Show();
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "App OnStartup");
            MessageBox.Show($"Uygulama başlatılırken kritik bir hata oluştu:\n{ex.Message}\n\nDetaylar log dosyasına kaydedildi.", "Kritik Hata", MessageBoxButton.OK, MessageBoxImage.Error);
            Shutdown();
        }
    }

    private void App_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
    {
        Logger.LogError(e.Exception, "DispatcherUnhandledException");
        MessageBox.Show($"Beklenmedik bir hata oluştu:\n{e.Exception.Message}", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
        e.Handled = true;
    }

    private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
        if (e.ExceptionObject is Exception ex)
        {
            Logger.LogError(ex, "AppDomain UnhandledException");
            MessageBox.Show($"Kritik sistem hatası:\n{ex.Message}", "Kritik Hata", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}
