using System;
using UsbDiskDoctor.Core.Enums;

namespace UsbDiskDoctor.Repair.Execution
{
    /// <summary>
    /// Describes a whitelisted repair action. Only actions listed in the
    /// executor's whitelist may be executed.
    /// </summary>
    internal sealed record ActionSpec
    {
        public string ActionCode { get; init; } = string.Empty;
        public RiskLevel RequiredRiskLevel { get; init; } = RiskLevel.Safe;
        public string? RequiredConfirmationToken { get; init; }
        public string? Executable { get; init; }
        public string? ArgumentsTemplate { get; init; }
        public bool RequiresDriveLetter { get; init; }
        public TimeSpan Timeout { get; init; } = TimeSpan.FromMinutes(1);
        public string MessageAr { get; init; } = string.Empty;
        public string MessageEn { get; init; } = string.Empty;
    }
}