using System;
using System.Collections.Generic;
using UsbDiskDoctor.Core.Models;

namespace UsbDiskDoctor.Diagnostics.Models
{
    /// <summary>
    /// Represents a complete diagnostic report for a single storage device,
    /// including SMART data, file system checks, and health evaluation.
    /// </summary>
    public sealed record DeviceDiagnosticReport
    {
        public DeviceSummary Device { get; init; } = null!;
        public SmartInfo SmartInfo { get; init; } = new SmartInfo();
        public IReadOnlyList<FileSystemCheckResult> FileSystemResults { get; init; } = Array.Empty<FileSystemCheckResult>();
        public HealthEvaluationResult HealthEvaluation { get; init; } = new HealthEvaluationResult();
        public DateTimeOffset Timestamp { get; init; } = DateTimeOffset.UtcNow;
    }
}