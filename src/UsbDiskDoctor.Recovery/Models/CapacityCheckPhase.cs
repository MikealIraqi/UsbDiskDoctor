namespace UsbDiskDoctor.Recovery.Models;

/// <summary>
/// Represents the current phase of a capacity check operation.
/// </summary>
public enum CapacityCheckPhase
{
    /// <summary>
    /// No operation in progress.
    /// </summary>
    Idle = 0,

    /// <summary>
    /// Preparing the test environment.
    /// </summary>
    Preparing = 1,

    /// <summary>
    /// Writing test patterns to the device.
    /// </summary>
    Writing = 2,

    /// <summary>
    /// Reading back test patterns from the device.
    /// </summary>
    Reading = 3,

    /// <summary>
    /// Verifying data integrity across tested blocks.
    /// </summary>
    Verifying = 4,

    /// <summary>
    /// Check completed successfully.
    /// </summary>
    Completed = 5,

    /// <summary>
    /// Check failed due to errors.
    /// </summary>
    Failed = 6,

    /// <summary>
    /// Check was cancelled by the user.
    /// </summary>
    Cancelled = 7
}