namespace UsbDiskDoctor.Recovery.Models;

/// <summary>
/// Represents the verdict of a capacity check operation.
/// </summary>
public enum CapacityVerdict
{
    /// <summary>
    /// Not yet checked or result unknown.
    /// </summary>
    Unknown = 0,

    /// <summary>
    /// Genuine: Claimed capacity matches actual capacity.
    /// </summary>
    Genuine = 1,

    /// <summary>
    /// Fake: Wraparound detected — actual capacity is less than claimed.
    /// </summary>
    Fake = 2,

    /// <summary>
    /// Inconclusive: I/O errors prevented full verification.
    /// </summary>
    Inconclusive = 3,

    /// <summary>
    /// Failed: Device did not respond or disconnected during check.
    /// </summary>
    Failed = 4
}