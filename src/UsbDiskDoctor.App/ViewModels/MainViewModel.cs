using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using UsbDiskDoctor.Core.Models;
using UsbDiskDoctor.Diagnostics.DeviceDiscovery;
using UsbDiskDoctor.Diagnostics.Models;

namespace UsbDiskDoctor.App.ViewModels
{
    /// <summary>
    /// Main view model for the UsbDiskDoctor application, managing device discovery and UI state.
    /// </summary>
    public sealed partial class MainViewModel : ObservableObject
    {
        private readonly IDeviceDiscoveryService _discoveryService;

        [ObservableProperty]
        private DeviceSummary? _selectedDevice;

        [ObservableProperty]
        private bool _isBusy;

        [ObservableProperty]
        private string _statusMessage = string.Empty;

        [ObservableProperty]
        private DeviceDetailsViewModel _detailsViewModel;

        public ObservableCollection<DeviceSummary> Devices { get; }

        public MainViewModel(IDeviceDiscoveryService discoveryService)
        {
            _discoveryService = discoveryService ?? throw new ArgumentNullException(nameof(discoveryService));
            Devices = new ObservableCollection<DeviceSummary>();
            _detailsViewModel = new DeviceDetailsViewModel(null);
        }

        partial void OnSelectedDeviceChanged(DeviceSummary? value)
        {
            DetailsViewModel = new DeviceDetailsViewModel(value);
        }

        partial void OnIsBusyChanged(bool value)
        {
            RefreshCommand.NotifyCanExecuteChanged();
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
    }
}