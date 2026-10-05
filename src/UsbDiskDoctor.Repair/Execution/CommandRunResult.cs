using System;

namespace UsbDiskDoctor.Repair.Execution
{
    /// <summary>
    /// Represents the result of executing an external command.
    /// </summary>
    public sealed record CommandRunResult
    {
        public bool StartedSuccessfully { get; init; } = false;
        public int ExitCode { get; init; } = -1;
        public string StdOut { get; init; } = string.Empty;
        public string StdErr { get; init; } = string.Empty;
        public bool TimedOut { get; init; } = false;
        public TimeSpan Duration { get; init; } = TimeSpan.Zero;
        public string? FailureReason { get; init; } = null;
    }
}