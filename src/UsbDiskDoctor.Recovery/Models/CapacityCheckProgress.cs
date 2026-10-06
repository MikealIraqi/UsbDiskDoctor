namespace UsbDiskDoctor.Recovery.Models;

/// <summary>
/// Represents a progress snapshot during capacity check operations.
/// </summary>
public sealed record CapacityCheckProgress
{
    /// <summary>
    /// Current phase of the operation.
    /// </summary>
    public CapacityCheckPhase Phase { get; init; } = CapacityCheckPhase.Idle;

    /// <summary>
    /// Percentage complete (0.0 to 100.0).
    /// </summary>
    public double PercentComplete { get; init; } = 0.0;

    /// <summary>
    /// Number of bytes processed so far.
    /// </summary>
    public long BytesProcessed { get; init; } = 0;

    /// <summary>
    /// Total bytes to process.
    /// </summary>
    public long BytesTotal { get; init; } = 0;

    /// <summary>
    /// Current processing speed in MB/sec.
    /// </summary>
    public double CurrentSpeedMbPerSec { get; init; } = 0.0;

    /// <summary>
    /// Time elapsed since operation started.
    /// </summary>
    public TimeSpan Elapsed { get; init; } = TimeSpan.Zero;

    /// <summary>
    /// Estimated time remaining.
    /// </summary>
    public TimeSpan EstimatedRemaining { get; init; } = TimeSpan.Zero;
}