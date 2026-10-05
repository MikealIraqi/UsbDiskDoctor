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

namespace UsbDiskDoctor.Diagnostics.DeviceDiscovery
{
    /// <summary>
    /// Discovers USB storage devices using Windows Management Instrumentation (WMI).
    /// </summary>
    public sealed class WmiDeviceDiscoveryService : IDeviceDiscoveryService
    {
        private static readonly ILogger _log = Log.ForContext<WmiDeviceDiscoveryService>();

        public async Task<IReadOnlyList<DeviceSummary>> DiscoverAsync(
            DiscoveryOptions? options = null,
            CancellationToken cancellationToken = default)
        {
            options ??= new DiscoveryOptions();

            try
            {
                return await Task.Run(() =>
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    return DiscoverDevices(options);
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

        private IReadOnlyList<DeviceSummary> DiscoverDevices(DiscoveryOptions options)
        {
            var query = options.UsbOnly
                ? "SELECT * FROM Win32_DiskDrive WHERE InterfaceType='USB'"
                : "SELECT * FROM Win32_DiskDrive";

            var devices = new List<DeviceSummary>();

            using var searcher = new ManagementObjectSearcher(query);
            using var results = searcher.Get();

            foreach (ManagementObject disk in results)
            {
                var status = GetString(disk, "Status");

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

                var summary = new DeviceSummary
                {
                    DeviceId = deviceId,
                    FriendlyName = friendlyName,
                    Model = model,
                    SerialNumber = serialNumber,
                    BusType = BusType.USB,
                    MediaType = MediaType.Unknown,
                    PartitionStyle = PartitionStyle.Unknown,
                    SizeBytes = sizeBytes,
                    HealthStatus = HealthStatus.Unknown,
                    OperationalStatus = operationalStatus,
                    IsUsb = true,
                    IsExternal = true,
                    Volumes = Array.Empty<VolumeInfo>()
                };

                devices.Add(summary);
            }

            return devices.OrderBy(d => d.DeviceId, StringComparer.Ordinal).ToList();
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