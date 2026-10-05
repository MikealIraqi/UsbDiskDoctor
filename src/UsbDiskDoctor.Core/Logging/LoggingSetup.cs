using Serilog;
using Serilog.Events;
using System.IO;

namespace UsbDiskDoctor.Core.Logging
{
    /// <summary>
    /// Centralized Serilog configuration for the entire application.
    /// All projects use this via the shared Log.Logger.
    /// </summary>
    public static class LoggingSetup
    {
        private const string LogFolderName = "logs";
        private const string LogFileName = "usbdiskdoctor-.log";

        /// <summary>
        /// Initializes Serilog with File + Debug sinks.
        /// Safe to call once at application startup.
        /// </summary>
        /// <param name="baseFolder">
        /// Base folder under which the logs/ directory will be created.
        /// Defaults to the current working directory.
        /// </param>
        /// <returns>The fully-qualified path of the log directory.</returns>
        public static string Initialize(string? baseFolder = null)
        {
            var root = string.IsNullOrWhiteSpace(baseFolder)
                ? Directory.GetCurrentDirectory()
                : baseFolder!;

            var logDir = Path.Combine(root, LogFolderName);
            Directory.CreateDirectory(logDir);

            var logPath = Path.Combine(logDir, LogFileName);

            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.Debug()
                .WriteTo.File(
                    path: logPath,
                    rollingInterval: RollingInterval.Day,
                    retainedFileCountLimit: 14,
                    outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {SourceContext}: {Message:lj}{NewLine}{Exception}")
                .CreateLogger();

            Log.Information("Serilog initialized. Log directory: {LogDir}", logDir);
            return logDir;
        }

        /// <summary>
        /// Flushes and disposes the global logger. Call at application shutdown.
        /// </summary>
        public static void Shutdown()
        {
            Log.Information("Serilog shutting down.");
            Log.CloseAndFlush();
        }
    }
}