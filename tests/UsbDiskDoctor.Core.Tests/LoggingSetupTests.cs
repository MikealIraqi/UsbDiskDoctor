using System;
using System.IO;
using UsbDiskDoctor.Core.Logging;
using Xunit;

namespace UsbDiskDoctor.Core.Tests
{
    /// <summary>
    /// Smoke tests for the Serilog logging setup.
    /// </summary>
    public class LoggingSetupTests : IDisposable
    {
        private readonly string _tempRoot;

        public LoggingSetupTests()
        {
            _tempRoot = Path.Combine(
                Path.GetTempPath(),
                "UsbDiskDoctorTests",
                Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(_tempRoot);
        }

        [Fact]
        public void Initialize_CreatesLogDirectory_And_WritesEntry()
        {
            // Act
            var logDir = LoggingSetup.Initialize(_tempRoot);
            Serilog.Log.Information("Test message from LoggingSetupTests");
            LoggingSetup.Shutdown();

            // Assert
            Assert.True(Directory.Exists(logDir), "Log directory was not created.");
            var files = Directory.GetFiles(logDir, "*.log");
            Assert.NotEmpty(files);

            var content = File.ReadAllText(files[0]);
            Assert.Contains("Test message from LoggingSetupTests", content);
        }

        public void Dispose()
        {
            try
            {
                if (Directory.Exists(_tempRoot))
                {
                    Directory.Delete(_tempRoot, recursive: true);
                }
            }
            catch
            {
                // Best-effort cleanup; do not fail the test run.
            }
        }
    }
}