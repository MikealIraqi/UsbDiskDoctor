using System.Threading;
using System.Threading.Tasks;
using UsbDiskDoctor.Core.Models;
using UsbDiskDoctor.Diagnostics.Models;

namespace UsbDiskDoctor.Diagnostics.DiagnosticEngine
{
    /// <summary>
    /// Orchestrates diagnostic checks on storage devices, coordinating
    /// SMART reading, file system checks, and health evaluation.
    /// </summary>
    public interface IDiagnosticEngine
    {
        /// <summary>
        /// Runs a complete diagnostic pass on a single device:
        /// SMART reading, file system checks for each mounted volume,
        /// and health evaluation.
        /// Read-only. Never modifies the device.
        /// </summary>
        /// <param name="device">The device to diagnose.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>
        ///   A DeviceDiagnosticReport. Never returns null. If individual 
        ///   steps fail, they are logged and safe defaults are used.
        /// </returns>
        Task<DeviceDiagnosticReport> DiagnoseAsync(
            DeviceSummary device,
            CancellationToken cancellationToken = default);
    }
}