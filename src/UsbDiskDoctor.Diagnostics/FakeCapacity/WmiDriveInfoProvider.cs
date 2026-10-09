using System;
using System.Collections.Generic;
using System.IO;
using System.Management;
using UsbDiskDoctor.Recovery.FakeCapacity;

namespace UsbDiskDoctor.Diagnostics.FakeCapacity;

public sealed class WmiDriveInfoProvider : IDriveInfoProvider
{
    private readonly Dictionary<char, DriveInfoEx> _cache;

    public WmiDriveInfoProvider()
    {
        _cache = new Dictionary<char, DriveInfoEx>();
        Refresh();
    }

    public void Refresh()
    {
        _cache.Clear();

        var drives = DriveInfo.GetDrives();
        foreach (var drive in drives)
        {
            if (!drive.IsReady)
            {
                continue;
            }

            var letter = char.ToUpperInvariant(drive.Name[0]);
            var busType = GetBusTypeForDrive(letter);

            if (busType is null)
            {
                continue;
            }

            if (!string.Equals(busType, "USB", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            long totalSize = 0;
            long freeSpace = 0;
            string volumeLabel = "";
            string format = "";

            try { totalSize = drive.TotalSize; } catch { }
            try { freeSpace = drive.TotalFreeSpace; } catch { }
            try { volumeLabel = drive.VolumeLabel ?? ""; } catch { }
            try { format = drive.DriveFormat ?? ""; } catch { }

            var driveInfo = new DriveInfoEx
            {
                DriveLetter = letter,
                VolumeLabel = volumeLabel,
                TotalSizeBytes = totalSize,
                FreeSpaceBytes = freeSpace,
                DriveType = drive.DriveType,
                DriveFormat = format,
                BusType = busType
            };

            _cache[letter] = driveInfo;
        }
    }

    public DriveInfoEx? GetDrive(char driveLetter)
    {
        var key = char.ToUpperInvariant(driveLetter);
        return _cache.TryGetValue(key, out var drive) ? drive : null;
    }

    public IEnumerable<DriveInfoEx> GetAllDrives() => _cache.Values;

    private static string? GetBusTypeForDrive(char driveLetter)
    {
        try
        {
            var scope = new ManagementScope(@"\\.\root\cimv2");
            scope.Connect();

            // Step 1: LogicalDisk -> Partition
            var query1 =
                $@"SELECT Antecedent FROM Win32_LogicalDiskToPartition WHERE Dependent = 'Win32_LogicalDisk.DeviceID=""{driveLetter}:""'";

            using var searcher1 = new ManagementObjectSearcher(scope, new ObjectQuery(query1));

            string? partitionId = null;
            foreach (ManagementObject obj in searcher1.Get())
            {
                if (obj["Antecedent"] is ManagementBaseObject antecedent)
                {
                    partitionId = antecedent["DeviceID"] as string;
                    break;
                }
            }

            if (partitionId is null)
            {
                return null;
            }

            // Step 2: Partition -> DiskDrive
            var query2 =
                $@"SELECT Antecedent FROM Win32_DiskDriveToDiskPartition WHERE Dependent = 'Win32_DiskPartition.DeviceID=""{partitionId}""'";

            using var searcher2 = new ManagementObjectSearcher(scope, new ObjectQuery(query2));

            string? diskId = null;
            foreach (ManagementObject obj in searcher2.Get())
            {
                if (obj["Antecedent"] is ManagementBaseObject antecedent)
                {
                    diskId = antecedent["DeviceID"] as string;
                    break;
                }
            }

            if (diskId is null)
            {
                return null;
            }

            // Step 3: DiskDrive -> InterfaceType
            var query3 = $"SELECT InterfaceType FROM Win32_DiskDrive WHERE DeviceID = '{diskId}'";

            using var searcher3 = new ManagementObjectSearcher(scope, new ObjectQuery(query3));

            foreach (ManagementObject obj in searcher3.Get())
            {
                return obj["InterfaceType"] as string;
            }

            return null;
        }
        catch
        {
            return null;
        }
    }
}