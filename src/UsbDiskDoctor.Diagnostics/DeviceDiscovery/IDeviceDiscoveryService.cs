using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UsbDiskDoctor.Core.Models;
using UsbDiskDoctor.Diagnostics.Models;

namespace UsbDiskDoctor.Diagnostics.DeviceDiscovery
{
    /// <summary>
    /// Discovers USB storage devices attached to the system and returns a snapshot for each.
    /// </summary>
    public interface IDeviceDiscoveryService
    {
        /// <summary>
        /// Enumerates currently attached USB storage devices.
        /// Read-only. Never modifies the devices.
        /// </summary>
        /// <param name="options">Discovery options (may be null for defaults).</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Read-only list of device snapshots.</returns>
        Task<IReadOnlyList<DeviceSummary>> DiscoverAsync(
            DiscoveryOptions? options = null,
            CancellationToken cancellationToken = default);
    }
}