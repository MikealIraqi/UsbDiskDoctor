namespace UsbDiskDoctor.Core.Enums
{
    /// <summary>
    /// Defines the partition table format used on the disk.
    /// </summary>
    public enum PartitionStyle
    {
        Unknown = 0,
        MBR = 1,
        GPT = 2,
        RAW = 3
    }
}