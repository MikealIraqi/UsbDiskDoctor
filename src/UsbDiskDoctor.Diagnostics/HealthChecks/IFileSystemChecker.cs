using System.Threading;
using System.Threading.Tasks;
using UsbDiskDoctor.Diagnostics.Models;

namespace UsbDiskDoctor.Diagnostics.HealthChecks
{
    /// <summary>
    /// Performs read-only inspections of file systems on storage volumes.
    /// </summary>
    public interface IFileSystemChecker
    {
        /// <summary>
        /// Performs a read-only inspection of the file system on the volume
        /// identified by the given drive letter (e.g., "E:").
        /// Never modifies the volume.
        /// </summary>
        /// <param name="driveLetter">Drive letter including colon (e.g., "E:").</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>
        ///   A FileSystemCheckResult. On failure, CheckSucceeded=false and 
        ///   ErrorMessage contains a description. Never returns null.
        /// </returns>
        Task<FileSystemCheckResult> CheckAsync(
            string driveLetter,
            CancellationToken cancellationToken = default);
    }
}