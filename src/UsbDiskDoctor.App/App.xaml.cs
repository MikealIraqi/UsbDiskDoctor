using System.Windows;
using UsbDiskDoctor.App.Localization;
using UsbDiskDoctor.App.ViewModels;
using UsbDiskDoctor.Core.Logging;
using UsbDiskDoctor.Diagnostics.DeviceDiscovery;
using UsbDiskDoctor.Diagnostics.DiagnosticEngine;
using UsbDiskDoctor.Diagnostics.HealthChecks;
using UsbDiskDoctor.Diagnostics.VolumeReading;
using UsbDiskDoctor.Knowledge.Services;
using UsbDiskDoctor.Recovery.CapacityChecking;
using UsbDiskDoctor.Repair.Execution;
using UsbDiskDoctor.Repair.Planning;
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

            // Localization: load saved language (Arabic default) before UI creation.
            LocalizationService.Initialize();

            // Discovery
            var volumeReader = new WmiVolumeReader();
            var discoveryService = new WmiDeviceDiscoveryService(volumeReader);

            // Diagnostics
            var smartReader = new WmiSmartReader();
            var fileSystemChecker = new WmiFileSystemChecker();
            var healthEvaluator = new HealthEvaluator();
            var diagnosticEngine = new DiagnosticEngine(smartReader, fileSystemChecker, healthEvaluator);

            // Knowledge
            var knowledgeService = new JsonKnowledgeService();
            knowledgeService.Load();

            // Repair
            var repairPlanner = new RepairPlanner(knowledgeService);
            var commandRunner = new ProcessCommandRunner();
            var repairExecutor = new SafeRepairExecutor(commandRunner);

            // Reporting
            var htmlReportGenerator = new HtmlReportGenerator();

            // Recovery
            var fakeCapacityChecker = new FakeCapacityChecker();

            var mainViewModel = new MainViewModel(
                discoveryService,
                diagnosticEngine,
                htmlReportGenerator,
                repairPlanner,
                repairExecutor,
                fakeCapacityChecker);
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