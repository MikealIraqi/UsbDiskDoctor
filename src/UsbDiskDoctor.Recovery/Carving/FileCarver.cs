using System;
using System.Collections.Generic;
using System.Threading;
using Serilog;
using UsbDiskDoctor.Recovery.Models;
using UsbDiskDoctor.Recovery.SectorReading;

namespace UsbDiskDoctor.Recovery.Carving
{
    /// <summary>
    /// Implements file carving by scanning for known file signatures in raw disk data.
    /// Stateless design — all state is local to the Scan method.
    /// </summary>
    public sealed class FileCarver : IFileCarver
    {
        private const int ChunkSize = 4 * 1024 * 1024;
        private const int OverlapSize = 64;
        private const long MinFileSizeBytes = 512;
        private const int MaxBufferSize = ChunkSize + OverlapSize;

        private static readonly ILogger _log = Log.ForContext<FileCarver>();

        public IReadOnlyList<RecoveredFileInfo> Scan(
            ISectorReader reader,
            long maxFileSizeBytes,
            IProgress<RecoveryProgressInfo>? progress = null,
            CancellationToken cancellationToken = default)
        {
            if (reader == null)
            {
                throw new ArgumentNullException(nameof(reader));
            }

            if (maxFileSizeBytes <= 0)
            {
                maxFileSizeBytes = 100L * 1024 * 1024;
            }

            var results = new List<RecoveredFileInfo>();
            var buffer = new byte[MaxBufferSize];

            FileSignature? pendingSignature = null;
            long pendingStartOffset = 0;

            long position = 0;
            long totalLength = reader.TotalLength;
            long bytesScanned = 0;
            int counter = 0;

            while (position < totalLength)
            {
                cancellationToken.ThrowIfCancellationRequested();

                int bytesRead = reader.ReadAt(position, buffer, cancellationToken);
                if (bytesRead <= 0)
                {
                    break;
                }

                long remainingInSource = totalLength - (position + bytesRead);
                int scanLimit = bytesRead;
                if (remainingInSource > 0 && scanLimit > OverlapSize)
                {
                    scanLimit -= OverlapSize;
                }

                for (int localOffset = 0; localOffset < scanLimit; localOffset++)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    long absoluteOffset = position + localOffset;

                    if (pendingSignature == null)
                    {
                        var matched = MatchStartSignatureAt(buffer, localOffset, scanLimit);
                        if (matched != null)
                        {
                            pendingSignature = matched;
                            pendingStartOffset = absoluteOffset;
                            localOffset += matched.StartMarker.Length - 1;
                        }
                    }
                    else
                    {
                        if (MatchAt(buffer, localOffset, scanLimit, pendingSignature.EndMarker))
                        {
                            long endOffset = absoluteOffset + pendingSignature.EndMarker.Length;
                            long fileSize = endOffset - pendingStartOffset;

                            if (fileSize >= MinFileSizeBytes && fileSize <= maxFileSizeBytes)
                            {
                                counter++;
                                var recoveredFile = new RecoveredFileInfo
                                {
                                    FileName = $"carved_{counter:D5}{pendingSignature.Extension}",
                                    Extension = pendingSignature.Extension,
                                    SizeBytes = fileSize,
                                    SourceOffset = pendingStartOffset,
                                    SourceFullPath = string.Empty,
                                    TargetFullPath = string.Empty
                                };
                                results.Add(recoveredFile);

                                if (progress != null && counter % 10 == 0)
                                {
                                    var percentComplete = totalLength > 0
                                        ? (int)((bytesScanned * 100) / totalLength)
                                        : 0;

                                    progress.Report(new RecoveryProgressInfo
                                    {
                                        BytesScanned = bytesScanned,
                                        TotalBytes = totalLength,
                                        FilesFound = counter,
                                        PercentComplete = percentComplete
                                    });
                                }
                            }

                            pendingSignature = null;
                            pendingStartOffset = 0;
                            localOffset += 1;
                        }
                        else if ((absoluteOffset - pendingStartOffset) > maxFileSizeBytes)
                        {
                            _log.Debug("Abandoned oversized candidate at offset {Offset}.", pendingStartOffset);
                            pendingSignature = null;
                            pendingStartOffset = 0;
                        }
                    }
                }

                bytesScanned += scanLimit;

                if (remainingInSource <= 0)
                {
                    break;
                }

                long nextPosition = position + bytesRead - OverlapSize;
                if (nextPosition <= position)
                {
                    break;
                }
                position = nextPosition;
            }

            if (progress != null)
            {
                progress.Report(new RecoveryProgressInfo
                {
                    BytesScanned = bytesScanned,
                    TotalBytes = totalLength,
                    FilesFound = counter,
                    PercentComplete = 100
                });
            }

            _log.Information("File carving completed: {FilesFound} files found in {BytesScanned} bytes scanned.", counter, bytesScanned);
            return results;
        }

        private static FileSignature? MatchStartSignatureAt(byte[] buffer, int localOffset, int scanLimit)
        {
            foreach (var signature in FileSignatureCatalog.All)
            {
                if (MatchAt(buffer, localOffset, scanLimit, signature.StartMarker))
                {
                    return signature;
                }
            }
            return null;
        }

        private static bool MatchAt(byte[] buffer, int localOffset, int scanLimit, byte[] pattern)
        {
            if (localOffset + pattern.Length > scanLimit)
            {
                return false;
            }

            for (int i = 0; i < pattern.Length; i++)
            {
                if (buffer[localOffset + i] != pattern[i])
                {
                    return false;
                }
            }

            return true;
        }
    }
}