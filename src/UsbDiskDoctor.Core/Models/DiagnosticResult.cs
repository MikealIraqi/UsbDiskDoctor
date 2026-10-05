using UsbDiskDoctor.Core.Enums;

namespace UsbDiskDoctor.Core.Models
{
    /// <summary>
    /// Represents a single diagnostic finding produced by the diagnostics engine for a specific device.
    /// </summary>
    public sealed record DiagnosticResult
    {
        public Guid Id { get; init; } = Guid.NewGuid();
        public string DeviceId { get; init; } = string.Empty;
        public DateTimeOffset Timestamp { get; init; } = DateTimeOffset.UtcNow;
        public Severity Severity { get; init; } = Severity.Info;
        public string Code { get; init; } = string.Empty;
        public string TitleAr { get; init; } = string.Empty;
        public string TitleEn { get; init; } = string.Empty;
        public string DescriptionAr { get; init; } = string.Empty;
        public string DescriptionEn { get; init; } = string.Empty;
        public IReadOnlyList<string> Evidence { get; init; } = Array.Empty<string>();
        public string? RecommendedAction { get; init; }
    }
}