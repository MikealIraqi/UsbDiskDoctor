namespace UsbDiskDoctor.Repair.Execution
{
    /// <summary>
    /// Represents the outcome of a repair-execution attempt.
    /// </summary>
    public sealed record RepairExecutionResult
    {
        /// <summary>True if the executor attempted to run the action (not rejected).</summary>
        public bool Executed { get; init; } = false;

        /// <summary>True if the action succeeded end-to-end.</summary>
        public bool Succeeded { get; init; } = false;

        /// <summary>Optional machine-readable rejection code.</summary>
        public string? RejectionCode { get; init; }

        /// <summary>Result of the underlying command, if any was run.</summary>
        public CommandRunResult? CommandResult { get; init; }

        /// <summary>User-facing message (Arabic).</summary>
        public string MessageAr { get; init; } = string.Empty;

        /// <summary>User-facing message (English).</summary>
        public string MessageEn { get; init; } = string.Empty;
    }
}