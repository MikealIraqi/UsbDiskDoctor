namespace UsbDiskDoctor.Core.Models
{
    /// <summary>
    /// Represents S.M.A.R.T. health data reported by the storage device, if available.
    /// </summary>
    public sealed record SmartInfo
    {
        public bool Available { get; init; } = false;
        public bool PredictFailure { get; init; } = false;
        public string? Reason { get; init; }
        public int? Temperature { get; init; }
        public IReadOnlyDictionary<string, string> RawAttributes { get; init; } = new Dictionary<string, string>();
    }
}