using System;
using System.Threading;

namespace UsbDiskDoctor.Recovery.SectorReading
{
    /// <summary>
    /// Abstraction for reading bytes from a storage source at specific offsets.
    /// Implementations can read from memory, files, or raw disk devices.
    /// </summary>
    public interface ISectorReader : IDisposable
    {
        /// <summary>
        /// Total size of the readable source in bytes (0 if unknown).
        /// </summary>
        long TotalLength { get; }

        /// <summary>
        /// Reads up to buffer.Length bytes at the given offset.
        /// Returns the number of bytes actually read (may be less than buffer.Length at end).
        /// </summary>
        /// <param name="offset">Byte offset from the start of the source.</param>
        /// <param name="buffer">Buffer to fill.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Number of bytes read.</returns>
        int ReadAt(long offset, byte[] buffer, CancellationToken cancellationToken = default);
    }
}