using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UsbDiskDoctor.Recovery.Models;

namespace UsbDiskDoctor.Recovery.Scanning
{
    /// <summary>
    /// Scans mounted volumes for recoverable files using read-only file system access.
    /// </summary>
    public interface IVolumeScanner
    {
        /// <summary>
        /// Scans a mounted volume (e.g., "E:\") for files that can be read.
        /// READ-ONLY. Never writes, deletes, or modifies anything.
        /// </summary>
        /// <param name="volumeRoot">
        ///   Root path of the mounted volume (e.g., "E:\"). Must end with "\".
        /// </param>
        /// <param name="options">Scan options.</param>
        /// <param name="progress">Optional progress callback.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Read-only list of files with metadata. Never null.</returns>
        Task<IReadOnlyList<RecoveredFileInfo>> ScanAsync(
            string volumeRoot,
            RecoveryOptions options,
            IProgress<RecoveryProgressInfo>? progress,
            CancellationToken cancellationToken = default);
    }
}