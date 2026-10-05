using System.Collections.Generic;
using UsbDiskDoctor.Core.Models;
using UsbDiskDoctor.Diagnostics.Models;

namespace UsbDiskDoctor.Repair.Planning
{
    /// <summary>
    /// Plans repair actions based on diagnostic reports.
    /// Pure function — no I/O, never executes commands.
    /// </summary>
    public interface IRepairPlanner
    {
        /// <summary>
        /// Builds repair proposals for all problems found in a diagnostic report.
        /// Pure function — no I/O, never executes commands.
        /// </summary>
        /// <param name="report">Diagnostic report to plan repairs for.</param>
        /// <returns>
        ///   One RepairProposal per actionable diagnostic. Never null.
        ///   If no diagnostics exist, returns empty list.
        /// </returns>
        IReadOnlyList<RepairProposal> Plan(DeviceDiagnosticReport report);
    }
}