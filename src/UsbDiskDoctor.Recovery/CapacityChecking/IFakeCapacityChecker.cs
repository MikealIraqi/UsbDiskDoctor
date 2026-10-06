using UsbDiskDoctor.Recovery.Models;

namespace UsbDiskDoctor.Recovery.CapacityChecking;

/// <summary>
/// Service for detecting fake capacity on storage devices (devices that report
/// larger capacity than actual, with firmware wrapping writes to same blocks).
/// </summary>
public interface IFakeCapacityChecker
{
    /// <summary>
    /// Checks a storage device for fake capacity by writing and verifying test patterns.
    /// This is a long-running operation that may take hours in Full mode.
    /// </summary>
    /// <param name="driveRoot">Root path of the drive to check (e.g., "E:\").</param>
    /// <param name="options">Check options including mode, block size, and test file path.</param>
    /// <param name="progress">Optional progress reporter for UI updates.</param>
    /// <param name="cancellationToken">Cancellation token. Must be respected; OperationCanceledException should be thrown on cancellation.</param>
    /// <returns>A CapacityCheckResult containing the verdict and diagnostic information.</returns>
    /// <exception cref="ArgumentException">Thrown when driveRoot is null or empty.</exception>
    /// <exception cref="ArgumentNullException">Thrown when options is null.</exception>
    /// <exception cref="OperationCanceledException">Thrown when the operation is cancelled via cancellationToken.</exception>
    /// <remarks>
    /// The options.TestFilePath must be on the same volume as driveRoot.
    /// This operation writes test data to the device and may take significant time.
    /// Ensure the device has sufficient free space for the test file.
    /// </remarks>
    Task<CapacityCheckResult> CheckAsync(
        string driveRoot,
        CapacityCheckOptions options,
        IProgress<CapacityCheckProgress>? progress = null,
        CancellationToken cancellationToken = default);
}