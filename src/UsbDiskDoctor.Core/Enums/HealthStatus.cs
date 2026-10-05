namespace UsbDiskDoctor.Core.Enums
{
    /// <summary>
    /// Represents the overall health status of a USB disk, ordered by priority.
    /// </summary>
    public enum HealthStatus
    {
        Unknown = 0,
        Healthy = 1,
        Warning = 2,
        Critical = 3
    }
}