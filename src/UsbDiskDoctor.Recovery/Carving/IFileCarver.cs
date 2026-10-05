using System;
using System.Collections.Generic;
using System.Threading;
using UsbDiskDoctor.Recovery.Models;
using UsbDiskDoctor.Recovery.SectorReading;

namespace UsbDiskDoctor.Recovery.Carving
{
    /// <summary>
    /// Scans storage media for recoverable files using file signature detection (carving).
    /// </summary>
    public interface IFileCarver
    {
        /// <summary>
        /// Scans the source for known file signatures and returns metadata
        /// about every recoverable file found. Does NOT extract bytes —
        /// only records offset and length.
        /// </summary>
        /// <param name="reader">Source sector reader.</param>
        /// <param name="maxFileSizeBytes">Maximum size per carved file.</param>
        /// <param name="progress">Optional progress callback.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Read-only list of recovered file metadata. Never null.</returns>
        IReadOnlyList<RecoveredFileInfo> Scan(
            ISectorReader reader,
            long maxFileSizeBytes,
            IProgress<RecoveryProgressInfo>? progress = null,
            CancellationToken cancellationToken = default);
    }
}