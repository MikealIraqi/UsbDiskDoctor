using System.Threading;
using System.Threading.Tasks;
using UsbDiskDoctor.Core.Models;

namespace UsbDiskDoctor.Diagnostics.HealthChecks
{
    /// <summary>
    /// Reads S.M.A.R.T. (Self-Monitoring, Analysis, and Reporting Technology) 
    /// data from storage devices to predict potential failures.
    /// </summary>
    public interface ISmartReader
    {
        /// <summary>
        /// Attempts to read S.M.A.R.T. failure-prediction status for the 
        /// given physical drive. Read-only.
        /// </summary>
        /// <param name="physicalDriveDeviceId">
        ///   WMI DeviceID of the physical drive (e.g., \\.\PHYSICALDRIVE2).
        /// </param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>
        ///   A SmartInfo instance. If SMART data is not available (which is common 
        ///   for USB flash drives), the result has Available=false and 
        ///   PredictFailure=false. Never returns null.
        /// </returns>
        Task<SmartInfo> ReadAsync(
            string physicalDriveDeviceId,
            CancellationToken cancellationToken = default);
    }
}