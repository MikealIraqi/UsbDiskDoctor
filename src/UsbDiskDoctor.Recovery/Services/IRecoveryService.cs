using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UsbDiskDoctor.Core.Models;
using UsbDiskDoctor.Recovery.Models;

namespace UsbDiskDoctor.Recovery.Services
{
    /// <summary>
    /// Provides file recovery capabilities for storage volumes.
    /// </summary>
    public interface IRecoveryService
    {
        /// <summary>
        /// Scans a source volume for recoverable files (read-only).
        /// Never writes to the source. Reports progress via callback.
        /// </summary>
        /// <param name="sourceVolume">
        ///   Volume path (e.g., "E:") to scan. Read-only.
        /// </param>
        /// <param name="options">Scan options. Must not be null.</param>
        /// <param name="progress">Optional progress callback. Called periodically.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>
        ///   Read-only list of recovered files (metadata only — nothing written yet).
        ///   Never null.
        /// </returns>
        Task<IReadOnlyList<RecoveredFileInfo>> ScanAsync(
            string sourceVolume,
            RecoveryOptions options,
            IProgress<RecoveryProgressInfo>? progress = null,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Writes selected recovered files to the target folder.
        /// Enforces: target must NOT be on the source volume.
        /// </summary>
        /// <param name="sourceVolume">Original source (for validation only).</param>
        /// <param name="files">Files to recover (from ScanAsync).</param>
        /// <param name="targetFolder">Destination folder (must be on a different volume).</param>
        /// <param name="progress">Optional progress callback.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>
        ///   A RecoverySession record summarizing the operation.
        ///   Never null.
        /// </returns>
        Task<RecoverySession> RestoreAsync(
            string sourceVolume,
            IReadOnlyList<RecoveredFileInfo> files,
            string targetFolder,
            IProgress<RecoveryProgressInfo>? progress = null,
            CancellationToken cancellationToken = default);
    }
}