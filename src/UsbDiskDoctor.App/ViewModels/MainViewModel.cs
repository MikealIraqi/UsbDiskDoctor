using System;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using UsbDiskDoctor.Core.Models;
using UsbDiskDoctor.Diagnostics.DeviceDiscovery;
using UsbDiskDoctor.Diagnostics.Models;

namespace UsbDiskDoctor.App.ViewModels
{
    /// <summary>
    /// Main view model for the UsbDiskDoctor application, managing device discovery and UI state.
    /// </summary>
    public sealed class MainViewModel : ViewModelBase
    {
        private readonly IDeviceDiscoveryService _discoveryService;
        private DeviceSummary? _selectedDevice;
        private bool _isBusy;
        private string _statusMessage = string.Empty;

        public MainViewModel(IDeviceDiscoveryService discoveryService)
        {
            _discoveryService = discoveryService ?? throw new ArgumentNullException(nameof(discoveryService));
            Devices = new ObservableCollection<DeviceSummary>();
            RefreshCommand = new RelayCommand(OnRefreshExecuted, _ => !IsBusy);
        }

        public ObservableCollection<DeviceSummary> Devices { get; }

        public DeviceSummary? SelectedDevice
        {
            get => _selectedDevice;
            set => SetProperty(ref _selectedDevice, value);
        }

        public bool IsBusy
        {
            get => _isBusy;
            set
            {
                if (SetProperty(ref _isBusy, value))
                {
                    ((RelayCommand)RefreshCommand).RaiseCanExecuteChanged();
                }
            }
        }

        public string StatusMessage
        {
            get => _statusMessage;
            set => SetProperty(ref _statusMessage, value);
        }

        public ICommand RefreshCommand { get; }

        private async void OnRefreshExecuted(object? parameter)
        {
            await RefreshAsync();
        }

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
    }
}