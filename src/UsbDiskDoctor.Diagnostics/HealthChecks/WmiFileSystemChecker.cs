using System;
using System.Management;
using System.Threading;
using System.Threading.Tasks;
using Serilog;
using UsbDiskDoctor.Core.Enums;
using UsbDiskDoctor.Diagnostics.Models;

namespace UsbDiskDoctor.Diagnostics.HealthChecks
{
    /// <summary>
    /// Checks file system health using Windows Management Instrumentation (WMI).
    /// </summary>
    public sealed class WmiFileSystemChecker : IFileSystemChecker
    {
        private static readonly ILogger _log = Log.ForContext<WmiFileSystemChecker>();

        public async Task<FileSystemCheckResult> CheckAsync(
            string driveLetter,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(driveLetter))
            {
                return new FileSystemCheckResult
                {
                    CheckSucceeded = false,
                    ErrorMessage = "Drive letter is empty."
                };
            }

            try
            {
                return await Task.Run(() =>
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    return CheckFileSystem(driveLetter);
                }, cancellationToken);
            }
            catch (OperationCanceledException)
            {
                _log.Information("File system check was cancelled for {DriveLetter}.", driveLetter);
                return new FileSystemCheckResult
                {
                    DriveLetter = driveLetter,
                    CheckSucceeded = false,
                    ErrorMessage = "Cancelled."
                };
            }
            catch (ManagementException mgmtEx)
            {
                _log.Warning(mgmtEx, "WMI management exception during file system check for {DriveLetter}.", driveLetter);
                return new FileSystemCheckResult
                {
                    DriveLetter = driveLetter,
                    CheckSucceeded = false,
                    ErrorMessage = mgmtEx.Message
                };
            }
            catch (UnauthorizedAccessException authEx)
            {
                _log.Error(authEx, "Unauthorized access during file system check for {DriveLetter}.", driveLetter);
                return new FileSystemCheckResult
                {
                    DriveLetter = driveLetter,
                    CheckSucceeded = false,
                    ErrorMessage = authEx.Message
                };
            }
            catch (Exception genericEx)
            {
                _log.Error(genericEx, "Unexpected error during file system check for {DriveLetter}.", driveLetter);
                return new FileSystemCheckResult
                {
                    DriveLetter = driveLetter,
                    CheckSucceeded = false,
                    ErrorMessage = genericEx.Message
                };
            }
        }

        private FileSystemCheckResult CheckFileSystem(string driveLetter)
        {
            var escapedDriveLetter = driveLetter.Replace("'", "''");
            var query = $"SELECT * FROM Win32_Volume WHERE DriveLetter='{escapedDriveLetter}'";

            using var searcher = new ManagementObjectSearcher(query);
            using var results = searcher.Get();

            if (results.Count == 0)
            {
                return new FileSystemCheckResult
                {
                    DriveLetter = driveLetter,
                    CheckSucceeded = false,
                    ErrorMessage = "Volume not found in WMI."
                };
            }

            ManagementObject? volumeObject = null;
            foreach (ManagementObject obj in results)
            {
                volumeObject = obj;
                break;
            }

            if (volumeObject == null)
            {
                return new FileSystemCheckResult
                {
                    DriveLetter = driveLetter,
                    CheckSucceeded = false,
                    ErrorMessage = "Failed to retrieve volume object."
                };
            }

            var retrievedDriveLetter = GetString(volumeObject, "DriveLetter");
            var fileSystemString = GetString(volumeObject, "FileSystem");
            var volumeLabel = GetString(volumeObject, "Label");
            var capacityBytes = GetLong(volumeObject, "Capacity");
            var freeSpaceBytes = GetLong(volumeObject, "FreeSpace");
            var automount = GetBool(volumeObject, "Automount");
            var dirtyBitSet = GetBool(volumeObject, "DirtyBitSet");

            var fileSystemType = MapFileSystem(fileSystemString);
            var isRaw = string.IsNullOrWhiteSpace(fileSystemString) || 
                       string.Equals(fileSystemString, "RAW", StringComparison.OrdinalIgnoreCase);
            var isMounted = automount && (capacityBytes > 0);

            return new FileSystemCheckResult
            {
                DriveLetter = string.IsNullOrWhiteSpace(retrievedDriveLetter) ? driveLetter : retrievedDriveLetter,
                FileSystem = fileSystemType,
                Label = volumeLabel,
                CapacityBytes = capacityBytes,
                FreeSpaceBytes = freeSpaceBytes,
                IsMounted = isMounted,
                IsRaw = isRaw,
                DirtyBitSet = dirtyBitSet,
                CheckSucceeded = true,
                ErrorMessage = null,
                Notes = Array.Empty<string>()
            };
        }

        private static string GetString(ManagementBaseObject wmiObject, string propertyName)
        {
            try
            {
                var propertyValue = wmiObject[propertyName];
                return propertyValue?.ToString() ?? string.Empty;
            }
            catch
            {
                return string.Empty;
            }
        }

        private static long GetLong(ManagementBaseObject wmiObject, string propertyName)
        {
            try
            {
                var propertyValue = wmiObject[propertyName];
                if (propertyValue is ulong ulongValue)
                {
                    return (long)ulongValue;
                }
                if (propertyValue != null && long.TryParse(propertyValue.ToString(), out var parsedValue))
                {
                    return parsedValue;
                }
                return 0;
            }
            catch
            {
                return 0;
            }
        }

        private static bool GetBool(ManagementBaseObject wmiObject, string propertyName)
        {
            try
            {
                var propertyValue = wmiObject[propertyName];
                if (propertyValue is bool boolValue)
                {
                    return boolValue;
                }
                if (propertyValue != null && bool.TryParse(propertyValue.ToString(), out var parsedValue))
                {
                    return parsedValue;
                }
                return false;
            }
            catch
            {
                return false;
            }
        }

        private static FileSystemType MapFileSystem(string? fileSystemString)
        {
            if (string.IsNullOrWhiteSpace(fileSystemString))
            {
                return FileSystemType.Unknown;
            }

            return fileSystemString.ToUpperInvariant() switch
            {
                "NTFS" => FileSystemType.NTFS,
                "FAT16" => FileSystemType.FAT16,
                "FAT32" => FileSystemType.FAT32,
                "EXFAT" => FileSystemType.exFAT,
                "REFS" => FileSystemType.ReFS,
                "RAW" => FileSystemType.RAW,
                "CDFS" => FileSystemType.CDFS,
                "UDF" => FileSystemType.UDF,
                "EXT4" => FileSystemType.EXT4,
                "APFS" => FileSystemType.APFS,
                _ => FileSystemType.Unknown
            };
        }
    }
}