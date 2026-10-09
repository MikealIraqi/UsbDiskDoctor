using System;
using System.IO;

namespace UsbDiskDoctor.Recovery.FakeCapacity;

public sealed record DriveInfoEx
{
    public required char DriveLetter { get; init; }
    public required string VolumeLabel { get; init; }
    public required long TotalSizeBytes { get; init; }
    public required long FreeSpaceBytes { get; init; }
    public required DriveType DriveType { get; init; }
    public required string DriveFormat { get; init; }
    public required string BusType { get; init; }
}

public sealed record FakeCapacityRequest
{
    public required char DriveLetter { get; init; }
    public long TestSizeBytes { get; init; } = 0;
    public int FileSizeBytes { get; init; } = 64 * 1024 * 1024;
    public bool QuickTest { get; init; } = true;
}

public sealed record FakeCapacityProgress
{
    public required FakeCapacityPhase Phase { get; init; }
    public required long BytesProcessed { get; init; }
    public required long TotalBytes { get; init; }
    public required int CurrentFileIndex { get; init; }
    public required int TotalFiles { get; init; }
    public required TimeSpan Elapsed { get; init; }
}

public sealed record FakeCapacityResult
{
    public required FakeCapacityStatus Status { get; init; }
    public required long TotalBytesWritten { get; init; }
    public required long TotalBytesRead { get; init; }
    public required long FailedBytes { get; init; }
    public required double FailurePercentage { get; init; }
    public required long DetectedRealCapacityBytes { get; init; }
    public required string Details { get; init; }
    public required TimeSpan Duration { get; init; }
}

public sealed record SafetyCheckResult
{
    public required bool IsSafe { get; init; }
    public required string Reason { get; init; }
}