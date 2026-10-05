using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UsbDiskDoctor.Core.Models;

namespace UsbDiskDoctor.Diagnostics.VolumeReading
{
    /// <summary>
    /// Reads volume information from physical storage devices using Windows APIs.
    /// </summary>
    public interface IVolumeReader
    {
        /// <summary>
        /// Reads all volumes (partitions + logical disks) for the given 
        /// physical drive, identified by its WMI DeviceID.
        /// Read-only.
        /// </summary>
        /// <param name="physicalDriveDeviceId">The WMI DeviceID of the physical drive (e.g., \\.\PHYSICALDRIVE2).</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Read-only list of volume information.</returns>
        Task<IReadOnlyList<VolumeInfo>> ReadVolumesAsync(
            string physicalDriveDeviceId,
            CancellationToken cancellationToken = default);
    }
}