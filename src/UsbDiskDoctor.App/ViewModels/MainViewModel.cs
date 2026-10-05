using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using UsbDiskDoctor.Core.Models;
using UsbDiskDoctor.Diagnostics.DeviceDiscovery;
using UsbDiskDoctor.Diagnostics.DiagnosticEngine;
using UsbDiskDoctor.Diagnostics.Models;
using UsbDiskDoctor.Reporting;

namespace UsbDiskDoctor.App.ViewModels
{
    /// <summary>
    /// Main view model for the UsbDiskDoctor application, managing device discovery,
    /// diagnostics, and report generation.
    /// </summary>
    public sealed partial class MainViewModel : ObservableObject
    {
        private readonly IDeviceDiscoveryService _discoveryService;
        private readonly IDiagnosticEngine _diagnosticEngine;
        private readonly HtmlReportGenerator _htmlReportGenerator;
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

        public DeviceDetailsViewModel DetailsViewModel => _detailsViewModel;

        public MainViewModel(
            IDeviceDiscoveryService discoveryService,
            IDiagnosticEngine diagnosticEngine,
            HtmlReportGenerator htmlReportGenerator)
        {
            _discoveryService = discoveryService ?? throw new ArgumentNullException(nameof(discoveryService));
            _diagnosticEngine = diagnosticEngine ?? throw new ArgumentNullException(nameof(diagnosticEngine));
            _htmlReportGenerator = htmlReportGenerator ?? throw new ArgumentNullException(nameof(htmlReportGenerator));
            Devices = new ObservableCollection<DeviceSummary>();
            _detailsViewModel = new DeviceDetailsViewModel(null);
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
                    : $"تم العثور على {devices.Count} جهاز.";
            }
            catch (Exception ex)
            {
                StatusMessage = $"خطأ أثناء البحث: {ex.Message}";
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
                var htmlOutput = _htmlReportGenerator.Generate(new[] { diagnosticReport });
                ReportViewModel.SetContent(htmlOutput);
                StatusMessage = $"اكتمل الفحص - الحالة: {diagnosticReport.HealthEvaluation.OverallStatus}";
            }
            catch (Exception diagnosticException)
            {
                StatusMessage = $"خطأ أثناء الفحص: {diagnosticException.Message}";
            }
            finally
            {
                IsBusy = false;
            }
        }

        private bool CanRunFullDiagnostic() => SelectedDevice != null && !IsBusy;
    }
}