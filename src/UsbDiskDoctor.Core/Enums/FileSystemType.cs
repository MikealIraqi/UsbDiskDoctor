namespace UsbDiskDoctor.Core.Enums
{
    /// <summary>
    /// Identifies the file system format of a volume or partition.
    /// </summary>
    public enum FileSystemType
    {
        Unknown = 0,
        NTFS = 1,
        FAT16 = 2,
        FAT32 = 3,
        exFAT = 4,
        ReFS = 5,
        RAW = 6,
        CDFS = 7,
        UDF = 8,
        APFS = 9,
        EXT4 = 10
    }
}