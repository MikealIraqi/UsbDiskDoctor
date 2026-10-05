using System;
using System.Collections.Generic;
using UsbDiskDoctor.Core.Enums;
using UsbDiskDoctor.Core.Models;
using UsbDiskDoctor.Diagnostics.HealthChecks;
using UsbDiskDoctor.Diagnostics.Models;

namespace UsbDiskDoctor.Diagnostics.Tests.HealthChecks
{
    /// <summary>
    /// Unit tests for <see cref="HealthEvaluator"/>.
    /// All tests are pure — no WMI, no I/O.
    /// </summary>
    public class HealthEvaluatorTests
    {
        private static DeviceSummary MakeDevice(
            string id = @"\\.\PHYSICALDRIVE1",
            OperationalStatus opStatus = OperationalStatus.OK) =>
            new DeviceSummary { DeviceId = id, OperationalStatus = opStatus };

        [Fact]
        public void Evaluate_ThrowsArgumentNullException_OnNullDevice()
        {
            var sut = new HealthEvaluator();

            Assert.Throws<ArgumentNullException>(() =>
                sut.Evaluate(null!, new SmartInfo(), Array.Empty<FileSystemCheckResult>()));
        }

        [Fact]
        public void Evaluate_HealthyDevice_ReturnsHealthyWithNoDiagnostics()
        {
            var sut = new HealthEvaluator();
            var device = MakeDevice(opStatus: OperationalStatus.OK);
            var smart = new SmartInfo { Available = false, PredictFailure = false };
            var fsResults = new[]
            {
                new FileSystemCheckResult { DriveLetter = "E:", CheckSucceeded = true, IsRaw = false, DirtyBitSet = false }
            };

            var result = sut.Evaluate(device, smart, fsResults);

            Assert.Equal(HealthStatus.Healthy, result.OverallStatus);
            Assert.Empty(result.Diagnostics);
            Assert.NotEmpty(result.SummaryAr);
            Assert.NotEmpty(result.SummaryEn);
        }

        [Fact]
        public void Evaluate_SmartPredictFailure_ReturnsCriticalWithSmartDiagnostic()
        {
            var sut = new HealthEvaluator();
            var device = MakeDevice();
            var smart = new SmartInfo { Available = true, PredictFailure = true, Reason = "Code 0x05" };

            var result = sut.Evaluate(device, smart, Array.Empty<FileSystemCheckResult>());

            Assert.Equal(HealthStatus.Critical, result.OverallStatus);
            Assert.Single(result.Diagnostics);
            Assert.Equal("SMART_PREDICT_FAILURE", result.Diagnostics[0].Code);
            Assert.Equal(Severity.Critical, result.Diagnostics[0].Severity);
        }

        [Fact]
        public void Evaluate_OperationalStatusError_ReturnsCritical()
        {
            var sut = new HealthEvaluator();
            var device = MakeDevice(opStatus: OperationalStatus.Error);

            var result = sut.Evaluate(device, new SmartInfo(), Array.Empty<FileSystemCheckResult>());

            Assert.Equal(HealthStatus.Critical, result.OverallStatus);
            Assert.Contains(result.Diagnostics, d => d.Code == "OPERATIONAL_STATUS_ERROR");
        }

        [Fact]
        public void Evaluate_OperationalStatusDegraded_ReturnsWarning()
        {
            var sut = new HealthEvaluator();
            var device = MakeDevice(opStatus: OperationalStatus.Degraded);

            var result = sut.Evaluate(device, new SmartInfo(), Array.Empty<FileSystemCheckResult>());

            Assert.Equal(HealthStatus.Warning, result.OverallStatus);
            Assert.Contains(result.Diagnostics, d => d.Code == "OPERATIONAL_STATUS_DEGRADED");
        }

        [Fact]
        public void Evaluate_RawFileSystem_ReturnsCritical()
        {
            var sut = new HealthEvaluator();
            var device = MakeDevice();
            var fsResults = new[]
            {
                new FileSystemCheckResult { DriveLetter = "E:", IsRaw = true, CheckSucceeded = true }
            };

            var result = sut.Evaluate(device, new SmartInfo(), fsResults);

            Assert.Equal(HealthStatus.Critical, result.OverallStatus);
            Assert.Contains(result.Diagnostics, d => d.Code == "RAW_FILESYSTEM");
        }

        [Fact]
        public void Evaluate_DirtyBitSet_ReturnsWarning()
        {
            var sut = new HealthEvaluator();
            var device = MakeDevice();
            var fsResults = new[]
            {
                new FileSystemCheckResult { DriveLetter = "E:", CheckSucceeded = true, DirtyBitSet = true }
            };

            var result = sut.Evaluate(device, new SmartInfo(), fsResults);

            Assert.Equal(HealthStatus.Warning, result.OverallStatus);
            Assert.Contains(result.Diagnostics, d => d.Code == "DIRTY_BIT_SET");
        }

        [Fact]
        public void Evaluate_FileSystemCheckFailed_ReturnsWarning()
        {
            var sut = new HealthEvaluator();
            var device = MakeDevice();
            var fsResults = new[]
            {
                new FileSystemCheckResult { DriveLetter = "E:", CheckSucceeded = false, ErrorMessage = "Volume not found" }
            };

            var result = sut.Evaluate(device, new SmartInfo(), fsResults);

            Assert.Equal(HealthStatus.Warning, result.OverallStatus);
            Assert.Contains(result.Diagnostics, d => d.Code == "FILESYSTEM_CHECK_FAILED");
        }

        [Fact]
        public void Evaluate_MultipleCriticalSources_ReturnsAllThreeDiagnostics()
        {
            var sut = new HealthEvaluator();
            var device = MakeDevice(opStatus: OperationalStatus.Error);
            var smart = new SmartInfo { Available = true, PredictFailure = true };
            var fsResults = new[]
            {
                new FileSystemCheckResult { DriveLetter = "E:", IsRaw = true, CheckSucceeded = true }
            };

            var result = sut.Evaluate(device, smart, fsResults);

            Assert.Equal(HealthStatus.Critical, result.OverallStatus);
            Assert.Equal(3, result.Diagnostics.Count);
        }

        [Fact]
        public void Evaluate_OperationalStatusUnknown_NoOtherIssues_ReturnsUnknown()
        {
            var sut = new HealthEvaluator();
            var device = MakeDevice(opStatus: OperationalStatus.Unknown);

            var result = sut.Evaluate(device, new SmartInfo(), Array.Empty<FileSystemCheckResult>());

            Assert.Equal(HealthStatus.Unknown, result.OverallStatus);
        }

        [Fact]
        public void Evaluate_NullSmartInfoAndNullFileSystemResults_UsesSafeDefaults()
        {
            var sut = new HealthEvaluator();
            var device = MakeDevice(opStatus: OperationalStatus.OK);

            var result = sut.Evaluate(device, null!, null!);

            Assert.Equal(HealthStatus.Healthy, result.OverallStatus);
            Assert.Empty(result.Diagnostics);
        }
    }
}