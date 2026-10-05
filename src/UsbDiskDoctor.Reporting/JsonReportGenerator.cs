using System;
using System.Collections.Generic;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using UsbDiskDoctor.Diagnostics.Models;

namespace UsbDiskDoctor.Reporting
{
    /// <summary>
    /// Generates JSON reports for diagnostic results.
    /// </summary>
    public sealed class JsonReportGenerator : IReportGenerator
    {
        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            WriteIndented = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.Never,
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
            Converters = { new JsonStringEnumConverter() }
        };

        public string Format => "JSON";

        public string FileExtension => ".json";

        public string Generate(IReadOnlyList<DeviceDiagnosticReport> reports)
        {
            if (reports == null)
            {
                throw new ArgumentNullException(nameof(reports));
            }

            var document = new
            {
                GeneratedAtUtc = DateTimeOffset.UtcNow,
                ApplicationName = "UsbDiskDoctor",
                ApplicationVersion = "0.1.0",
                ReportCount = reports.Count,
                Reports = reports
            };

            return JsonSerializer.Serialize(document, JsonOptions);
        }
    }
}