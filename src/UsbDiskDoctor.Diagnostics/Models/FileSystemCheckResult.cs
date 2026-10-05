using System;
using System.Collections.Generic;
using UsbDiskDoctor.Core.Enums;

namespace UsbDiskDoctor.Diagnostics.Models
{
    /// <summary>
    /// Represents the result of a read-only file system inspection on a volume.
    /// </summary>
    public sealed record FileSystemCheckResult
    {
        public string DriveLetter { get; init; } = string.Empty;
        public FileSystemType FileSystem { get; init; } = FileSystemType.Unknown;
        public string Label { get; init; } = string.Empty;
        public long CapacityBytes { get; init; } = 0;
        public long FreeSpaceBytes { get; init; } = 0;
        public bool IsMounted { get; init; } = false;
        public bool IsRaw { get; init; } = false;
        public bool DirtyBitSet { get; init; } = false;
        public bool CheckSucceeded { get; init; } = false;
        public string? ErrorMessage { get; init; } = null;
        public IReadOnlyList<string> Notes { get; init; } = Array.Empty<string>();
    }
}