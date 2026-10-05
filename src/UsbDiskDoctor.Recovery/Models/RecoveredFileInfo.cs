namespace UsbDiskDoctor.Recovery.Models
{
    /// <summary>
    /// Represents metadata for a file discovered during recovery scanning.
    /// </summary>
    public sealed record RecoveredFileInfo
    {
        /// <summary>
        /// Suggested file name (may be generated if original name is unavailable).
        /// </summary>
        public string FileName { get; init; } = string.Empty;

        /// <summary>
        /// File extension (e.g., ".jpg", ".docx").
        /// </summary>
        public string Extension { get; init; } = string.Empty;

        /// <summary>
        /// File size in bytes.
        /// </summary>
        public long SizeBytes { get; init; } = 0;

        /// <summary>
        /// Starting offset on the source disk where the file data begins.
        /// Used for raw-device recovery (Phase 10). Zero for mounted volume scans.
        /// </summary>
        public long SourceOffset { get; init; } = 0;

        /// <summary>
        /// Absolute path of the source file on the mounted volume.
        /// Empty for raw-device recovery (Phase 10).
        /// </summary>
        public string SourceFullPath { get; init; } = string.Empty;

        /// <summary>
        /// Full path where the file will be restored (set during restore operation).
        /// </summary>
        public string TargetFullPath { get; init; } = string.Empty;
    }
}