namespace UsbDiskDoctor.Core.Enums
{
    /// <summary>
    /// Indicates the operational state of the storage device as reported by the OS.
    /// </summary>
    public enum OperationalStatus
    {
        Unknown = 0,
        OK = 1,
        Degraded = 2,
        Stressed = 3,
        PredictingFailure = 4,
        Error = 5,
        Starting = 6,
        Stopping = 7,
        Stopped = 8,
        Offline = 9
    }
}