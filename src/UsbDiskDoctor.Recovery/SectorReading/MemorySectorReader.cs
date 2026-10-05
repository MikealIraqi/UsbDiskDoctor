using System;
using System.Threading;

namespace UsbDiskDoctor.Recovery.SectorReading
{
    /// <summary>
    /// Sector reader that reads from an in-memory byte array.
    /// Useful for testing recovery logic without real disk access.
    /// </summary>
    public sealed class MemorySectorReader : ISectorReader
    {
        private readonly byte[] _data;

        public MemorySectorReader(byte[] data)
        {
            _data = data ?? throw new ArgumentNullException(nameof(data));
        }

        public long TotalLength => _data.Length;

        public int ReadAt(long offset, byte[] buffer, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (offset < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(offset), "Offset cannot be negative.");
            }

            if (buffer == null)
            {
                throw new ArgumentNullException(nameof(buffer));
            }

            if (buffer.Length == 0)
            {
                return 0;
            }

            if (offset >= _data.Length)
            {
                return 0;
            }

            var available = (int)Math.Min((long)buffer.Length, (long)_data.Length - offset);
            Array.Copy(_data, (int)offset, buffer, 0, available);
            return available;
        }

        public void Dispose()
        {
            // No unmanaged resources to release
        }
    }
}