using UsbDiskDoctor.Core.Enums;

namespace UsbDiskDoctor.Core.Models
{
    /// <summary>
    /// Represents a file recovery session, tracking its source, target, lifecycle and results.
    /// </summary>
    public sealed record RecoverySession
    {
        public Guid Id { get; init; } = Guid.NewGuid();
        public string SourceDiskId { get; init; } = string.Empty;
        public string SourceVolume { get; init; } = string.Empty;
        public string TargetFolder { get; init; } = string.Empty;
        public string ScanMode { get; init; } = string.Empty;
        public RecoverySessionStatus Status { get; init; } = RecoverySessionStatus.Pending;
        public DateTimeOffset StartedAt { get; init; } = DateTimeOffset.UtcNow;
        public DateTimeOffset? FinishedAt { get; init; }
        public int RecoveredFilesCount { get; init; } = 0;
        public IReadOnlyList<string> Errors { get; init; } = Array.Empty<string>();
    }
}