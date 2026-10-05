namespace UsbDiskDoctor.Core.Enums
{
    /// <summary>
    /// Specifies the physical media type of the storage device.
    /// </summary>
    public enum MediaType
    {
        Unknown = 0,
        HDD = 1,
        SSD = 2,
        SCM = 3,
        Flash = 4,
        Removable = 5
    }
}