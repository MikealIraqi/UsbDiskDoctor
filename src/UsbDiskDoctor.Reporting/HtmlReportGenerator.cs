using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using UsbDiskDoctor.Core.Enums;
using UsbDiskDoctor.Diagnostics.Models;

namespace UsbDiskDoctor.Reporting
{
    /// <summary>
    /// Generates diagnostic reports in HTML format with RTL support for Arabic content.
    /// </summary>
    public sealed class HtmlReportGenerator : IReportGenerator
    {
        public string Format => "HTML";

        public string FileExtension => ".html";

        public string Generate(IReadOnlyList<DeviceDiagnosticReport> reports)
        {
            if (reports == null)
            {
                throw new ArgumentNullException(nameof(reports));
            }

            var htmlBuilder = new StringBuilder();

            htmlBuilder.AppendLine("<!DOCTYPE html>");
            htmlBuilder.AppendLine("<html lang=\"ar\" dir=\"rtl\">");
            htmlBuilder.AppendLine("<head>");
            htmlBuilder.AppendLine("  <meta charset=\"UTF-8\">");
            htmlBuilder.AppendLine("  <meta name=\"viewport\" content=\"width=device-width, initial-scale=1.0\">");
            htmlBuilder.AppendLine("  <title>تقرير UsbDiskDoctor</title>");
            htmlBuilder.AppendLine("  <style>");
            htmlBuilder.AppendLine(GetCssStyles());
            htmlBuilder.AppendLine("  </style>");
            htmlBuilder.AppendLine("</head>");
            htmlBuilder.AppendLine("<body>");

            htmlBuilder.AppendLine("  <header class=\"report-header\">");
            htmlBuilder.AppendLine("    <h1>تقرير فحص UsbDiskDoctor</h1>");
            htmlBuilder.AppendLine("    <div class=\"meta\">");
            htmlBuilder.AppendLine($"      <span>تاريخ التقرير: <strong>{DateTimeOffset.UtcNow:yyyy-MM-dd HH:mm:ss} UTC</strong></span>");
            htmlBuilder.AppendLine($"      <span>عدد الأجهزة: <strong>{reports.Count}</strong></span>");
            htmlBuilder.AppendLine("    </div>");
            htmlBuilder.AppendLine("  </header>");

            foreach (var deviceReport in reports)
            {
                var device = deviceReport.Device;
                var smartInfo = deviceReport.SmartInfo;
                var fileSystemResults = deviceReport.FileSystemResults;
                var healthEvaluation = deviceReport.HealthEvaluation;

                htmlBuilder.AppendLine("  <section class=\"device-card\">");

                htmlBuilder.AppendLine("    <div class=\"device-header\">");
                htmlBuilder.AppendLine($"      <h2>{Enc(device.FriendlyName)}</h2>");
                htmlBuilder.AppendLine($"      <span class=\"badge badge-{StatusClass(healthEvaluation.OverallStatus)}\">{healthEvaluation.OverallStatus}</span>");
                htmlBuilder.AppendLine("    </div>");
                htmlBuilder.AppendLine($"    <p class=\"summary\">{Enc(healthEvaluation.SummaryAr)}</p>");

                htmlBuilder.AppendLine("    <h3>معلومات الجهاز</h3>");
                htmlBuilder.AppendLine("    <table class=\"info-table\">");
                htmlBuilder.AppendLine($"      <tr><th>معرف الجهاز</th><td>{Enc(device.DeviceId)}</td></tr>");
                htmlBuilder.AppendLine($"      <tr><th>الموديل</th><td>{Enc(device.Model ?? "-")}</td></tr>");
                htmlBuilder.AppendLine($"      <tr><th>الرقم التسلسلي</th><td>{Enc(device.SerialNumber ?? "-")}</td></tr>");
                htmlBuilder.AppendLine($"      <tr><th>نوع الناقل</th><td>{device.BusType}</td></tr>");
                htmlBuilder.AppendLine($"      <tr><th>نوع الوسائط</th><td>{device.MediaType}</td></tr>");
                htmlBuilder.AppendLine($"      <tr><th>نمط التقسيم</th><td>{device.PartitionStyle}</td></tr>");
                htmlBuilder.AppendLine($"      <tr><th>الحجم</th><td>{FormatBytes(device.SizeBytes)}</td></tr>");
                htmlBuilder.AppendLine($"      <tr><th>الحالة التشغيلية</th><td>{device.OperationalStatus}</td></tr>");
                htmlBuilder.AppendLine("    </table>");

                htmlBuilder.AppendLine("    <h3>بيانات SMART</h3>");
                if (smartInfo.Available)
                {
                    htmlBuilder.AppendLine("    <table class=\"info-table\">");
                    htmlBuilder.AppendLine("      <tr><th>الحالة</th><td>متوفرة</td></tr>");
                    htmlBuilder.AppendLine($"      <tr><th>توقع الفشل</th><td>{(smartInfo.PredictFailure ? "نعم" : "لا")}</td></tr>");
                    htmlBuilder.AppendLine($"      <tr><th>السبب</th><td>{Enc(smartInfo.Reason ?? "-")}</td></tr>");
                    htmlBuilder.AppendLine("    </table>");
                }
                else
                {
                    htmlBuilder.AppendLine("    <p class=\"muted\">بيانات SMART غير متوفرة (شائع لأقراص USB).</p>");
                }

                htmlBuilder.AppendLine($"    <h3>أنظمة الملفات ({fileSystemResults.Count})</h3>");
                if (fileSystemResults.Count > 0)
                {
                    htmlBuilder.AppendLine("    <table class=\"data-table\">");
                    htmlBuilder.AppendLine("      <thead>");
                    htmlBuilder.AppendLine("        <tr>");
                    htmlBuilder.AppendLine("          <th>الحرف</th><th>الاسم</th><th>النظام</th>");
                    htmlBuilder.AppendLine("          <th>الحجم</th><th>المتاح</th><th>مُثبّت</th><th>RAW</th><th>Dirty</th>");
                    htmlBuilder.AppendLine("        </tr>");
                    htmlBuilder.AppendLine("      </thead>");
                    htmlBuilder.AppendLine("      <tbody>");

                    foreach (var volumeResult in fileSystemResults)
                    {
                        htmlBuilder.AppendLine("        <tr>");
                        htmlBuilder.AppendLine($"          <td>{Enc(volumeResult.DriveLetter)}</td>");
                        htmlBuilder.AppendLine($"          <td>{Enc(volumeResult.Label)}</td>");
                        htmlBuilder.AppendLine($"          <td>{volumeResult.FileSystem}</td>");
                        htmlBuilder.AppendLine($"          <td>{FormatBytes(volumeResult.CapacityBytes)}</td>");
                        htmlBuilder.AppendLine($"          <td>{FormatBytes(volumeResult.FreeSpaceBytes)}</td>");
                        htmlBuilder.AppendLine($"          <td>{(volumeResult.IsMounted ? "✓" : "✗")}</td>");
                        htmlBuilder.AppendLine($"          <td>{(volumeResult.IsRaw ? "⚠" : "—")}</td>");
                        htmlBuilder.AppendLine($"          <td>{(volumeResult.DirtyBitSet ? "⚠" : "—")}</td>");
                        htmlBuilder.AppendLine("        </tr>");
                    }

                    htmlBuilder.AppendLine("      </tbody>");
                    htmlBuilder.AppendLine("    </table>");
                }
                else
                {
                    htmlBuilder.AppendLine("    <p class=\"muted\">لا توجد أنظمة ملفات مقروءة.</p>");
                }

                var diagnosticsList = healthEvaluation.Diagnostics;
                htmlBuilder.AppendLine($"    <h3>المشاكل المكتشفة ({diagnosticsList.Count})</h3>");
                if (diagnosticsList.Count > 0)
                {
                    htmlBuilder.AppendLine("    <div class=\"diagnostics\">");

                    foreach (var diagnosticItem in diagnosticsList)
                    {
                        htmlBuilder.AppendLine($"      <div class=\"diagnostic diag-{SeverityClass(diagnosticItem.Severity)}\">");
                        htmlBuilder.AppendLine("        <div class=\"diag-title\">");
                        htmlBuilder.AppendLine($"          <strong>{Enc(diagnosticItem.TitleAr)}</strong>");
                        htmlBuilder.AppendLine($"          <span class=\"diag-code\">{Enc(diagnosticItem.Code)}</span>");
                        htmlBuilder.AppendLine("        </div>");
                        htmlBuilder.AppendLine($"        <p>{Enc(diagnosticItem.DescriptionAr)}</p>");

                        if (diagnosticItem.Evidence.Count > 0)
                        {
                            htmlBuilder.AppendLine("        <ul class=\"evidence\">");
                            foreach (var evidenceItem in diagnosticItem.Evidence)
                            {
                                htmlBuilder.AppendLine($"          <li>{Enc(evidenceItem)}</li>");
                            }
                            htmlBuilder.AppendLine("        </ul>");
                        }

                        if (!string.IsNullOrWhiteSpace(diagnosticItem.RecommendedAction))
                        {
                            htmlBuilder.AppendLine($"        <p class=\"recommendation\">التوصية: {Enc(diagnosticItem.RecommendedAction)}</p>");
                        }

                        htmlBuilder.AppendLine("      </div>");
                    }

                    htmlBuilder.AppendLine("    </div>");
                }
                else
                {
                    htmlBuilder.AppendLine("    <p class=\"muted\">لا مشاكل مكتشفة.</p>");
                }

                htmlBuilder.AppendLine("  </section>");
            }

            htmlBuilder.AppendLine("  <footer>");
            htmlBuilder.AppendLine("    <p>تم توليد هذا التقرير بواسطة UsbDiskDoctor</p>");
            htmlBuilder.AppendLine("  </footer>");
            htmlBuilder.AppendLine("</body>");
            htmlBuilder.AppendLine("</html>");

            return htmlBuilder.ToString();
        }

        private static string GetCssStyles()
        {
            return @"
    * {
      margin: 0;
      padding: 0;
      box-sizing: border-box;
    }
    
    body {
      font-family: 'Segoe UI', Tahoma, Arial, sans-serif;
      background: #f9fafb;
      padding: 20px;
      line-height: 1.6;
    }
    
    .report-header {
      background: white;
      border: 1px solid #e5e7eb;
      padding: 20px;
      margin-bottom: 20px;
      border-radius: 8px;
    }
    
    .report-header h1 {
      font-size: 24px;
      margin-bottom: 10px;
    }
    
    .meta {
      display: flex;
      gap: 20px;
      font-size: 14px;
      color: #6b7280;
    }
    
    .device-card {
      background: white;
      border-radius: 8px;
      padding: 20px;
      margin-bottom: 20px;
      box-shadow: 0 1px 3px rgba(0,0,0,0.1);
    }
    
    .device-header {
      display: flex;
      justify-content: space-between;
      align-items: center;
      margin-bottom: 10px;
    }
    
    .device-header h2 {
      font-size: 20px;
    }
    
    .badge {
      padding: 4px 12px;
      border-radius: 999px;
      font-size: 12px;
      font-weight: 600;
    }
    
    .badge-critical { background: #fee2e2; color: #991b1b; }
    .badge-warning { background: #fef3c7; color: #92400e; }
    .badge-healthy { background: #d1fae5; color: #065f46; }
    .badge-unknown { background: #e5e7eb; color: #374151; }
    
    .summary {
      font-size: 14px;
      color: #4b5563;
      margin-bottom: 20px;
    }
    
    h3 {
      font-size: 16px;
      margin: 20px 0 10px 0;
      color: #111827;
    }
    
    .info-table {
      width: 100%;
      border-collapse: collapse;
      margin-bottom: 20px;
    }
    
    .info-table th {
      text-align: right;
      padding: 8px;
      background: #f9fafb;
      border: 1px solid #e5e7eb;
      width: 180px;
      font-weight: 600;
    }
    
    .info-table td {
      padding: 8px;
      border: 1px solid #e5e7eb;
    }
    
    .data-table {
      width: 100%;
      border-collapse: collapse;
      margin-bottom: 20px;
    }
    
    .data-table thead { background: #f3f4f6; }
    
    .data-table th {
      padding: 10px;
      text-align: right;
      border: 1px solid #e5e7eb;
      font-weight: 600;
    }
    
    .data-table td {
      padding: 10px;
      border: 1px solid #e5e7eb;
    }
    
    .data-table tbody tr:nth-child(even) { background: #f9fafb; }
    
    .diagnostics {
      display: flex;
      flex-direction: column;
      gap: 10px;
    }
    
    .diagnostic {
      border-right: 4px solid;
      padding: 15px;
      border-radius: 4px;
    }
    
    .diag-critical { border-right-color: #dc2626; background: #fef2f2; }
    .diag-error { border-right-color: #dc2626; background: #fef2f2; }
    .diag-warning { border-right-color: #f59e0b; background: #fffbeb; }
    .diag-info { border-right-color: #3b82f6; background: #eff6ff; }
    
    .diag-title {
      display: flex;
      justify-content: space-between;
      align-items: center;
      margin-bottom: 8px;
    }
    
    .diag-code {
      font-family: 'Courier New', monospace;
      font-size: 12px;
      color: #6b7280;
      background: rgba(0,0,0,0.05);
      padding: 2px 8px;
      border-radius: 4px;
    }
    
    .evidence {
      margin: 10px 0;
      padding-right: 20px;
    }
    
    .evidence li {
      margin: 5px 0;
      font-size: 13px;
      color: #4b5563;
    }
    
    .recommendation {
      color: #991b1b;
      font-weight: bold;
      margin-top: 10px;
    }
    
    .muted {
      color: #6b7280;
      font-style: italic;
      font-size: 14px;
    }
    
    footer {
      text-align: center;
      padding: 20px;
      color: #6b7280;
      font-size: 14px;
    }
";
        }

        private static string StatusClass(HealthStatus status)
        {
            return status switch
            {
                HealthStatus.Critical => "critical",
                HealthStatus.Warning => "warning",
                HealthStatus.Healthy => "healthy",
                _ => "unknown"
            };
        }

        private static string SeverityClass(Severity severity)
        {
            return severity switch
            {
                Severity.Critical => "critical",
                Severity.Error => "error",
                Severity.Warning => "warning",
                _ => "info"
            };
        }

        private static string FormatBytes(long bytes)
        {
            if (bytes < 1024)
                return $"{bytes} B";

            if (bytes < 1024L * 1024)
                return $"{bytes / 1024.0:F2} KB";

            if (bytes < 1024L * 1024 * 1024)
                return $"{bytes / (1024.0 * 1024):F2} MB";

            if (bytes < 1024L * 1024 * 1024 * 1024)
                return $"{bytes / (1024.0 * 1024 * 1024):F2} GB";

            return $"{bytes / (1024.0 * 1024 * 1024 * 1024):F2} TB";
        }

        private static string Enc(string? input)
        {
            return WebUtility.HtmlEncode(input ?? string.Empty);
        }
    }
}