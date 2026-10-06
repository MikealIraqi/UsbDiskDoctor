// File: src/UsbDiskDoctor.Recovery/CapacityChecking/FakeCapacityChecker.cs
using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UsbDiskDoctor.Recovery.Models;

namespace UsbDiskDoctor.Recovery.CapacityChecking;

/// <summary>
/// Implements fake capacity checking by writing and verifying test patterns across the storage device.
/// </summary>
public sealed class FakeCapacityChecker : IFakeCapacityChecker
{
    /// <summary>
    /// Checks a storage device for fake capacity by writing and verifying test patterns.
    /// </summary>
    /// <param name="driveRoot">Root path of the drive to check (e.g., "E:\").</param>
    /// <param name="options">Check options including mode, block size, and test file path.</param>
    /// <param name="progress">Optional progress reporter for UI updates.</param>
    /// <param name="cancellationToken">Cancellation token. Must be respected; OperationCanceledException is thrown on cancellation.</param>
    /// <returns>A CapacityCheckResult containing the verdict and diagnostic information.</returns>
    /// <exception cref="ArgumentException">Thrown when driveRoot or options.TestFilePath is null or empty.</exception>
    /// <exception cref="ArgumentNullException">Thrown when options is null.</exception>
    /// <exception cref="OperationCanceledException">Thrown when the operation is cancelled via cancellationToken.</exception>
    public async Task<CapacityCheckResult> CheckAsync(
        string driveRoot,
        CapacityCheckOptions options,
        IProgress<CapacityCheckProgress>? progress,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrEmpty(driveRoot);
        ArgumentNullException.ThrowIfNull(options);
        ArgumentException.ThrowIfNullOrEmpty(options.TestFilePath);

        if (options.BlockSizeBytes <= 16)
        {
            throw new ArgumentException(
                "BlockSizeBytes must be greater than 16 to accommodate magic bytes and index.",
                nameof(options));
        }

        // Safety: TestFilePath must reside on the same volume as driveRoot.
        string testRoot = Path.GetPathRoot(Path.GetFullPath(options.TestFilePath)) ?? string.Empty;
        string targetRoot = Path.GetPathRoot(Path.GetFullPath(driveRoot)) ?? string.Empty;
        if (!string.Equals(testRoot, targetRoot, StringComparison.OrdinalIgnoreCase))
        {
            return new CapacityCheckResult
            {
                DriveRoot = driveRoot,
                Verdict = CapacityVerdict.Failed,
                DiagnosticMessage = "TestFilePath must reside on the same volume as driveRoot."
            };
        }

        // Safety: refuse to write on non-empty volumes unless explicitly allowed.
        if (!options.AllowWriteOnNonEmptyVolume)
        {
            try
            {
                var rootInfo = new DirectoryInfo(driveRoot);
                bool hasUserEntries = rootInfo
                    .EnumerateFileSystemInfos()
                    .Any(fi => (fi.Attributes & FileAttributes.System) == 0);

                if (hasUserEntries)
                {
                    return new CapacityCheckResult
                    {
                        DriveRoot = driveRoot,
                        Verdict = CapacityVerdict.Failed,
                        DiagnosticMessage = "Volume is not empty. Set AllowWriteOnNonEmptyVolume=true to override."
                    };
                }
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
            {
                return new CapacityCheckResult
                {
                    DriveRoot = driveRoot,
                    Verdict = CapacityVerdict.Failed,
                    DiagnosticMessage = $"Failed to inspect volume contents: {ex.Message}"
                };
            }
        }

        DriveInfo driveInfo;
        try
        {
            driveInfo = new DriveInfo(driveRoot);
        }
        catch (ArgumentException)
        {
            return new CapacityCheckResult
            {
                DriveRoot = driveRoot,
                Verdict = CapacityVerdict.Failed,
                DiagnosticMessage = "Invalid drive root or drive not found."
            };
        }

        if (!driveInfo.IsReady)
        {
            return new CapacityCheckResult
            {
                DriveRoot = driveRoot,
                Verdict = CapacityVerdict.Failed,
                DiagnosticMessage = "Drive is not ready."
            };
        }

        long claimed = driveInfo.TotalSize;
        int blockSizeInt = options.BlockSizeBytes;
        long blockSize = blockSizeInt;
        long totalBlocks = claimed / blockSize;

        if (totalBlocks == 0)
        {
            return new CapacityCheckResult
            {
                DriveRoot = driveRoot,
                ClaimedCapacityBytes = claimed,
                Verdict = CapacityVerdict.Failed,
                DiagnosticMessage = "Drive capacity is smaller than one block."
            };
        }

        long sampleCountLong = options.Mode switch
        {
            CapacityCheckMode.Quick => 100L,
            CapacityCheckMode.Smart => Math.Min(1000L, totalBlocks),
            CapacityCheckMode.Full => totalBlocks,
            _ => 100L
        };

        if (options.Mode == CapacityCheckMode.Full && totalBlocks > int.MaxValue)
        {
            throw new InvalidOperationException(
                "Full mode is not supported for drives exceeding 2TB with 1KB blocks (totalBlocks > int.MaxValue).");
        }

        int sampleCount = (int)Math.Min((long)int.MaxValue, sampleCountLong);
        if (sampleCount <= 0)
        {
            sampleCount = 1;
        }

        var sortedIndices = ComputeSampleIndices(totalBlocks, sampleCount);

        FileStream? fs = null;
        try
        {
            fs = new FileStream(
                options.TestFilePath,
                FileMode.Create,
                FileAccess.ReadWrite,
                FileShare.None,
                bufferSize: blockSizeInt,
                FileOptions.WriteThrough | FileOptions.Asynchronous);
        }
        catch (IOException openIoEx)
        {
            return new CapacityCheckResult
            {
                DriveRoot = driveRoot,
                ClaimedCapacityBytes = claimed,
                Verdict = CapacityVerdict.Failed,
                DiagnosticMessage = $"Failed to open test file: {openIoEx.Message}"
            };
        }
        catch (UnauthorizedAccessException openUaEx)
        {
            return new CapacityCheckResult
            {
                DriveRoot = driveRoot,
                ClaimedCapacityBytes = claimed,
                Verdict = CapacityVerdict.Failed,
                DiagnosticMessage = $"Access denied to test file: {openUaEx.Message}"
            };
        }

        try
        {
            await using (fs.ConfigureAwait(false))
            {
                var sw = Stopwatch.StartNew();
                long bytesProcessed = 0;
                long bytesTotal = blockSize * sampleCount * 2;
                DateTime lastReportTime = DateTime.MinValue;
                double lastReportPercent = -1.0;

                void ReportProgress(CapacityCheckPhase phase)
                {
                    double percent = bytesTotal > 0 ? (double)bytesProcessed / bytesTotal * 100.0 : 0.0;
                    double elapsedSec = sw.Elapsed.TotalSeconds;
                    double speedMb = elapsedSec > 0 ? (bytesProcessed / elapsedSec) / 1024.0 / 1024.0 : 0.0;
                    TimeSpan estimatedRemaining = TimeSpan.Zero;
                    if (speedMb > 0 && bytesTotal > bytesProcessed)
                    {
                        double remainingBytes = bytesTotal - bytesProcessed;
                        double remainingSec = remainingBytes / (speedMb * 1024.0 * 1024.0);
                        estimatedRemaining = TimeSpan.FromSeconds(remainingSec);
                    }

                    bool timePassed = (DateTime.UtcNow - lastReportTime).TotalMilliseconds >= 250;
                    bool percentChanged = Math.Abs(percent - lastReportPercent) >= 1.0;

                    if (timePassed || percentChanged
                        || phase == CapacityCheckPhase.Completed
                        || phase == CapacityCheckPhase.Failed
                        || phase == CapacityCheckPhase.Cancelled)
                    {
                        progress?.Report(new CapacityCheckProgress
                        {
                            Phase = phase,
                            PercentComplete = percent,
                            BytesProcessed = bytesProcessed,
                            BytesTotal = bytesTotal,
                            CurrentSpeedMbPerSec = speedMb,
                            Elapsed = sw.Elapsed,
                            EstimatedRemaining = estimatedRemaining
                        });
                        lastReportTime = DateTime.UtcNow;
                        lastReportPercent = percent;
                    }
                }

                ReportProgress(CapacityCheckPhase.Preparing);

                try
                {
                    fs.SetLength(claimed);
                }
                catch (IOException setLenIoEx)
                {
                    ReportProgress(CapacityCheckPhase.Failed);
                    return new CapacityCheckResult
                    {
                        DriveRoot = driveRoot,
                        ClaimedCapacityBytes = claimed,
                        Verdict = CapacityVerdict.Fake,
                        DiagnosticMessage = $"SetLength failed, indicating fake capacity: {setLenIoEx.Message}"
                    };
                }
                catch (UnauthorizedAccessException setLenUaEx)
                {
                    ReportProgress(CapacityCheckPhase.Failed);
                    return new CapacityCheckResult
                    {
                        DriveRoot = driveRoot,
                        ClaimedCapacityBytes = claimed,
                        Verdict = CapacityVerdict.Failed,
                        DiagnosticMessage = $"Access denied during SetLength: {setLenUaEx.Message}"
                    };
                }

                byte[] buffer = new byte[blockSizeInt];
                var failures = new List<(long expectedIndex, long readIndex)>();
                int blocksWritten = 0;

                ReportProgress(CapacityCheckPhase.Writing);
                try
                {
                    foreach (long writeBlockIdx in sortedIndices)
                    {
                        cancellationToken.ThrowIfCancellationRequested();
                        BuildBlock(buffer, writeBlockIdx, blockSizeInt);
                        fs.Position = writeBlockIdx * blockSize;
                        await fs.WriteAsync(buffer, 0, blockSizeInt, cancellationToken).ConfigureAwait(false);
                        blocksWritten++;
                        bytesProcessed += blockSize;
                        ReportProgress(CapacityCheckPhase.Writing);
                    }
                    await fs.FlushAsync(cancellationToken).ConfigureAwait(false);
                }
                catch (OperationCanceledException)
                {
                    ReportProgress(CapacityCheckPhase.Cancelled);
                    throw;
                }
                catch (IOException writeIoEx)
                {
                    ReportProgress(CapacityCheckPhase.Failed);
                    return new CapacityCheckResult
                    {
                        DriveRoot = driveRoot,
                        ClaimedCapacityBytes = claimed,
                        Verdict = blocksWritten > 0 ? CapacityVerdict.Inconclusive : CapacityVerdict.Failed,
                        DiagnosticMessage = $"IOException during writing: {writeIoEx.Message}"
                    };
                }

                int blocksRead = 0;
                ReportProgress(CapacityCheckPhase.Reading);
                try
                {
                    foreach (long readBlockIdx in sortedIndices)
                    {
                        cancellationToken.ThrowIfCancellationRequested();
                        fs.Position = readBlockIdx * blockSize;
                        await ReadExactlyAsync(fs, buffer, blockSizeInt, cancellationToken).ConfigureAwait(false);
                        blocksRead++;
                        long readback = ParseBlockIndex(buffer);
                        if (readback != readBlockIdx)
                        {
                            failures.Add((readBlockIdx, readback));
                        }
                        bytesProcessed += blockSize;
                        ReportProgress(CapacityCheckPhase.Reading);
                    }
                }
                catch (OperationCanceledException)
                {
                    ReportProgress(CapacityCheckPhase.Cancelled);
                    throw;
                }
                catch (EndOfStreamException readEosEx)
                {
                    // Must precede the IOException catch: EndOfStreamException derives from IOException.
                    ReportProgress(CapacityCheckPhase.Failed);
                    return new CapacityCheckResult
                    {
                        DriveRoot = driveRoot,
                        ClaimedCapacityBytes = claimed,
                        Verdict = blocksRead > 0 ? CapacityVerdict.Inconclusive : CapacityVerdict.Failed,
                        DiagnosticMessage = $"EndOfStreamException during reading: {readEosEx.Message}"
                    };
                }
                catch (IOException readIoEx)
                {
                    ReportProgress(CapacityCheckPhase.Failed);
                    return new CapacityCheckResult
                    {
                        DriveRoot = driveRoot,
                        ClaimedCapacityBytes = claimed,
                        Verdict = blocksRead > 0 ? CapacityVerdict.Inconclusive : CapacityVerdict.Failed,
                        DiagnosticMessage = $"IOException during reading: {readIoEx.Message}"
                    };
                }

                ReportProgress(CapacityCheckPhase.Verifying);

                if (failures.Count == 0)
                {
                    ReportProgress(CapacityCheckPhase.Completed);
                    return new CapacityCheckResult
                    {
                        DriveRoot = driveRoot,
                        ClaimedCapacityBytes = claimed,
                        ActualCapacityBytes = claimed,
                        Verdict = CapacityVerdict.Genuine,
                        TotalBlocksChecked = sortedIndices.Count,
                        FailedBlocksCount = 0,
                        Duration = sw.Elapsed,
                        DiagnosticMessage = "Capacity is genuine."
                    };
                }

                var firstFailure = failures[0];
                long failedExpected = firstFailure.expectedIndex;
                long failedReadback = firstFailure.readIndex;
                long actualBytes = ComputeActualCapacity(totalBlocks, blockSize, failedExpected, failedReadback);

                double claimedGb = claimed / (1024.0 * 1024.0 * 1024.0);
                double actualGb = actualBytes / (1024.0 * 1024.0 * 1024.0);
                string verdictMsg =
                    $"Claimed {claimedGb:F2} GB, actual \u2248 {actualGb:F2} GB (block wraparound detected at sample {failedExpected})";

                ReportProgress(CapacityCheckPhase.Completed);
                return new CapacityCheckResult
                {
                    DriveRoot = driveRoot,
                    ClaimedCapacityBytes = claimed,
                    ActualCapacityBytes = actualBytes,
                    Verdict = CapacityVerdict.Fake,
                    TotalBlocksChecked = sortedIndices.Count,
                    FailedBlocksCount = failures.Count,
                    Duration = sw.Elapsed,
                    DiagnosticMessage = verdictMsg
                };
            }
        }
        finally
        {
            try
            {
                File.Delete(options.TestFilePath);
            }
            catch
            {
                // Best-effort cleanup; ignore failures.
            }
        }
    }

    /// <summary>
    /// Computes evenly distributed sample block indices across the claimed capacity.
    /// </summary>
    /// <param name="totalBlocks">Total blocks based on claimed capacity.</param>
    /// <param name="sampleCount">Number of samples to distribute.</param>
    /// <returns>Sorted, distinct list of block indices.</returns>
    internal static List<long> ComputeSampleIndices(long totalBlocks, int sampleCount)
    {
        var sampleBlockIndices = new HashSet<long>();
        for (long sampleIdx = 0; sampleIdx < sampleCount; sampleIdx++)
        {
            long index = sampleCount == 1 ? 0 : (totalBlocks - 1) * sampleIdx / (sampleCount - 1);
            sampleBlockIndices.Add(index);
        }
        return sampleBlockIndices.OrderBy(x => x).ToList();
    }

    /// <summary>
    /// Computes the actual capacity in bytes based on a wraparound failure.
    /// </summary>
    /// <param name="totalBlocks">Total blocks based on claimed capacity.</param>
    /// <param name="blockSize">Size of each block in bytes.</param>
    /// <param name="failedExpected">Block index that was written.</param>
    /// <param name="failedReadback">Block index that was read back (differs from expected = wraparound).</param>
    /// <returns>Actual capacity in bytes, with a minimum of one block.</returns>
    internal static long ComputeActualCapacity(
        long totalBlocks,
        long blockSize,
        long failedExpected,
        long failedReadback)
    {
        long deltaBlocks = failedReadback - failedExpected;
        long actualBlocks = totalBlocks - deltaBlocks;
        if (actualBlocks < 1)
        {
            actualBlocks = 1;
        }
        return actualBlocks * blockSize;
    }

    /// <summary>
    /// Builds a test block with magic header and block index for later verification.
    /// </summary>
    /// <param name="buffer">Destination buffer; must be at least <paramref name="blockSize"/> bytes.</param>
    /// <param name="blockIndex">Unique block index written into the header.</param>
    /// <param name="blockSize">Total block size in bytes.</param>
    internal static void BuildBlock(byte[] buffer, long blockIndex, int blockSize)
    {
        // Magic "USBDDOC1"
        buffer[0] = 0x55;
        buffer[1] = 0x53;
        buffer[2] = 0x42;
        buffer[3] = 0x44;
        buffer[4] = 0x44;
        buffer[5] = 0x4F;
        buffer[6] = 0x43;
        buffer[7] = 0x31;

        BinaryPrimitives.WriteInt64LittleEndian(buffer.AsSpan(8, 8), blockIndex);

        Array.Clear(buffer, 16, blockSize - 16);
    }

    /// <summary>
    /// Reads the block index stored in the block header.
    /// </summary>
    /// <param name="buffer">Block buffer previously written by <see cref="BuildBlock"/>.</param>
    /// <returns>The block index that was stored.</returns>
    internal static long ParseBlockIndex(byte[] buffer)
    {
        return BinaryPrimitives.ReadInt64LittleEndian(buffer.AsSpan(8, 8));
    }

    private static async Task ReadExactlyAsync(
        Stream stream,
        byte[] buffer,
        int count,
        CancellationToken cancellationToken)
    {
        int offset = 0;
        while (offset < count)
        {
            int read = await stream
                .ReadAsync(buffer.AsMemory(offset, count - offset), cancellationToken)
                .ConfigureAwait(false);
            if (read == 0)
            {
                throw new EndOfStreamException($"End of stream reached before reading {count} bytes.");
            }
            offset += read;
        }
    }
}