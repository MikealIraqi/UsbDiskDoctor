using System.Collections.Generic;
using UsbDiskDoctor.Diagnostics.Models;

namespace UsbDiskDoctor.Reporting
{
    /// <summary>
    /// Generates diagnostic reports in various formats.
    /// Implementations should be pure functions with no I/O operations.
    /// </summary>
    public interface IReportGenerator
    {
        /// <summary>
        /// Short format identifier, e.g., "JSON" or "HTML".
        /// </summary>
        string Format { get; }

        /// <summary>
        /// File extension including the leading dot, e.g., ".json".
        /// </summary>
        string FileExtension { get; }

        /// <summary>
        /// Renders a report document as a string.
        /// Pure function — no I/O, no file system access.
        /// </summary>
        /// <param name="reports">Collection of diagnostic reports to include.</param>
        /// <returns>Formatted report string. Never null.</returns>
        string Generate(IReadOnlyList<DeviceDiagnosticReport> reports);
    }
}