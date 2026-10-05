namespace UsbDiskDoctor.Core.Enums
{
    /// <summary>
    /// Represents the lifecycle state of a file recovery session.
    /// </summary>
    public enum RecoverySessionStatus
    {
        Pending = 0,
        Scanning = 1,
        Recovering = 2,
        Completed = 3,
        Failed = 4,
        Cancelled = 5
    }
}