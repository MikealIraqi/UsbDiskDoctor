namespace UsbDiskDoctor.Recovery.Models;

/// <summary>
/// Represents the final result of a capacity check operation.
/// </summary>
public sealed record CapacityCheckResult
{
    /// <summary>
    /// Root path of the drive that was checked.
    /// </summary>
    public string DriveRoot { get; init; } = string.Empty;

    /// <summary>
    /// Claimed capacity of the device in bytes.
    /// </summary>
    public long ClaimedCapacityBytes { get; init; } = 0;

    /// <summary>
    /// Actual capacity of the device in bytes (null if undetermined).
    /// </summary>
    public long? ActualCapacityBytes { get; init; } = null;

    /// <summary>
    /// Verdict of the capacity check.
    /// </summary>
    public CapacityVerdict Verdict { get; init; } = CapacityVerdict.Unknown;

    /// <summary>
    /// Total number of blocks checked during the operation.
    /// </summary>
    public int TotalBlocksChecked { get; init; } = 0;

    /// <summary>
    /// Number of blocks that failed verification.
    /// </summary>
    public int FailedBlocksCount { get; init; } = 0;

    /// <summary>
    /// Total duration of the check operation.
    /// </summary>
    public TimeSpan Duration { get; init; } = TimeSpan.Zero;

    /// <summary>
    /// Diagnostic message describing the result (English, for later translation in UI).
    /// </summary>
    public string? DiagnosticMessage { get; init; } = null;
}