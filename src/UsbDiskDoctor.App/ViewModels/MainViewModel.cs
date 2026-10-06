using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
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
            StatusMessage = "جاري البحث عن أجهزة USB...";
            try
            {
                var options = new DiscoveryOptions();
                var devices = await _discoveryService.DiscoverAsync(options);

                Devices.Clear();
                foreach (var d in devices)
                {
                    Devices.Add(d);
                }

                StatusMessage = devices.Count == 0
                    ? "لا توجد أجهزة USB متصلة."
                    : "تم العثور على " + devices.Count + " جهاز.";
            }
            catch (Exception ex)
            {
                StatusMessage = "خطأ أثناء البحث: " + ex.Message;
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
            StatusMessage = "جاري الفحص الصحي الشامل...";
            try
            {
                var diagnosticReport = await _diagnosticEngine.DiagnoseAsync(deviceForDiagnostic);

                var proposals = _repairPlanner.Plan(diagnosticReport);
                DetailsViewModel.SetProposals(proposals);

                var htmlOutput = _htmlReportGenerator.Generate(new[] { diagnosticReport });
                ReportViewModel.SetContent(htmlOutput);

                StatusMessage = "اكتمل الفحص - الحالة: " + diagnosticReport.HealthEvaluation.OverallStatus + " - " + proposals.Count + " اقتراح إصلاح.";
            }
            catch (Exception diagnosticException)
            {
                StatusMessage = "خطأ أثناء الفحص: " + diagnosticException.Message;
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
            StatusMessage = "جاري تنفيذ: " + proposal.ActionCode + "...";
            try
            {
                var execResult = await _repairExecutor.ExecuteAsync(
                    proposal,
                    confirmationToken,
                    targetDriveLetter);

                StatusMessage = execResult.Succeeded
                    ? "نجح التنفيذ: " + execResult.MessageAr
                    : "تم الرفض أو الفشل: " + execResult.MessageAr;
            }
            catch (Exception execException)
            {
                StatusMessage = "خطأ أثناء التنفيذ: " + execException.Message;
            }
            finally
            {
                IsBusy = false;
            }
        }
    }
}
