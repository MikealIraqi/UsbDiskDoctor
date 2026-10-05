using System;
using UsbDiskDoctor.Core.Enums;
using UsbDiskDoctor.Core.Models;
using UsbDiskDoctor.Diagnostics.Models;
using UsbDiskDoctor.Reporting;
using Xunit;

namespace UsbDiskDoctor.Diagnostics.Tests.Reporting
{
    /// <summary>
    /// Unit tests for <see cref="HtmlReportGenerator"/>.
    /// </summary>
    public class HtmlReportGeneratorTests
    {
        private static DeviceDiagnosticReport MakeReport(
            string friendlyName = "Test Drive",
            HealthStatus overallStatus = HealthStatus.Healthy)
        {
            return new DeviceDiagnosticReport
            {
                Device = new DeviceSummary
                {
                    DeviceId = @"\\.\PHYSICALDRIVE1",
                    FriendlyName = friendlyName
                },
                HealthEvaluation = new HealthEvaluationResult { OverallStatus = overallStatus }
            };
        }

        [Fact]
        public void Generate_ThrowsArgumentNullException_OnNullInput()
        {
            var sut = new HtmlReportGenerator();

            Assert.Throws<ArgumentNullException>(() => sut.Generate(null!));
        }

        [Fact]
        public void Generate_HasRtlAndArabicLanguage()
        {
            var sut = new HtmlReportGenerator();
            var html = sut.Generate(Array.Empty<DeviceDiagnosticReport>());

            Assert.Contains("dir=\"rtl\"", html);
            Assert.Contains("lang=\"ar\"", html);
            Assert.Contains("<!DOCTYPE html>", html);
        }

        [Fact]
        public void Generate_EscapesHtmlInDeviceName_PreventingXss()
        {
            var sut = new HtmlReportGenerator();
            var report = MakeReport(friendlyName: "<script>alert('xss')</script>");

            var html = sut.Generate(new[] { report });

            Assert.Contains("&lt;script&gt;", html);
            Assert.DoesNotContain("<script>alert", html);
        }

        [Fact]
        public void Generate_IncludesBadgeClassForCriticalStatus()
        {
            var sut = new HtmlReportGenerator();
            var report = MakeReport(overallStatus: HealthStatus.Critical);

            var html = sut.Generate(new[] { report });

            Assert.Contains("badge-critical", html);
        }

        [Fact]
        public void Generate_HandlesEmptyReportList()
        {
            var sut = new HtmlReportGenerator();
            var html = sut.Generate(Array.Empty<DeviceDiagnosticReport>());

            Assert.NotEmpty(html);
            Assert.Contains("<html", html);
            Assert.Contains("</html>", html);
            Assert.Contains("عدد الأجهزة", html);
        }
    }
}