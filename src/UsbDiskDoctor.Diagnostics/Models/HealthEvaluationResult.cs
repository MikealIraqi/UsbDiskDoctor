using System;
using System.Collections.Generic;
using UsbDiskDoctor.Core.Enums;
using UsbDiskDoctor.Core.Models;

namespace UsbDiskDoctor.Diagnostics.Models
{
    /// <summary>
    /// Represents the result of a comprehensive health evaluation for a storage device.
    /// </summary>
    public sealed record HealthEvaluationResult
    {
        public HealthStatus OverallStatus { get; init; } = HealthStatus.Unknown;
        public IReadOnlyList<DiagnosticResult> Diagnostics { get; init; } = Array.Empty<DiagnosticResult>();
        public string SummaryAr { get; init; } = string.Empty;
        public string SummaryEn { get; init; } = string.Empty;
    }
}