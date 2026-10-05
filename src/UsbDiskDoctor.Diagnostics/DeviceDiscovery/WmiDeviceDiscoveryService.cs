using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;
using System.Threading;
using System.Threading.Tasks;
using Serilog;
using UsbDiskDoctor.Core.Enums;
using UsbDiskDoctor.Core.Models;
using UsbDiskDoctor.Diagnostics.Models;
using UsbDiskDoctor.Diagnostics.VolumeReading;

namespace UsbDiskDoctor.Diagnostics.DeviceDiscovery
{
    /// <summary>
    /// Discovers USB storage devices using Windows Management Instrumentation (WMI).
    /// Supports both direct USB and USB Attached SCSI (UASP) devices.
    /// </summary>
    public sealed class WmiDeviceDiscoveryService : IDeviceDiscoveryService
    {
        private static readonly ILogger _log = Log.ForContext<WmiDeviceDiscoveryService>();
        private readonly IVolumeReader _volumeReader;

        public WmiDeviceDiscoveryService(IVolumeReader volumeReader)
        {
            _volumeReader = volumeReader ?? throw new ArgumentNullException(nameof(volumeReader));
        }

        public async Task<IReadOnlyList<DeviceSummary>> DiscoverAsync(
            DiscoveryOptions? options = null,
            CancellationToken cancellationToken = default)
        {
            options ??= new DiscoveryOptions();

            try
            {
                return await Task.Run(async () =>
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    return await DiscoverDevicesAsync(options, cancellationToken);
                }, cancellationToken);
            }
            catch (OperationCanceledException)
            {
                _log.Information("Device discovery was cancelled.");
                return Array.Empty<DeviceSummary>();
            }
            catch (ManagementException ex)
            {
                _log.Error(ex, "WMI management exception during device discovery.");
                return Array.Empty<DeviceSummary>();
            }
            catch (UnauthorizedAccessException ex)
            {
                _log.Error(ex, "Unauthorized access during device discovery.");
                return Array.Empty<DeviceSummary>();
            }
            catch (Exception ex)
            {
                _log.Error(ex, "Unexpected error during device discovery.");
                return Array.Empty<DeviceSummary>();
            }
        }

        private async Task<IReadOnlyList<DeviceSummary>> DiscoverDevicesAsync(
            DiscoveryOptions options,
            CancellationToken cancellationToken)
        {
            var query = "SELECT * FROM Win32_DiskDrive";

            var devices = new List<DeviceSummary>();

            using var searcher = new ManagementObjectSearcher(query);
            using var results = searcher.Get();

            foreach (ManagementObject disk in results)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var status = GetString(disk, "Status");
                var interfaceType = GetString(disk, "InterfaceType");
                var mediaType = GetString(disk, "MediaType");

                if (options.UsbOnly && !IsUsbLikeDevice(interfaceType, mediaType))
                {
                    continue;
                }

                if (!options.IncludeOfflineDevices)
                {
                    if (string.Equals(status, "Unknown", StringComparison.OrdinalIgnoreCase) ||
                        string.Equals(status, "Offline", StringComparison.OrdinalIgnoreCase) ||
                        string.Equals(status, "Stopped", StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }
                }

                var deviceId = GetString(disk, "DeviceID");
                var caption = GetString(disk, "Caption");
                var model = GetString(disk, "Model");
                var friendlyName = string.IsNullOrWhiteSpace(caption) ? model : caption;

                var serialNumber = GetString(disk, "SerialNumber");
                if (string.IsNullOrWhiteSpace(serialNumber))
                {
                    serialNumber = null;
                }
                else
                {
                    serialNumber = serialNumber.Trim();
                }

                var sizeBytes = GetSizeBytes(disk, "Size");
                var operationalStatus = MapOperationalStatus(status);

                var busType = DetermineBusType(interfaceType, mediaType);

                var summary = new DeviceSummary
                {
                    DeviceId = deviceId,
                    FriendlyName = friendlyName,
                    Model = model,
                    SerialNumber = serialNumber,
                    BusType = busType,
                    MediaType = MediaType.Unknown,
                    PartitionStyle = PartitionStyle.Unknown,
                    SizeBytes = sizeBytes,
                    HealthStatus = HealthStatus.Unknown,
                    OperationalStatus = operationalStatus,
                    IsUsb = (busType == BusType.USB),
                    IsExternal = (busType == BusType.USB),
                    Volumes = Array.Empty<VolumeInfo>()
                };

                devices.Add(summary);
            }

            var devicesWithVolumes = new List<DeviceSummary>();
            foreach (var device in devices)
            {
                cancellationToken.ThrowIfCancellationRequested();

                try
                {
                    var volumes = await _volumeReader.ReadVolumesAsync(device.DeviceId, cancellationToken);
                    var deviceWithVolumes = device with { Volumes = volumes };
                    devicesWithVolumes.Add(deviceWithVolumes);
                }
                catch (Exception ex)
                {
                    _log.Warning(ex, "Failed to read volumes for device {DeviceId}. Continuing with empty volumes.", device.DeviceId);
                    devicesWithVolumes.Add(device);
                }
            }

            return devicesWithVolumes.OrderBy(d => d.DeviceId, StringComparer.Ordinal).ToList();
        }

        private static bool IsUsbLikeDevice(string interfaceType, string mediaType)
        {
            if (string.Equals(interfaceType, "USB", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            if (string.Equals(interfaceType, "SCSI", StringComparison.OrdinalIgnoreCase)
                && mediaType.IndexOf("External", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return true;
            }

            return false;
        }

        private static BusType DetermineBusType(string interfaceType, string mediaType)
        {
            if (string.Equals(interfaceType, "USB", StringComparison.OrdinalIgnoreCase))
            {
                return BusType.USB;
            }

            if (string.Equals(interfaceType, "SCSI", StringComparison.OrdinalIgnoreCase)
                && mediaType.IndexOf("External", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return BusType.USB;
            }

            if (string.Equals(interfaceType, "SATA", StringComparison.OrdinalIgnoreCase))
            {
                return BusType.SATA;
            }

            if (string.Equals(interfaceType, "NVMe", StringComparison.OrdinalIgnoreCase))
            {
                return BusType.NVMe;
            }

            return BusType.Unknown;
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

        private static long GetSizeBytes(ManagementBaseObject obj, string propertyName)
        {
            try
            {
                var value = obj[propertyName];
                if (value is ulong ulongValue)
                {
                    return (long)ulongValue;
                }
                if (value != null && ulong.TryParse(value.ToString(), out var parsed))
                {
                    return (long)parsed;
                }
                return 0;
            }
            catch
            {
                return 0;
            }
        }

        private static OperationalStatus MapOperationalStatus(string? status)
        {
            if (string.IsNullOrWhiteSpace(status))
            {
                return OperationalStatus.Unknown;
            }

            return status.ToUpperInvariant() switch
            {
                "OK" => OperationalStatus.OK,
                "DEGRADED" => OperationalStatus.Degraded,
                "ERROR" => OperationalStatus.Error,
                _ => OperationalStatus.Unknown
            };
        }
    }
}