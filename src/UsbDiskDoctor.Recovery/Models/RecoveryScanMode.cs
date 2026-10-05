namespace UsbDiskDoctor.Recovery.Models
{
    /// <summary>
    /// Defines the scanning strategy for file recovery operations.
    /// </summary>
    public enum RecoveryScanMode
    {
        /// <summary>
        /// Quick scan: reads file system tables only (faster, less thorough).
        /// </summary>
        Quick = 0,

        /// <summary>
        /// Deep scan: performs file carving across all sectors (slower, more thorough).
        /// </summary>
        Deep = 1
    }
}