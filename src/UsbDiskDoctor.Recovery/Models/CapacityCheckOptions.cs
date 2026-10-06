namespace UsbDiskDoctor.Recovery.Models;

/// <summary>
/// Configuration options for fake capacity checking operations.
/// </summary>
public sealed record CapacityCheckOptions
{
    /// <summary>
    /// The checking strategy to use. Default: Smart.
    /// </summary>
    public CapacityCheckMode Mode { get; init; } = CapacityCheckMode.Smart;

    /// <summary>
    /// Size of each test block in bytes. Default: 1 MB (1,048,576 bytes).
    /// </summary>
    public int BlockSizeBytes { get; init; } = 1_048_576;

    /// <summary>
    /// Number of sample points to check in Quick mode. Default: 100.
    /// </summary>
    public int SampleCount { get; init; } = 100;

    /// <summary>
    /// Full path to the temporary test file on the target volume.
    /// </summary>
    public string TestFilePath { get; init; } = string.Empty;

    /// <summary>
    /// Whether to allow writing on non-empty volumes. Default: false (safety protection).
    /// </summary>
    public bool AllowWriteOnNonEmptyVolume { get; init; } = false;
}