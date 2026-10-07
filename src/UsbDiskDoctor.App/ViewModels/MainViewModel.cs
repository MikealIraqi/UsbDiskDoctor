using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using UsbDiskDoctor.App.Localization;
using UsbDiskDoctor.Core.Models;
using UsbDiskDoctor.Diagnostics.DeviceDiscovery;
using UsbDiskDoctor.Diagnostics.DiagnosticEngine;
using UsbDiskDoctor.Diagnostics.Models;
using UsbDiskDoctor.Recovery.CapacityChecking;
using UsbDiskDoctor.Repair.Execution;
using UsbDiskDoctor.Repair.Planning;
using UsbDiskDoctor.Reporting;

namespace UsbDiskDoctor.App.ViewModels
{
    /// <summary>
    /// Main view model for the UsbDiskDoctor application, managing device discovery,
    /// diagnostics, repair planning, and safe execution.
    /// </summary>
    public sealed partial class MainViewModel : ObservableObject
    {
        private readonly IDeviceDiscoveryService _discoveryService;
        private readonly IDiagnosticEngine _diagnosticEngine;
        private readonly HtmlReportGenerator _htmlReportGenerator;
        private readonly IRepairPlanner _repairPlanner;
        private readonly IRepairExecutor _repairExecutor;
        private readonly IFakeCapacityChecker _capacityChecker;
        private readonly DeviceDetailsViewModel _detailsViewModel;
        private string? _lastStatusKey;
        private object[] _lastStatusArgs = System.Array.Empty<object>();

        [ObservableProperty]
        private DeviceSummary? _selectedDevice;

        [ObservableProperty]
        private bool _isBusy;

        [ObservableProperty]
        private string _statusMessage = string.Empty;

        [ObservableProperty]
        private ReportViewModel _reportViewModel;

        public ObservableCollection<DeviceSummary> Devices { get; }

        /// <summary>True when no devices have been discovered yet.</summary>
        public bool HasNoDevices => Devices.Count == 0;

        public DeviceDetailsViewModel DetailsViewModel => _detailsViewModel;

        /// <summary>
        /// Raised when the user requests execution of a repair proposal.
        /// Handled by the View (MainWindow) to open the confirmation dialog.
        /// </summary>
        public event Action<RepairProposal>? ProposalExecuteRequested;

        public MainViewModel(
            IDeviceDiscoveryService discoveryService,
            IDiagnosticEngine diagnosticEngine,
            HtmlReportGenerator htmlReportGenerator,
            IRepairPlanner repairPlanner,
            IRepairExecutor repairExecutor,
            IFakeCapacityChecker capacityChecker)
        {
            _discoveryService = discoveryService ?? throw new ArgumentNullException(nameof(discoveryService));
            _diagnosticEngine = diagnosticEngine ?? throw new ArgumentNullException(nameof(diagnosticEngine));
            _htmlReportGenerator = htmlReportGenerator ?? throw new ArgumentNullException(nameof(htmlReportGenerator));
            _repairPlanner = repairPlanner ?? throw new ArgumentNullException(nameof(repairPlanner));
            _repairExecutor = repairExecutor ?? throw new ArgumentNullException(nameof(repairExecutor));
            _capacityChecker = capacityChecker ?? throw new ArgumentNullException(nameof(capacityChecker));

            Devices = new ObservableCollection<DeviceSummary>();
            Devices.CollectionChanged += (_, _) => OnPropertyChanged(nameof(HasNoDevices));
            _detailsViewModel = new DeviceDetailsViewModel(null, _capacityChecker);
            _reportViewModel = new ReportViewModel();

            LocalizationService.LanguageChanged += ReapplyLastStatus;
        }

        partial void OnSelectedDeviceChanged(DeviceSummary? value)
        {
            _detailsViewModel.Device = value;
            RunFullDiagnosticCommand.NotifyCanExecuteChanged();
        }

        partial void OnIsBusyChanged(bool value)
        {
            RefreshCommand.NotifyCanExecuteChanged();
            RunFullDiagnosticCommand.NotifyCanExecuteChanged();
            RequestExecuteProposalCommand.NotifyCanExecuteChanged();
        }

        [RelayCommand(CanExecute = nameof(CanRefresh))]
        private async Task RefreshAsync()
        {
            if (IsBusy) return;
            IsBusy = true;
            SetStatus("Status.Searching");
            try
            {
                var options = new DiscoveryOptions();
                var devices = await _discoveryService.DiscoverAsync(options);

                Devices.Clear();
                foreach (var d in devices)
                {
                    Devices.Add(d);
                }

                if (devices.Count == 0) { SetStatus("Status.NoDevices"); }
                else { SetStatus("Status.FoundDevicesFormat", devices.Count); }
            }
            catch (Exception ex)
            {
                SetStatus("Status.SearchError", ex.Message);
            }
            finally
            {
                IsBusy = false;
            }
        }

        private bool CanRefresh() => !IsBusy;

        [RelayCommand(CanExecute = nameof(CanRunFullDiagnostic))]
        private async Task RunFullDiagnosticAsync()
        {
            var deviceForDiagnostic = SelectedDevice;
            if (deviceForDiagnostic == null)
            {
                return;
            }

            IsBusy = true;
            SetStatus("Status.DiagnosingFull");
            try
            {
                var diagnosticReport = await _diagnosticEngine.DiagnoseAsync(deviceForDiagnostic);

                var proposals = _repairPlanner.Plan(diagnosticReport);
                DetailsViewModel.SetProposals(proposals);

                var htmlOutput = _htmlReportGenerator.Generate(new[] { diagnosticReport });
                ReportViewModel.SetContent(htmlOutput);

                SetStatus("Status.DiagnosisCompleteFormat", diagnosticReport.HealthEvaluation.OverallStatus, proposals.Count);
            }
            catch (Exception diagnosticException)
            {
                SetStatus("Status.DiagnosisError", diagnosticException.Message);
            }
            finally
            {
                IsBusy = false;
            }
        }

        private bool CanRunFullDiagnostic() => SelectedDevice != null && !IsBusy;

        [RelayCommand(CanExecute = nameof(CanRequestExecute))]
        private void RequestExecuteProposal(RepairProposal? proposal)
        {
            if (proposal == null) return;
            ProposalExecuteRequested?.Invoke(proposal);
        }

        private bool CanRequestExecute(RepairProposal? proposal)
        {
            return !IsBusy && proposal != null;
        }

        /// <summary>
        /// Executes a repair proposal. Called by the View AFTER the user confirms
        /// in the confirmation dialog.
        /// </summary>
        public async Task ExecuteProposalAsync(
            RepairProposal proposal,
            string? confirmationToken,
            string? targetDriveLetter)
        {
            if (proposal == null) return;

            IsBusy = true;
            SetStatus("Status.ExecutingFormat", proposal.ActionCode);
            try
            {
                var execResult = await _repairExecutor.ExecuteAsync(
                    proposal,
                    confirmationToken,
                    targetDriveLetter);

                SetStatus(execResult.Succeeded
                    ? "Status.ExecuteSucceeded"
                    : "Status.ExecuteFailed", execResult.MessageAr);
            }
            catch (Exception execException)
            {
                SetStatus("Status.ExecuteError", execException.Message);
            }
            finally
            {
                IsBusy = false;
            }
        }
    

        private void SetStatus(string key, params object[] args)
        {
            _lastStatusKey = key;
            _lastStatusArgs = args;
            StatusMessage = args.Length == 0
                ? LocalizationService.Get(key)
                : string.Format(LocalizationService.Get(key), args);
        }

        private void ReapplyLastStatus()
        {
            if (_lastStatusKey is null) return;
            StatusMessage = _lastStatusArgs.Length == 0
                ? LocalizationService.Get(_lastStatusKey)
                : string.Format(LocalizationService.Get(_lastStatusKey), _lastStatusArgs);
        }
}
}
