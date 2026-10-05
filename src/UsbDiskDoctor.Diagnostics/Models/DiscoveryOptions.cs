namespace UsbDiskDoctor.Diagnostics.Models
{
    /// <summary>
    /// Options controlling which devices are included in a discovery pass.
    /// </summary>
    public sealed record DiscoveryOptions
    {
        /// <summary>
        /// When true, only USB devices are included in the discovery results.
        /// </summary>
        public bool UsbOnly { get; init; } = true;

        /// <summary>
        /// When true, includes devices that are currently offline or disconnected.
        /// </summary>
        public bool IncludeOfflineDevices { get; init; } = false;
    }
}