using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Serilog;
using UsbDiskDoctor.Core.Models;
using UsbDiskDoctor.Diagnostics.HealthChecks;
using UsbDiskDoctor.Diagnostics.Models;

namespace UsbDiskDoctor.Diagnostics.DiagnosticEngine
{
    /// <summary>
    /// Orchestrates diagnostic checks on storage devices by coordinating
    /// SMART reading, file system checks, and health evaluation.
    /// </summary>
    public sealed class DiagnosticEngine : IDiagnosticEngine
    {
        private readonly ISmartReader _smartReader;
        private readonly IFileSystemChecker _fileSystemChecker;
        private readonly IHealthEvaluator _healthEvaluator;
        private static readonly ILogger _log = Log.ForContext<DiagnosticEngine>();

        public DiagnosticEngine(
            ISmartReader smartReader,
            IFileSystemChecker fileSystemChecker,
            IHealthEvaluator healthEvaluator)
        {
            _smartReader = smartReader ?? throw new ArgumentNullException(nameof(smartReader));
            _fileSystemChecker = fileSystemChecker ?? throw new ArgumentNullException(nameof(fileSystemChecker));
            _healthEvaluator = healthEvaluator ?? throw new ArgumentNullException(nameof(healthEvaluator));
        }

        public async Task<DeviceDiagnosticReport> DiagnoseAsync(
            DeviceSummary device,
            CancellationToken cancellationToken = default)
        {
            if (device == null)
            {
                throw new ArgumentNullException(nameof(device));
            }

            _log.Information("Starting diagnosis for {DeviceId}.", device.DeviceId);

            // Step 1: Read SMART data
            SmartInfo smartInfoResult;
            try
            {
                smartInfoResult = await _smartReader.ReadAsync(device.DeviceId, cancellationToken);
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception smartException)
            {
                _log.Warning(smartException, "Failed to read SMART data for {DeviceId}. Using default.", device.DeviceId);
                smartInfoResult = new SmartInfo { Available = false, PredictFailure = false };
            }

            // Step 2: Check file systems for each mounted volume
            var fileSystemResultsList = new List<FileSystemCheckResult>();
            foreach (var volumeItem in device.Volumes)
            {
                if (volumeItem == null || !volumeItem.IsMounted)
                {
                    continue;
                }

                var driveLetterForCheck = volumeItem.DriveLetter;
                if (string.IsNullOrEmpty(driveLetterForCheck))
                {
                    continue;
                }

                try
                {
                    var fileSystemCheckResult = await _fileSystemChecker.CheckAsync(driveLetterForCheck, cancellationToken);
                    fileSystemResultsList.Add(fileSystemCheckResult);
                }
                catch (OperationCanceledException)
                {
                    throw;
                }
                catch (Exception fileSystemException)
                {
                    _log.Warning(fileSystemException, "Failed to check file system for {DriveLetter} on {DeviceId}.", driveLetterForCheck, device.DeviceId);
                    var failedResult = new FileSystemCheckResult
                    {
                        DriveLetter = driveLetterForCheck,
                        CheckSucceeded = false,
                        ErrorMessage = fileSystemException.Message
                    };
                    fileSystemResultsList.Add(failedResult);
                }
            }

            var fileSystemResultsArray = fileSystemResultsList.ToArray();

            // Step 3: Evaluate health
            HealthEvaluationResult healthEvaluationResult;
            try
            {
                healthEvaluationResult = _healthEvaluator.Evaluate(device, smartInfoResult, fileSystemResultsArray);
            }
            catch (Exception evaluationException)
            {
                _log.Error(evaluationException, "Failed to evaluate health for {DeviceId}. Using default.", device.DeviceId);
                healthEvaluationResult = new HealthEvaluationResult();
            }

            _log.Information("Diagnosis completed for {DeviceId}. Overall status: {OverallStatus}.", device.DeviceId, healthEvaluationResult.OverallStatus);

            return new DeviceDiagnosticReport
            {
                Device = device,
                SmartInfo = smartInfoResult,
                FileSystemResults = fileSystemResultsArray,
                HealthEvaluation = healthEvaluationResult,
                Timestamp = DateTimeOffset.UtcNow
            };
        }
    }
}