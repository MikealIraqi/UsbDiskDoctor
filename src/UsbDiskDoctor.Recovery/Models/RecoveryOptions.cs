using System;
using System.Collections.Generic;

namespace UsbDiskDoctor.Recovery.Models
{
    /// <summary>
    /// Configuration options for file recovery scanning operations.
    /// </summary>
    public sealed record RecoveryOptions
    {
        /// <summary>
        /// The scanning strategy to use (Quick or Deep).
        /// </summary>
        public RecoveryScanMode ScanMode { get; init; } = RecoveryScanMode.Quick;

        /// <summary>
        /// Maximum number of bytes to scan. 0 means scan the entire source.
        /// </summary>
        public long MaxBytesToScan { get; init; } = 0;

        /// <summary>
        /// File extensions to recover (e.g., [".jpg", ".png"]). 
        /// Empty list means recover all supported file types.
        /// </summary>
        public IReadOnlyList<string> FileExtensionsToRecover { get; init; } = Array.Empty<string>();
    }
}