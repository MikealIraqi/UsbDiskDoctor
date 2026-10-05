using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Serilog;
using UsbDiskDoctor.Recovery.Models;

namespace UsbDiskDoctor.Recovery.Scanning
{
    /// <summary>
    /// Scans mounted volumes for recoverable files using standard file system APIs.
    /// Strictly read-only — never writes, deletes, or modifies any files.
    /// </summary>
    public sealed class MountedVolumeScanner : IVolumeScanner
    {
        private static readonly ILogger _log = Log.ForContext<MountedVolumeScanner>();

        public async Task<IReadOnlyList<RecoveredFileInfo>> ScanAsync(
            string volumeRoot,
            RecoveryOptions options,
            IProgress<RecoveryProgressInfo>? progress,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(volumeRoot))
            {
                _log.Warning("Volume root path is empty.");
                return Array.Empty<RecoveredFileInfo>();
            }

            var normalizedRoot = volumeRoot.EndsWith('\\') ? volumeRoot : volumeRoot + '\\';

            if (!Directory.Exists(normalizedRoot))
            {
                _log.Warning("Volume root does not exist: {VolumeRoot}", normalizedRoot);
                return Array.Empty<RecoveredFileInfo>();
            }

            var effectiveOptions = options ?? new RecoveryOptions();

            var extensionsFilter = BuildExtensionsFilter(effectiveOptions.FileExtensionsToRecover);

            return await Task.Run(() =>
            {
                return ScanVolume(normalizedRoot, effectiveOptions, extensionsFilter, progress, cancellationToken);
            }, cancellationToken);
        }

        private IReadOnlyList<RecoveredFileInfo> ScanVolume(
            string volumeRoot,
            RecoveryOptions options,
            HashSet<string> extensionsFilter,
            IProgress<RecoveryProgressInfo>? progress,
            CancellationToken cancellationToken)
        {
            var results = new List<RecoveredFileInfo>();
            long bytesScanned = 0;
            int filesFound = 0;

            try
            {
                var enumerateOptions = new EnumerationOptions
                {
                    RecurseSubdirectories = true,
                    IgnoreInaccessible = true,
                    AttributesToSkip = FileAttributes.System | FileAttributes.ReparsePoint,
                    ReturnSpecialDirectories = false
                };

                foreach (var filePath in Directory.EnumerateFiles(volumeRoot, "*", enumerateOptions))
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    try
                    {
                        var fileInfo = new FileInfo(filePath);

                        var fileExtension = fileInfo.Extension;

                        if (extensionsFilter.Count > 0 && !extensionsFilter.Contains(fileExtension))
                        {
                            continue;
                        }

                        var recoveredFile = new RecoveredFileInfo
                        {
                            FileName = fileInfo.Name,
                            Extension = fileExtension ?? string.Empty,
                            SizeBytes = fileInfo.Length,
                            SourceOffset = 0,
                            TargetFullPath = string.Empty
                        };

                        results.Add(recoveredFile);

                        bytesScanned += fileInfo.Length;
                        filesFound++;

                        if (filesFound % 50 == 0 && progress != null)
                        {
                            var progressInfo = new RecoveryProgressInfo
                            {
                                BytesScanned = bytesScanned,
                                TotalBytes = 0,
                                FilesFound = filesFound,
                                PercentComplete = 0
                            };
                            progress.Report(progressInfo);
                        }
                    }
                    catch (UnauthorizedAccessException)
                    {
                        _log.Debug("Skipped inaccessible file: {Path}", filePath);
                    }
                    catch (FileNotFoundException)
                    {
                        _log.Debug("File vanished: {Path}", filePath);
                    }
                    catch (PathTooLongException)
                    {
                        _log.Debug("Path too long: {Path}", filePath);
                    }
                    catch (IOException ioException)
                    {
                        _log.Debug(ioException, "IO error on: {Path}", filePath);
                    }
                }

                if (progress != null)
                {
                    var finalProgress = new RecoveryProgressInfo
                    {
                        BytesScanned = bytesScanned,
                        TotalBytes = bytesScanned,
                        FilesFound = filesFound,
                        PercentComplete = 100
                    };
                    progress.Report(finalProgress);
                }

                _log.Information("Scan completed: {FilesFound} files in {Bytes} bytes.", filesFound, bytesScanned);
                return results;
            }
            catch (OperationCanceledException)
            {
                _log.Information("Scan cancelled for {VolumeRoot}.", volumeRoot);
                throw;
            }
            catch (Exception genericException)
            {
                _log.Error(genericException, "Scan failed for {VolumeRoot}.", volumeRoot);
                return Array.Empty<RecoveredFileInfo>();
            }
        }

        private static HashSet<string> BuildExtensionsFilter(IReadOnlyList<string>? extensions)
        {
            var filterSet = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            if (extensions == null || extensions.Count == 0)
            {
                return filterSet;
            }

            foreach (var rawExtension in extensions)
            {
                if (string.IsNullOrWhiteSpace(rawExtension))
                {
                    continue;
                }

                var normalizedExtension = rawExtension.Trim();
                if (!normalizedExtension.StartsWith('.'))
                {
                    normalizedExtension = "." + normalizedExtension;
                }

                filterSet.Add(normalizedExtension);
            }

            return filterSet;
        }
    }
}