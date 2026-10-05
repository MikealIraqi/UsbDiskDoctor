namespace UsbDiskDoctor.Core.Enums
{
    /// <summary>
    /// Specifies the physical bus type through which the disk is connected.
    /// </summary>
    public enum BusType
    {
        Unknown = 0,
        USB = 1,
        SATA = 2,
        NVMe = 3,
        eSATA = 4,
        SD = 5,
        MMC = 6,
        iSCSI = 7,
        RAID = 8,
        Virtual = 9,
        Other = 10
    }
}