namespace UsbDiskDoctor.Recovery.Models;

/// <summary>
/// Defines the strategy for checking fake capacity on storage devices.
/// </summary>
public enum CapacityCheckMode
{
    /// <summary>
    /// Quick check: 100 sample points distributed across the full capacity. Fast (~minutes).
    /// </summary>
    Quick = 0,

    /// <summary>
    /// Smart check: Binary search from the end of claimed capacity. Medium speed.
    /// </summary>
    Smart = 1,

    /// <summary>
    /// Full check: Write/read every block. Accurate but slow (hours).
    /// </summary>
    Full = 2
}