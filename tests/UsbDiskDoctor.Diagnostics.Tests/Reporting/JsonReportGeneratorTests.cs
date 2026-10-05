using System;
using System.Text.Json;
using UsbDiskDoctor.Core.Enums;
using UsbDiskDoctor.Core.Models;
using UsbDiskDoctor.Diagnostics.Models;
using UsbDiskDoctor.Reporting;
using Xunit;

namespace UsbDiskDoctor.Diagnostics.Tests.Reporting
{
    /// <summary>
    /// Unit tests for <see cref="JsonReportGenerator"/>.
    /// </summary>
    public class JsonReportGeneratorTests
    {
        private static DeviceDiagnosticReport MakeReport(
            string deviceId = @"\\.\PHYSICALDRIVE1",
            HealthStatus overallStatus = HealthStatus.Healthy)
        {
            return new DeviceDiagnosticReport
            {
                Device = new DeviceSummary { DeviceId = deviceId },
                HealthEvaluation = new HealthEvaluationResult { OverallStatus = overallStatus }
            };
        }

        [Fact]
        public void Generate_ThrowsArgumentNullException_OnNullInput()
        {
            var sut = new JsonReportGenerator();

            Assert.Throws<ArgumentNullException>(() => sut.Generate(null!));
        }

        [Fact]
        public void Generate_HasExpectedMetadata()
        {
            var sut = new JsonReportGenerator();
            var json = sut.Generate(Array.Empty<DeviceDiagnosticReport>());

            using var document = JsonDocument.Parse(json);
            var root = document.RootElement;

            Assert.Equal("UsbDiskDoctor", root.GetProperty("ApplicationName").GetString());
            Assert.Equal("0.1.0", root.GetProperty("ApplicationVersion").GetString());
            Assert.Equal(0, root.GetProperty("ReportCount").GetInt32());
            Assert.Equal(JsonValueKind.Array, root.GetProperty("Reports").ValueKind);
        }

        [Fact]
        public void Generate_SerializesEnumsAsNames()
        {
            var sut = new JsonReportGenerator();
            var report = MakeReport(overallStatus: HealthStatus.Critical);

            var json = sut.Generate(new[] { report });

            Assert.Contains("\"Critical\"", json);
        }

        [Fact]
        public void Generate_ReportCountMatchesInput()
        {
            var sut = new JsonReportGenerator();
            var reports = new[]
            {
                MakeReport(@"\\.\PHYSICALDRIVE1"),
                MakeReport(@"\\.\PHYSICALDRIVE2"),
                MakeReport(@"\\.\PHYSICALDRIVE3")
            };

            var json = sut.Generate(reports);

            using var document = JsonDocument.Parse(json);
            Assert.Equal(3, document.RootElement.GetProperty("ReportCount").GetInt32());
        }
    }
}