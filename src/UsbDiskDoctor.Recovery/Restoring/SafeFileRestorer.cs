using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Serilog;
using UsbDiskDoctor.Core.Enums;
using UsbDiskDoctor.Core.Models;
using UsbDiskDoctor.Recovery.Models;

namespace UsbDiskDoctor.Recovery.Restoring
{
    /// <summary>
    /// Secure implementation of <see cref="IFileRestorer"/>.
    /// Multiple layers of validation before any byte is written.
    /// </summary>
    public sealed class SafeFileRestorer : IFileRestorer
    {
        private static readonly ILogger _log = Log.ForContext<SafeFileRestorer>();

        public async Task<RecoverySession> RestoreAsync(
            string sourceVolumeRoot,
            IReadOnlyList<RecoveredFileInfo> files,
            string targetFolder,
            IProgress<RecoveryProgressInfo>? progress,
            CancellationToken cancellationToken = default)
        {
            // -------- Input validation --------
            if (string.IsNullOrWhiteSpace(sourceVolumeRoot))
            {
                throw new ArgumentException("Source volume root is required.", nameof(sourceVolumeRoot));
            }

            if (files == null)
            {
                throw new ArgumentNullException(nameof(files));
            }

            if (string.IsNullOrWhiteSpace(targetFolder))
            {
                throw new ArgumentException("Target folder is required.", nameof(targetFolder));
            }

            var session = new RecoverySession
            {
                SourceVolume = sourceVolumeRoot,
                TargetFolder = targetFolder,
                ScanMode = "MountedVolume",
                Status = RecoverySessionStatus.Recovering,
                StartedAt = DateTimeOffset.UtcNow
            };

            // -------- Path normalization --------
            var normalizedSource = NormalizeRoot(sourceVolumeRoot);
            var normalizedTarget = Path.GetFullPath(targetFolder);

            // -------- Layer 1: Target must NOT be inside source --------
            if (IsPathInsideOrEqual(normalizedTarget, normalizedSource))
            {
                _log.Error(
                    "Rejected restore: target folder is inside source volume. Source={Source}, Target={Target}",
                    normalizedSource, normalizedTarget);
                return Fail(session,
                    "لا يمكن الاستعادة إلى نفس القرص المصدر. اختر مجلداً على قرص آخر.",
                    "Cannot restore to the source volume. Choose a folder on a different drive.");
            }

            // -------- Layer 2: Source and target must be on different roots --------
            var sourceRoot = Path.GetPathRoot(normalizedSource);
            var targetRoot = Path.GetPathRoot(normalizedTarget);

            if (string.IsNullOrEmpty(sourceRoot) || string.IsNullOrEmpty(targetRoot))
            {
                return Fail(session,
                    "تعذر تحديد جذر أحد المسارين.",
                    "Could not determine root of source or target path.");
            }

            if (string.Equals(sourceRoot, targetRoot, StringComparison.OrdinalIgnoreCase))
            {
                _log.Error(
                    "Rejected restore: same volume root. SourceRoot={SourceRoot}, TargetRoot={TargetRoot}",
                    sourceRoot, targetRoot);
                return Fail(session,
                    "المصدر والهدف على نفس القسم. يجب اختيار قسم مختلف.",
                    "Source and target are on the same volume. Choose a different volume.");
            }

            // -------- Layer 3: Free space check --------
            try
            {
                var targetDriveInfo = new DriveInfo(targetRoot);
                long totalRequested = 0;
                foreach (var fileForSize in files)
                {
                    if (fileForSize != null && fileForSize.SizeBytes > 0)
                    {
                        totalRequested += fileForSize.SizeBytes;
                    }
                }

                // Reserve 1 MB safety buffer
                const long SafetyBufferBytes = 1024L * 1024L;
                var requiredBytes = totalRequested + SafetyBufferBytes;

                if (targetDriveInfo.AvailableFreeSpace < requiredBytes)
                {
                    _log.Error(
                        "Insufficient free space. Required={Required}, Available={Available}",
                        requiredBytes, targetDriveInfo.AvailableFreeSpace);
                    return Fail(session,
                        $"المساحة غير كافية. مطلوب {requiredBytes} بايت، متاح {targetDriveInfo.AvailableFreeSpace} بايت.",
                        $"Insufficient space. Required {requiredBytes} bytes, available {targetDriveInfo.AvailableFreeSpace}.");
                }
            }
            catch (Exception driveException)
            {
                _log.Error(driveException, "Failed to read target drive info.");
                return Fail(session,
                    "تعذر قراءة معلومات القرص الهدف.",
                    "Failed to read target drive information.");
            }

            // -------- Create target folder --------
            try
            {
                Directory.CreateDirectory(normalizedTarget);
            }
            catch (Exception createException)
            {
                _log.Error(createException, "Failed to create target folder: {Target}", normalizedTarget);
                return Fail(session,
                    "تعذر إنشاء المجلد الهدف.",
                    "Failed to create target folder.");
            }

            // -------- Perform copies --------
            var errors = new List<string>();
            int recoveredCount = 0;
            long totalBytesCopied = 0;
            int attempted = 0;

            await Task.Run(() =>
            {
                foreach (var fileItem in files)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    if (fileItem == null)
                    {
                        continue;
                    }

                    attempted++;

                    if (string.IsNullOrWhiteSpace(fileItem.SourceFullPath))
                    {
                        errors.Add($"Missing source path for {fileItem.FileName}");
                        continue;
                    }

                    try
                    {
                        var uniqueTargetPath = BuildUniqueTargetPath(normalizedTarget, fileItem.FileName);

                        File.Copy(fileItem.SourceFullPath, uniqueTargetPath, overwrite: false);

                        recoveredCount++;
                        totalBytesCopied += fileItem.SizeBytes;

                        _log.Information("Recovered: {Src} -> {Dst}", fileItem.SourceFullPath, uniqueTargetPath);
                    }
                    catch (Exception fileException)
                    {
                        _log.Warning(fileException, "Failed to recover {FileName}", fileItem.FileName);
                        errors.Add($"{fileItem.FileName}: {fileException.Message}");
                    }

                    if (progress != null && attempted % 10 == 0)
                    {
                        progress.Report(new RecoveryProgressInfo
                        {
                            BytesScanned = totalBytesCopied,
                            TotalBytes = 0,
                            FilesFound = recoveredCount,
                            PercentComplete = 0
                        });
                    }
                }
            }, cancellationToken);

            // -------- Finalize session --------
            var finalStatus = recoveredCount > 0
                ? RecoverySessionStatus.Completed
                : RecoverySessionStatus.Failed;

            var completedSession = session with
            {
                Status = finalStatus,
                FinishedAt = DateTimeOffset.UtcNow,
                RecoveredFilesCount = recoveredCount,
                Errors = errors
            };

            if (progress != null)
            {
                progress.Report(new RecoveryProgressInfo
                {
                    BytesScanned = totalBytesCopied,
                    TotalBytes = totalBytesCopied,
                    FilesFound = recoveredCount,
                    PercentComplete = 100
                });
            }

            _log.Information(
                "Restore finished. Recovered={Recovered}, Attempted={Attempted}, Errors={Errors}",
                recoveredCount, attempted, errors.Count);

            return completedSession;
        }

        private static string NormalizeRoot(string path)
        {
            var trimmed = path.Trim();
            if (!trimmed.EndsWith('\\'))
            {
                trimmed += '\\';
            }
            return Path.GetFullPath(trimmed);
        }

        private static bool IsPathInsideOrEqual(string candidate, string parent)
        {
            var normalizedCandidate = candidate.TrimEnd('\\') + '\\';
            var normalizedParent = parent.TrimEnd('\\') + '\\';

            return normalizedCandidate.StartsWith(normalizedParent, StringComparison.OrdinalIgnoreCase);
        }

        private static string BuildUniqueTargetPath(string folder, string fileName)
        {
            var safeName = string.IsNullOrWhiteSpace(fileName) ? "recovered_file" : fileName;
            var candidate = Path.Combine(folder, safeName);

            if (!File.Exists(candidate))
            {
                return candidate;
            }

            var baseName = Path.GetFileNameWithoutExtension(safeName);
            var extension = Path.GetExtension(safeName);

            for (int suffix = 1; suffix < 10000; suffix++)
            {
                var alternative = Path.Combine(folder, $"{baseName}_{suffix}{extension}");
                if (!File.Exists(alternative))
                {
                    return alternative;
                }
            }

            // Fallback: guid suffix
            return Path.Combine(folder, $"{baseName}_{Guid.NewGuid():N}{extension}");
        }

        private static RecoverySession Fail(RecoverySession session, string messageAr, string messageEn)
        {
            var errors = new List<string> { messageEn };
            return session with
            {
                Status = RecoverySessionStatus.Failed,
                FinishedAt = DateTimeOffset.UtcNow,
                RecoveredFilesCount = 0,
                Errors = errors
            };
        }
    }
}