namespace UsbDiskDoctor.Core.Models
{
    /// <summary>
    /// Represents an entry in the local knowledge base, mapped to a problem code produced by the diagnostics engine.
    /// </summary>
    public sealed record KnowledgeArticle
    {
        public string Code { get; init; } = string.Empty;
        public string TitleAr { get; init; } = string.Empty;
        public string TitleEn { get; init; } = string.Empty;
        public string CauseAr { get; init; } = string.Empty;
        public string CauseEn { get; init; } = string.Empty;
        public IReadOnlyList<string> SafeActions { get; init; } = Array.Empty<string>();
        public IReadOnlyList<string> DangerousActions { get; init; } = Array.Empty<string>();
        public bool RequiresConfirmation { get; init; } = false;
        public IReadOnlyList<string> ExternalSearchKeywords { get; init; } = Array.Empty<string>();
    }
}