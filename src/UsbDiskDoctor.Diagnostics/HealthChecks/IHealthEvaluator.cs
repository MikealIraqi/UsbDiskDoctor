using System.Collections.Generic;
using UsbDiskDoctor.Core.Models;
using UsbDiskDoctor.Diagnostics.Models;

namespace UsbDiskDoctor.Diagnostics.HealthChecks
{
    /// <summary>
    /// Evaluates the overall health of storage devices based on SMART data,
    /// operational status, and file system check results.
    /// </summary>
    public interface IHealthEvaluator
    {
        /// <summary>
        /// Evaluates the overall health of a device based on SMART status,
        /// operational state, and file system check results.
        /// Pure function — no I/O, no side effects.
        /// </summary>
        HealthEvaluationResult Evaluate(
            DeviceSummary device,
            SmartInfo smartInfo,
            IReadOnlyList<FileSystemCheckResult> fileSystemResults);
    }
}