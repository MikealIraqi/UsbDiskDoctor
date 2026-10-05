using System.Windows;
using UsbDiskDoctor.App.ViewModels;
using UsbDiskDoctor.Core.Logging;
using UsbDiskDoctor.Diagnostics.DeviceDiscovery;
using UsbDiskDoctor.Diagnostics.DiagnosticEngine;
using UsbDiskDoctor.Diagnostics.HealthChecks;
using UsbDiskDoctor.Diagnostics.VolumeReading;
using UsbDiskDoctor.Reporting;

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
            var volumeReader = new WmiVolumeReader();
            var discoveryService = new WmiDeviceDiscoveryService(volumeReader);

            var smartReader = new WmiSmartReader();
            var fileSystemChecker = new WmiFileSystemChecker();
            var healthEvaluator = new HealthEvaluator();
            var diagnosticEngine = new DiagnosticEngine(smartReader, fileSystemChecker, healthEvaluator);

            var htmlReportGenerator = new HtmlReportGenerator();

            var mainViewModel = new MainViewModel(discoveryService, diagnosticEngine, htmlReportGenerator);
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