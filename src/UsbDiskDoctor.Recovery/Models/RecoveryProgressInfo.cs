namespace UsbDiskDoctor.Recovery.Models
{
    /// <summary>
    /// Represents progress information for recovery operations.
    /// </summary>
    public sealed record RecoveryProgressInfo
    {
        /// <summary>
        /// Number of bytes scanned so far.
        /// </summary>
        public long BytesScanned { get; init; } = 0;

        /// <summary>
        /// Total bytes to scan (0 if unknown).
        /// </summary>
        public long TotalBytes { get; init; } = 0;

        /// <summary>
        /// Number of recoverable files found so far.
        /// </summary>
        public int FilesFound { get; init; } = 0;

        /// <summary>
        /// Percentage complete (0-100). Pre-computed for display.
        /// </summary>
        public int PercentComplete { get; init; } = 0;
    }
}