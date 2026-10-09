using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace UsbDiskDoctor.Recovery.FakeCapacity;

public sealed class FakeCapacityChecker : IFakeCapacityChecker
{
    private const long QuickTestCapBytes = 4L * 1024 * 1024 * 1024;
    private const int MaxFileCount = 100_000;

    private readonly IDriveInfoProvider _driveProvider;

    public FakeCapacityChecker(IDriveInfoProvider driveProvider)
    {
        _driveProvider = driveProvider ?? throw new ArgumentNullException(nameof(driveProvider));
    }

    public async Task<FakeCapacityResult> CheckAsync(
        FakeCapacityRequest request,
        IProgress<FakeCapacityProgress>? progress = null,
        CancellationToken cancellationToken = default)
    {
        var sw = Stopwatch.StartNew();
        TestFolderManager? folderManager = null;

        try
        {
            var drive = _driveProvider.GetDrive(request.DriveLetter);
            if (drive is null)
            {
                return ErrorResult("Drive not found", sw.Elapsed);
            }

            var safety = FakeCapacitySafetyPolicy.Validate(drive);
            if (!safety.IsSafe)
            {
                return ErrorResult(safety.Reason, sw.Elapsed);
            }

            var fileSize = request.FileSizeBytes;
            if (fileSize <= 0)
            {
                return ErrorResult("Invalid FileSizeBytes", sw.Elapsed);
            }

            var maxSize = (long)(drive.FreeSpaceBytes * 0.9);
            if (request.TestSizeBytes > 0)
            {
                maxSize = Math.Min(maxSize, request.TestSizeBytes);
            }
            if (request.QuickTest)
            {
                maxSize = Math.Min(maxSize, QuickTestCapBytes);
            }

            var fileCountL = maxSize / fileSize;
            if (fileCountL < 1)
            {
                return ErrorResult("Not enough space for a single test file", sw.Elapsed);
            }

            var fileCount = (int)Math.Min(fileCountL, (long)MaxFileCount);

            cancellationToken.ThrowIfCancellationRequested();

            folderManager = new TestFolderManager(drive.DriveLetter);

            var totalBytes = (long)fileCount * fileSize;

            // ---------- Write Phase ----------
            progress?.Report(new FakeCapacityProgress
            {
                Phase = FakeCapacityPhase.Writing,
                BytesProcessed = 0,
                TotalBytes = totalBytes,
                CurrentFileIndex = 0,
                TotalFiles = fileCount,
                Elapsed = sw.Elapsed
            });

            long bytesWrittenTotal = 0L;
            var writeSuccess = new bool[fileCount];
            var expectedHash = new string[fileCount];

            for (var i = 0; i < fileCount; i++)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var pattern = PatternGenerator.Generate(i, fileSize);
                expectedHash[i] = PatternGenerator.ComputeSha256(pattern);
                var path = folderManager.GetFilePath(i);

                try
                {
                    using (var fs = new FileStream(
                        path,
                        FileMode.CreateNew,
                        FileAccess.Write,
                        FileShare.None,
                        bufferSize: 64 * 1024,
                        FileOptions.WriteThrough))
                    {
                        await fs.WriteAsync(pattern, cancellationToken);
                        fs.Flush(flushToDisk: true);
                    }

                    writeSuccess[i] = true;
                    bytesWrittenTotal += fileSize;
                }
                catch (OperationCanceledException)
                {
                    throw;
                }
                catch (IOException)
                {
                    writeSuccess[i] = false;
                    break;
                }
                catch (UnauthorizedAccessException)
                {
                    writeSuccess[i] = false;
                    break;
                }

                if (progress is not null && (i % 5 == 0 || i == fileCount - 1))
                {
                    progress.Report(new FakeCapacityProgress
                    {
                        Phase = FakeCapacityPhase.Writing,
                        BytesProcessed = (long)(i + 1) * fileSize,
                        TotalBytes = totalBytes,
                        CurrentFileIndex = i + 1,
                        TotalFiles = fileCount,
                        Elapsed = sw.Elapsed
                    });
                }
            }

            // ---------- Read Phase ----------
            progress?.Report(new FakeCapacityProgress
            {
                Phase = FakeCapacityPhase.Reading,
                BytesProcessed = 0,
                TotalBytes = totalBytes,
                CurrentFileIndex = 0,
                TotalFiles = fileCount,
                Elapsed = sw.Elapsed
            });

            long failedBytes = 0L;
            long bytesReadTotal = 0L;
            var firstFailIndex = -1;

            for (var i = 0; i < fileCount; i++)
            {
                cancellationToken.ThrowIfCancellationRequested();

                if (!writeSuccess[i])
                {
                    failedBytes += fileSize;
                    bytesReadTotal += fileSize;
                    if (firstFailIndex < 0)
                    {
                        firstFailIndex = i;
                    }
                    continue;
                }

                var path = folderManager.GetFilePath(i);
                var buffer = new byte[fileSize];
                var readOk = false;

                try
                {
                    using (var fs = new FileStream(
                        path,
                        FileMode.Open,
                        FileAccess.Read,
                        FileShare.None,
                        bufferSize: 64 * 1024,
                        FileOptions.SequentialScan))
                    {
                        await fs.ReadExactlyAsync(buffer.AsMemory(0, fileSize), cancellationToken);
                        readOk = true;
                    }
                }
                catch (OperationCanceledException)
                {
                    throw;
                }
                catch (IOException)
                {
                    readOk = false;
                }
                catch (UnauthorizedAccessException)
                {
                    readOk = false;
                }

                bytesReadTotal += fileSize;

                if (!readOk)
                {
                    failedBytes += fileSize;
                    if (firstFailIndex < 0)
                    {
                        firstFailIndex = i;
                    }
                }
                else
                {
                    var actualHash = PatternGenerator.ComputeSha256(buffer);
                    if (!string.Equals(actualHash, expectedHash[i], StringComparison.OrdinalIgnoreCase))
                    {
                        failedBytes += fileSize;
                        if (firstFailIndex < 0)
                        {
                            firstFailIndex = i;
                        }
                    }
                }

                if (progress is not null && (i % 5 == 0 || i == fileCount - 1))
                {
                    progress.Report(new FakeCapacityProgress
                    {
                        Phase = FakeCapacityPhase.Reading,
                        BytesProcessed = (long)(i + 1) * fileSize,
                        TotalBytes = totalBytes,
                        CurrentFileIndex = i + 1,
                        TotalFiles = fileCount,
                        Elapsed = sw.Elapsed
                    });
                }
            }

            // ---------- Final progress ----------
            progress?.Report(new FakeCapacityProgress
            {
                Phase = FakeCapacityPhase.Completed,
                BytesProcessed = bytesReadTotal,
                TotalBytes = totalBytes,
                CurrentFileIndex = fileCount,
                TotalFiles = fileCount,
                Elapsed = sw.Elapsed
            });

            // ---------- Result classification ----------
            var failurePercentage = bytesReadTotal > 0
                ? ((double)failedBytes / bytesReadTotal) * 100.0
                : 0.0;

            FakeCapacityStatus status;
            if (failurePercentage == 0.0)
            {
                status = FakeCapacityStatus.Genuine;
            }
            else if (failurePercentage <= 5.0)
            {
                status = FakeCapacityStatus.Suspect;
            }
            else
            {
                status = FakeCapacityStatus.FakeCapacity;
            }

            var detectedRealCapacityBytes = firstFailIndex >= 0
                ? (long)firstFailIndex * fileSize
                : drive.TotalSizeBytes;

            var failedCount = (int)(failedBytes / fileSize);
            var details =
                $"الملفات: {fileCount} | حجم الملف: {fileSize} بايت | " +
                $"الفشل: {failedCount} | نسبة الفشل: {failurePercentage:F2}% | " +
                $"أول فشل عند الفهرس: {firstFailIndex}";

            return new FakeCapacityResult
            {
                Status = status,
                TotalBytesWritten = bytesWrittenTotal,
                TotalBytesRead = bytesReadTotal,
                FailedBytes = failedBytes,
                FailurePercentage = failurePercentage,
                DetectedRealCapacityBytes = detectedRealCapacityBytes,
                Details = details,
                Duration = sw.Elapsed
            };
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            return new FakeCapacityResult
            {
                Status = FakeCapacityStatus.Error,
                TotalBytesWritten = 0,
                TotalBytesRead = 0,
                FailedBytes = 0,
                FailurePercentage = 0,
                DetectedRealCapacityBytes = 0,
                Details = "Exception: " + ex.Message,
                Duration = sw.Elapsed
            };
        }
        finally
        {
            folderManager?.Cleanup();
        }
    }

    private static FakeCapacityResult ErrorResult(string reason, TimeSpan elapsed)
    {
        return new FakeCapacityResult
        {
            Status = FakeCapacityStatus.Error,
            TotalBytesWritten = 0,
            TotalBytesRead = 0,
            FailedBytes = 0,
            FailurePercentage = 0,
            DetectedRealCapacityBytes = 0,
            Details = reason,
            Duration = elapsed
        };
    }
}