using System;
using System.IO;
using System.Threading;

namespace UsbDiskDoctor.Recovery.SectorReading
{
    /// <summary>
    /// Sector reader that reads from a file using FileStream.
    /// Useful for testing with disk images or ISO files.
    /// Strictly read-only — never writes to the file.
    /// </summary>
    public sealed class FileStreamSectorReader : ISectorReader
    {
        private readonly FileStream _fileStream;
        private bool _disposed;

        public FileStreamSectorReader(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
            {
                throw new ArgumentException("File path cannot be empty.", nameof(filePath));
            }

            _fileStream = new FileStream(
                filePath,
                FileMode.Open,
                FileAccess.Read,
                FileShare.Read,
                bufferSize: 64 * 1024,
                FileOptions.SequentialScan);
        }

        public long TotalLength => _fileStream.Length;

        public int ReadAt(long offset, byte[] buffer, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (_disposed)
            {
                throw new ObjectDisposedException(nameof(FileStreamSectorReader));
            }

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

            _fileStream.Seek(offset, SeekOrigin.Begin);

            int totalRead = 0;
            while (totalRead < buffer.Length)
            {
                int read = _fileStream.Read(buffer, totalRead, buffer.Length - totalRead);
                if (read == 0)
                {
                    break;
                }
                totalRead += read;
            }

            return totalRead;
        }

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;
            _fileStream.Dispose();
        }
    }
}