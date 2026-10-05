using System;
using System.Threading;
using System.Threading.Tasks;

namespace UsbDiskDoctor.Repair.Execution
{
    /// <summary>
    /// Executes external commands with timeout, output capture, and error handling.
    /// </summary>
    public interface ICommandRunner
    {
        /// <summary>
        /// Runs an external process with the given executable and arguments.
        /// Captures stdout/stderr, enforces a timeout, and NEVER throws —
        /// returns a structured result even on failure.
        /// </summary>
        /// <param name="executable">
        /// Full path or name of the executable (e.g., "cmd.exe").
        /// Caller is responsible for providing a trusted value.
        /// </param>
        /// <param name="arguments">Command arguments. Must come from trusted source.</param>
        /// <param name="timeout">Maximum execution time. Defaults to 30 seconds.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>CommandRunResult with captured output and status. Never null.</returns>
        Task<CommandRunResult> RunAsync(
            string executable,
            string arguments,
            TimeSpan timeout,
            CancellationToken cancellationToken = default);
    }
}