using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Serilog;
using UsbDiskDoctor.Core.Enums;
using UsbDiskDoctor.Core.Models;

namespace UsbDiskDoctor.Diagnostics.VolumeReading
{
    /// <summary>
    /// Reads volume information from physical storage devices using Windows Management Instrumentation (WMI).
    /// </summary>
    public sealed class WmiVolumeReader : IVolumeReader
    {
        private static readonly ILogger _log = Log.ForContext<WmiVolumeReader>();

        public async Task<IReadOnlyList<VolumeInfo>> ReadVolumesAsync(
            string physicalDriveDeviceId,
            CancellationToken cancellationToken = default)
        {
            try
            {
                return await Task.Run(() =>
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    return ReadVolumes(physicalDriveDeviceId);
                }, cancellationToken);
            }
            catch (OperationCanceledException)
            {
                _log.Information("Volume reading was cancelled.");
                return Array.Empty<VolumeInfo>();
            }
            catch (ManagementException ex)
            {
                _log.Error(ex, "WMI management exception during volume reading for {DeviceId}.", physicalDriveDeviceId);
                return Array.Empty<VolumeInfo>();
            }
            catch (UnauthorizedAccessException ex)
            {
                _log.Error(ex, "Unauthorized access during volume reading for {DeviceId}.", physicalDriveDeviceId);
                return Array.Empty<VolumeInfo>();
            }
            catch (Exception ex)
            {
                _log.Error(ex, "Unexpected error during volume reading for {DeviceId}.", physicalDriveDeviceId);
                return Array.Empty<VolumeInfo>();
            }
        }

        private IReadOnlyList<VolumeInfo> ReadVolumes(string physicalDriveDeviceId)
        {
            var volumes = new List<VolumeInfo>();

            // Escape backslashes for WMI query
            var escapedDeviceId = physicalDriveDeviceId.Replace("\\", "\\\\");

            // Query disk-to-partition associations
            var associationQuery = $"SELECT * FROM Win32_DiskDriveToDiskPartition WHERE Antecedent=\"Win32_DiskDrive.DeviceID='{escapedDeviceId}'\"";

            using var searcher = new ManagementObjectSearcher(associationQuery);
            using var associations = searcher.Get();

            foreach (ManagementObject association in associations)
            {
                var dependent = GetString(association, "Dependent");
                if (string.IsNullOrWhiteSpace(dependent))
                {
                    continue;
                }

                var partitionDeviceId = ExtractDeviceId(dependent);
                if (string.IsNullOrWhiteSpace(partitionDeviceId))
                {
                    continue;
                }

                var escapedPartitionId = partitionDeviceId.Replace("\\", "\\\\");
                var logicalDiskAssocQuery = $"SELECT * FROM Win32_LogicalDiskToPartition WHERE Antecedent=\"Win32_DiskPartition.DeviceID='{escapedPartitionId}'\"";

                using var logicalDiskAssocSearcher = new ManagementObjectSearcher(logicalDiskAssocQuery);
                using var logicalDiskAssociations = logicalDiskAssocSearcher.Get();

                foreach (ManagementObject logicalDiskAssoc in logicalDiskAssociations)
                {
                    var logicalDiskDependent = GetString(logicalDiskAssoc, "Dependent");
                    if (string.IsNullOrWhiteSpace(logicalDiskDependent))
                    {
                        continue;
                    }

                    var logicalDiskId = ExtractDeviceId(logicalDiskDependent);
                    if (string.IsNullOrWhiteSpace(logicalDiskId))
                    {
                        continue;
                    }

                    var logicalDiskQuery = $"SELECT * FROM Win32_LogicalDisk WHERE DeviceID='{logicalDiskId}'";
                    using var logicalDiskSearcher = new ManagementObjectSearcher(logicalDiskQuery);
                    using var logicalDisks = logicalDiskSearcher.Get();

                    foreach (ManagementObject logicalDisk in logicalDisks)
                    {
                        var driveLetter = GetString(logicalDisk, "DeviceID");
                        var volumeName = GetString(logicalDisk, "VolumeName");
                        var fileSystem = GetString(logicalDisk, "FileSystem");
                        var sizeBytes = GetLong(logicalDisk, "Size");
                        var freeSpaceBytes = GetLong(logicalDisk, "FreeSpace");

                        var fileSystemType = MapFileSystemType(fileSystem);
                        var isMounted = !string.IsNullOrWhiteSpace(driveLetter) && sizeBytes > 0;
                        var isRaw = string.Equals(fileSystem, "RAW", StringComparison.OrdinalIgnoreCase) ||
                                   string.IsNullOrWhiteSpace(fileSystem);

                        var volume = new VolumeInfo
                        {
                            DriveLetter = string.IsNullOrWhiteSpace(driveLetter) ? null : driveLetter,
                            Label = string.IsNullOrWhiteSpace(volumeName) ? string.Empty : volumeName,
                            FileSystem = fileSystemType,
                            SizeBytes = sizeBytes,
                            FreeSpaceBytes = freeSpaceBytes,
                            HealthStatus = HealthStatus.Unknown,
                            IsRaw = isRaw,
                            IsMounted = isMounted
                        };

                        volumes.Add(volume);
                    }
                }
            }

            return volumes;
        }

        private static string GetString(ManagementBaseObject obj, string propertyName)
        {
            try
            {
                var value = obj[propertyName];
                return value?.ToString() ?? string.Empty;
            }
            catch
            {
                return string.Empty;
            }
        }

        private static long GetLong(ManagementBaseObject obj, string propertyName)
        {
            try
            {
                var value = obj[propertyName];
                if (value is ulong ulongValue)
                {
                    return (long)ulongValue;
                }
                if (value != null && long.TryParse(value.ToString(), out var parsed))
                {
                    return parsed;
                }
                return 0;
            }
            catch
            {
                return 0;
            }
        }

        private static FileSystemType MapFileSystemType(string? fileSystem)
        {
            if (string.IsNullOrWhiteSpace(fileSystem))
            {
                return FileSystemType.Unknown;
            }

            return fileSystem.ToUpperInvariant() switch
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

        /// <summary>
        /// Extracts the value of DeviceID from a WMI object path string.
        /// Example: 'Win32_DiskPartition.DeviceID="Disk #2, Partition #0"' returns 'Disk #2, Partition #0'.
        /// </summary>
        private static string ExtractDeviceId(string wmiPath)
        {
            var match = Regex.Match(wmiPath, @"DeviceID=""([^""]+)""");
            return match.Success ? match.Groups[1].Value : string.Empty;
        }
    }
}