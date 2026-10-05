using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using UsbDiskDoctor.Core.Models;

namespace UsbDiskDoctor.App.ViewModels
{
    /// <summary>
    /// View model for displaying detailed information about a selected USB device.
    /// </summary>
    public sealed partial class DeviceDetailsViewModel : ObservableObject
    {
        [ObservableProperty]
        private DeviceSummary? _device;

        public DeviceDetailsViewModel(DeviceSummary? device)
        {
            _device = device;
        }

        public bool HasDevice => Device != null;

        public string FriendlyName => Device?.FriendlyName ?? "لا يوجد جهاز مختار";

        public string DeviceIdText => Device?.DeviceId ?? "-";

        public string ModelText => Device?.Model ?? "-";

        public string SerialNumberText => Device?.SerialNumber ?? "-";

        public string BusTypeText => Device?.BusType.ToString() ?? "-";

        public string MediaTypeText => Device?.MediaType.ToString() ?? "-";

        public string PartitionStyleText => Device?.PartitionStyle.ToString() ?? "-";

        public string SizeText => Device == null ? "-" : FormatBytes(Device.SizeBytes);

        public string OperationalStatusText => Device?.OperationalStatus.ToString() ?? "-";

        public string HealthStatusText => Device?.HealthStatus.ToString() ?? "-";

        public string VolumesCountText => Device == null ? "0" : Device.Volumes.Count.ToString();

        public IReadOnlyList<VolumeInfo> Volumes => Device?.Volumes ?? Array.Empty<VolumeInfo>();

        public ObservableCollection<RepairProposal> Proposals { get; } =
            new ObservableCollection<RepairProposal>();

        public int ProposalsCount => Proposals.Count;

        public bool HasProposals => Proposals.Count > 0;

        public void SetProposals(IReadOnlyList<RepairProposal> proposals)
        {
            Proposals.Clear();
            if (proposals != null)
            {
                foreach (var proposal in proposals)
                {
                    Proposals.Add(proposal);
                }
            }
            OnPropertyChanged(nameof(ProposalsCount));
            OnPropertyChanged(nameof(HasProposals));
        }

        public void ClearProposals()
        {
            Proposals.Clear();
            OnPropertyChanged(nameof(ProposalsCount));
            OnPropertyChanged(nameof(HasProposals));
        }

        partial void OnDeviceChanged(DeviceSummary? value)
        {
            OnPropertyChanged(nameof(HasDevice));
            OnPropertyChanged(nameof(FriendlyName));
            OnPropertyChanged(nameof(DeviceIdText));
            OnPropertyChanged(nameof(ModelText));
            OnPropertyChanged(nameof(SerialNumberText));
            OnPropertyChanged(nameof(BusTypeText));
            OnPropertyChanged(nameof(MediaTypeText));
            OnPropertyChanged(nameof(PartitionStyleText));
            OnPropertyChanged(nameof(SizeText));
            OnPropertyChanged(nameof(OperationalStatusText));
            OnPropertyChanged(nameof(HealthStatusText));
            OnPropertyChanged(nameof(VolumesCountText));
            OnPropertyChanged(nameof(Volumes));

            // Clear proposals when device changes — they're stale now.
            ClearProposals();
        }

        private static string FormatBytes(long bytes)
        {
            if (bytes < 1024)
                return $"{bytes} B";

            if (bytes < 1024L * 1024)
                return $"{bytes / 1024.0:F2} KB";

            if (bytes < 1024L * 1024 * 1024)
                return $"{bytes / (1024.0 * 1024):F2} MB";

            if (bytes < 1024L * 1024 * 1024 * 1024)
                return $"{bytes / (1024.0 * 1024 * 1024):F2} GB";

            return $"{bytes / (1024.0 * 1024 * 1024 * 1024):F2} TB";
        }
    }
}