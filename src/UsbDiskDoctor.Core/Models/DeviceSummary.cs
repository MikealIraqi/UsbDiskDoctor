using UsbDiskDoctor.Core.Enums;

namespace UsbDiskDoctor.Core.Models
{
    /// <summary>
    /// Represents a snapshot of a USB storage device and its volumes.
    /// </summary>
    public sealed record DeviceSummary
    {
        public string DeviceId { get; init; } = string.Empty;
        public string FriendlyName { get; init; } = string.Empty;
        public string? Model { get; init; }
        public string? SerialNumber { get; init; }
        public BusType BusType { get; init; } = BusType.Unknown;
        public MediaType MediaType { get; init; } = MediaType.Unknown;
        public PartitionStyle PartitionStyle { get; init; } = PartitionStyle.Unknown;
        public long SizeBytes { get; init; } = 0;
        public HealthStatus HealthStatus { get; init; } = HealthStatus.Unknown;
        public OperationalStatus OperationalStatus { get; init; } = OperationalStatus.Unknown;
        public bool IsUsb { get; init; } = false;
        public bool IsExternal { get; init; } = false;
        public IReadOnlyList<VolumeInfo> Volumes { get; init; } = Array.Empty<VolumeInfo>();
    }
}