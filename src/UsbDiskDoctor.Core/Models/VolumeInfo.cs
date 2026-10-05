using UsbDiskDoctor.Core.Enums;

namespace UsbDiskDoctor.Core.Models
{
    /// <summary>
    /// Represents a logical volume (partition with a file system) on a storage device.
    /// </summary>
    public sealed record VolumeInfo
    {
        public string? DriveLetter { get; init; }
        public string Label { get; init; } = string.Empty;
        public FileSystemType FileSystem { get; init; } = FileSystemType.Unknown;
        public long SizeBytes { get; init; } = 0;
        public long FreeSpaceBytes { get; init; } = 0;
        public HealthStatus HealthStatus { get; init; } = HealthStatus.Unknown;
        public bool IsRaw { get; init; } = false;
        public bool IsMounted { get; init; } = false;
    }
}