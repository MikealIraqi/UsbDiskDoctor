using System.Windows;
using UsbDiskDoctor.App.ViewModels;
using UsbDiskDoctor.Core.Logging;
using UsbDiskDoctor.Diagnostics.DeviceDiscovery;

namespace UsbDiskDoctor.App
{
    /// <summary>
    /// Application entry point with manual dependency injection composition.
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            LoggingSetup.Initialize();

            // Manual DI composition root
            var discoveryService = new WmiDeviceDiscoveryService();
            var mainViewModel = new MainViewModel(discoveryService);
            var mainWindow = new MainWindow(mainViewModel);

            mainWindow.Show();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            LoggingSetup.Shutdown();
            base.OnExit(e);
        }
    }
}