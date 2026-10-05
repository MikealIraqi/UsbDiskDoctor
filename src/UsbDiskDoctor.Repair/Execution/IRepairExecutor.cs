using System.Threading;
using System.Threading.Tasks;
using UsbDiskDoctor.Core.Models;

namespace UsbDiskDoctor.Repair.Execution
{
    /// <summary>
    /// Executes whitelisted repair actions. Never runs arbitrary commands
    /// from <see cref="RepairProposal.CommandPreview"/>.
    /// </summary>
    public interface IRepairExecutor
    {
        /// <summary>
        /// Executes the given repair proposal after validating:
        /// 1. The action code is present in the whitelist.
        /// 2. The proposal's risk level matches the whitelist.
        /// 3. The confirmation token matches the required token (exact match).
        /// 4. A drive letter is supplied when required.
        /// </summary>
        /// <param name="proposal">The proposal to execute.</param>
        /// <param name="confirmationToken">
        ///   Optional confirmation token. Required for Medium actions ("CONFIRM").
        ///   Exact, case-sensitive match.
        /// </param>
        /// <param name="targetDriveLetter">
        ///   Target drive letter (e.g., "E:") when the action requires one.
        /// </param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Never null.</returns>
        Task<RepairExecutionResult> ExecuteAsync(
            RepairProposal proposal,
            string? confirmationToken,
            string? targetDriveLetter,
            CancellationToken cancellationToken = default);
    }
}