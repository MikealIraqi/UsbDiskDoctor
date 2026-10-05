using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UsbDiskDoctor.Core.Models;
using UsbDiskDoctor.Recovery.Models;

namespace UsbDiskDoctor.Recovery.Restoring
{
    /// <summary>
    /// Restores (copies) recoverable files to a safe target folder.
    /// Enforces: target must NOT be on the source volume.
    /// </summary>
    public interface IFileRestorer
    {
        /// <summary>
        /// Copies the given files from their source paths to the target folder.
        /// Strictly enforces that the target folder is on a DIFFERENT volume
        /// from the source. Never modifies source files.
        /// </summary>
        /// <param name="sourceVolumeRoot">
        ///   Root of the source volume (e.g., "E:\"). Used only for validation.
        /// </param>
        /// <param name="files">Files to restore (must have SourceFullPath set).</param>
        /// <param name="targetFolder">Destination folder (must be on a different volume).</param>
        /// <param name="progress">Optional progress callback.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>A RecoverySession summarizing the operation. Never null.</returns>
        Task<RecoverySession> RestoreAsync(
            string sourceVolumeRoot,
            IReadOnlyList<RecoveredFileInfo> files,
            string targetFolder,
            IProgress<RecoveryProgressInfo>? progress,
            CancellationToken cancellationToken = default);
    }
}