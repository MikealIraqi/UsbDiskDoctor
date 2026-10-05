using UsbDiskDoctor.Core.Enums;

namespace UsbDiskDoctor.Core.Models
{
    /// <summary>
    /// Represents a proposed repair action derived from a diagnostic finding. Never executed automatically.
    /// </summary>
    public sealed record RepairProposal
    {
        public Guid Id { get; init; } = Guid.NewGuid();
        public Guid DiagnosticResultId { get; init; }
        public string ActionCode { get; init; } = string.Empty;
        public RiskLevel RiskLevel { get; init; } = RiskLevel.Safe;
        public bool RequiresConfirmation { get; init; } = false;
        public string DescriptionAr { get; init; } = string.Empty;
        public string DescriptionEn { get; init; } = string.Empty;
        public string? CommandPreview { get; init; }
        public bool IsExecutableNow { get; init; } = false;
    }
}