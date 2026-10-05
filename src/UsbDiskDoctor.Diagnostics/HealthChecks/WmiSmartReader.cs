using System;
using System.Collections.Generic;
using System.Management;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Serilog;
using UsbDiskDoctor.Core.Models;

namespace UsbDiskDoctor.Diagnostics.HealthChecks
{
    /// <summary>
    /// Reads S.M.A.R.T. data using Windows Management Instrumentation (WMI).
    /// </summary>
    public sealed class WmiSmartReader : ISmartReader
    {
        private static readonly ILogger _log = Log.ForContext<WmiSmartReader>();

        public async Task<SmartInfo> ReadAsync(
            string physicalDriveDeviceId,
            CancellationToken cancellationToken = default)
        {
            try
            {
                return await Task.Run(() =>
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    return ReadSmartData(physicalDriveDeviceId);
                }, cancellationToken);
            }
            catch (OperationCanceledException)
            {
                _log.Information("SMART reading was cancelled for {DeviceId}.", physicalDriveDeviceId);
                return CreateUnavailableSmartInfo();
            }
            catch (ManagementException ex)
            {
                _log.Warning(ex, "WMI management exception during SMART reading for {DeviceId}. SMART may not be supported.", physicalDriveDeviceId);
                return CreateUnavailableSmartInfo();
            }
            catch (UnauthorizedAccessException ex)
            {
                _log.Error(ex, "Unauthorized access during SMART reading for {DeviceId}.", physicalDriveDeviceId);
                return CreateUnavailableSmartInfo();
            }
            catch (Exception ex)
            {
                _log.Error(ex, "Unexpected error during SMART reading for {DeviceId}.", physicalDriveDeviceId);
                return CreateUnavailableSmartInfo();
            }
        }

        private SmartInfo ReadSmartData(string physicalDriveDeviceId)
        {
            var scope = new ManagementScope(@"root\wmi");
            scope.Connect();

            var query = new ObjectQuery("SELECT * FROM MSStorageDriver_FailurePredictStatus");
            using var searcher = new ManagementObjectSearcher(scope, query);
            using var results = searcher.Get();

            if (results.Count == 0)
            {
                _log.Debug("No SMART data available for {DeviceId}.", physicalDriveDeviceId);
                return CreateUnavailableSmartInfo();
            }

            var driveNumberMatch = Regex.Match(physicalDriveDeviceId, @"PHYSICALDRIVE(\d+)", RegexOptions.IgnoreCase);
            var driveNumber = driveNumberMatch.Success ? driveNumberMatch.Groups[1].Value : null;

            ManagementObject? matchedResult = null;

            foreach (ManagementObject result in results)
            {
                var instanceNameInLoop = GetString(result, "InstanceName");

                if (driveNumber != null && instanceNameInLoop.Contains(driveNumber))
                {
                    matchedResult = result;
                    break;
                }
            }

            if (matchedResult == null)
            {
                foreach (ManagementObject result in results)
                {
                    matchedResult = result;
                    break;
                }
            }

            if (matchedResult == null)
            {
                return CreateUnavailableSmartInfo();
            }

            var active = GetBool(matchedResult, "Active");
            var predictFailure = GetBool(matchedResult, "PredictFailure");
            var reasonCode = GetUInt(matchedResult, "Reason");
            var instanceName = GetString(matchedResult, "InstanceName");

            var available = active;
            var reason = MapReason(reasonCode, predictFailure);

            var rawAttributes = new Dictionary<string, string>
            {
                ["InstanceName"] = instanceName,
                ["Active"] = active.ToString()
            };

            return new SmartInfo
            {
                Available = available,
                PredictFailure = predictFailure,
                Reason = reason,
                Temperature = null,
                RawAttributes = rawAttributes
            };
        }

        private static SmartInfo CreateUnavailableSmartInfo()
        {
            return new SmartInfo
            {
                Available = false,
                PredictFailure = false,
                Reason = null,
                Temperature = null,
                RawAttributes = new Dictionary<string, string>()
            };
        }

        private static string? MapReason(uint reason, bool predictFailure)
        {
            if (!predictFailure && reason == 0)
            {
                return null;
            }

            return reason switch
            {
                0x00 => "No failure predicted",
                0x05 => "General failure predicted",
                _ => $"Code 0x{reason:X2}"
            };
        }

        private static bool GetBool(ManagementBaseObject obj, string propertyName)
        {
            try
            {
                var value = obj[propertyName];
                if (value is bool boolValue)
                {
                    return boolValue;
                }
                if (value != null && bool.TryParse(value.ToString(), out var parsed))
                {
                    return parsed;
                }
                return false;
            }
            catch
            {
                return false;
            }
        }

        private static uint GetUInt(ManagementBaseObject obj, string propertyName)
        {
            try
            {
                var value = obj[propertyName];
                if (value is uint uintValue)
                {
                    return uintValue;
                }
                if (value != null && uint.TryParse(value.ToString(), out var parsed))
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
    }
}