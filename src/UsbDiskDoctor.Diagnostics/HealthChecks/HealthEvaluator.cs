using System;
using System.Collections.Generic;
using UsbDiskDoctor.Core.Enums;
using UsbDiskDoctor.Core.Models;
using UsbDiskDoctor.Diagnostics.Models;

namespace UsbDiskDoctor.Diagnostics.HealthChecks
{
    /// <summary>
    /// Evaluates device health by analyzing SMART data, operational status, and file system checks.
    /// </summary>
    public sealed class HealthEvaluator : IHealthEvaluator
    {
        public HealthEvaluationResult Evaluate(
            DeviceSummary device,
            SmartInfo smartInfo,
            IReadOnlyList<FileSystemCheckResult> fileSystemResults)
        {
            if (device == null)
            {
                throw new ArgumentNullException(nameof(device));
            }

            var effectiveSmartInfo = smartInfo ?? new SmartInfo { Available = false, PredictFailure = false };
            var effectiveFileSystemResults = fileSystemResults ?? Array.Empty<FileSystemCheckResult>();

            var diagnosticsList = new List<DiagnosticResult>();
            var highestSeverity = Severity.Info;

            // SMART failure prediction check
            if (effectiveSmartInfo.PredictFailure)
            {
                var smartEvidence = new List<string>
                {
                    "SMART PredictFailure=true"
                };

                if (!string.IsNullOrWhiteSpace(effectiveSmartInfo.Reason))
                {
                    smartEvidence.Add($"Reason: {effectiveSmartInfo.Reason}");
                }

                var smartDiagnostic = new DiagnosticResult
                {
                    DeviceId = device.DeviceId,
                    Severity = Severity.Critical,
                    Code = "SMART_PREDICT_FAILURE",
                    TitleAr = "تحذير فشل SMART",
                    TitleEn = "SMART Failure Predicted",
                    DescriptionAr = "القرص يبلغ عن احتمال فشل وشيك. يجب نسخ البيانات فوراً.",
                    DescriptionEn = "The disk reports an imminent failure risk. Back up data immediately.",
                    Evidence = smartEvidence,
                    RecommendedAction = "أوقف الاستخدام، أنشئ صورة للقرص إن أمكن، واستشر مختصاً."
                };

                diagnosticsList.Add(smartDiagnostic);
                highestSeverity = MaxSeverity(highestSeverity, Severity.Critical);
            }

            // Operational status check
            if (device.OperationalStatus == OperationalStatus.Error ||
                device.OperationalStatus == OperationalStatus.PredictingFailure)
            {
                var operationalDiagnostic = new DiagnosticResult
                {
                    DeviceId = device.DeviceId,
                    Severity = Severity.Critical,
                    Code = "OPERATIONAL_STATUS_ERROR",
                    TitleAr = "حالة تشغيلية حرجة",
                    TitleEn = "Critical Operational Status",
                    DescriptionAr = "النظام يبلغ عن حالة تشغيلية حرجة للجهاز.",
                    DescriptionEn = "The system reports a critical operational status for the device.",
                    Evidence = new List<string> { $"OperationalStatus={device.OperationalStatus}" },
                    RecommendedAction = "افحص الجهاز واستبدله إذا لزم الأمر."
                };

                diagnosticsList.Add(operationalDiagnostic);
                highestSeverity = MaxSeverity(highestSeverity, Severity.Critical);
            }
            else if (device.OperationalStatus == OperationalStatus.Degraded ||
                     device.OperationalStatus == OperationalStatus.Stressed)
            {
                var degradedDiagnostic = new DiagnosticResult
                {
                    DeviceId = device.DeviceId,
                    Severity = Severity.Warning,
                    Code = "OPERATIONAL_STATUS_DEGRADED",
                    TitleAr = "حالة تشغيلية متدهورة",
                    TitleEn = "Degraded Operational Status",
                    DescriptionAr = "الجهاز يعمل بحالة متدهورة أو تحت ضغط.",
                    DescriptionEn = "The device is operating in a degraded or stressed state.",
                    Evidence = new List<string> { $"OperationalStatus={device.OperationalStatus}" },
                    RecommendedAction = "راقب الجهاز وفكر في استبداله."
                };

                diagnosticsList.Add(degradedDiagnostic);
                highestSeverity = MaxSeverity(highestSeverity, Severity.Warning);
            }

            // Device size zero check (hardware failure indicator)
            if (device.SizeBytes == 0)
            {
                var zeroSizeDiagnostic = new DiagnosticResult
                {
                    DeviceId = device.DeviceId,
                    Severity = Severity.Critical,
                    Code = "DEVICE_SIZE_ZERO",
                    TitleAr = "حجم الجهاز غير قابل للقراءة",
                    TitleEn = "Device Size Unreadable",
                    DescriptionAr = "النظام يُبلغ عن حجم صفر. قد يعني عطلاً هاردويرياً، أو مشكلة في الاتصال، أو وحدة تخزين تالفة.",
                    DescriptionEn = "The system reports zero size. This may indicate a hardware failure, connection problem, or damaged storage.",
                    Evidence = new List<string> { "SizeBytes=0" },
                    RecommendedAction = "تحقق من الكابل والمنفذ. إن استمرت المشكلة، الجهاز معطوب وقد يحتاج استبدالاً."
                };

                diagnosticsList.Add(zeroSizeDiagnostic);
                highestSeverity = MaxSeverity(highestSeverity, Severity.Critical);
            }
            // No volumes detected (with non-zero size)
            else if (device.SizeBytes > 0 && (device.Volumes == null || device.Volumes.Count == 0))
            {
                var noVolumesDiagnostic = new DiagnosticResult
                {
                    DeviceId = device.DeviceId,
                    Severity = Severity.Warning,
                    Code = "NO_VOLUMES_DETECTED",
                    TitleAr = "لا توجد فولومات مكتشفة",
                    TitleEn = "No Volumes Detected",
                    DescriptionAr = "لم يتم العثور على فولومات مقروءة على الجهاز. قد يكون غير مهيأ أو نظام الملفات تالف.",
                    DescriptionEn = "No readable volumes were found on the device. It may be unformatted or have a corrupt file system.",
                    Evidence = new List<string> { $"SizeBytes={device.SizeBytes}", "Volumes=0" },
                    RecommendedAction = "افحص الجهاز في Disk Management. لا تعمل فورمات قبل التأكد من البيانات."
                };

                diagnosticsList.Add(noVolumesDiagnostic);
                highestSeverity = MaxSeverity(highestSeverity, Severity.Warning);
            }

            // File system checks for each volume
            foreach (var volumeResult in effectiveFileSystemResults)
            {
                if (volumeResult.IsRaw)
                {
                    var rawDiagnostic = new DiagnosticResult
                    {
                        DeviceId = device.DeviceId,
                        Severity = Severity.Critical,
                        Code = "RAW_FILESYSTEM",
                        TitleAr = "نظام ملفات تالف (RAW)",
                        TitleEn = "RAW File System",
                        DescriptionAr = "الفوليوم لا يحتوي نظام ملفات معروف. تجنب الفورمات قبل محاولة الاستعادة.",
                        DescriptionEn = "The volume does not contain a recognized file system. Avoid formatting before attempting recovery.",
                        Evidence = new List<string> { $"DriveLetter={volumeResult.DriveLetter}" },
                        RecommendedAction = "لا تعمل فورمات. حاول استعادة الملفات أولاً."
                    };

                    diagnosticsList.Add(rawDiagnostic);
                    highestSeverity = MaxSeverity(highestSeverity, Severity.Critical);
                }
                else if (!volumeResult.CheckSucceeded)
                {
                    var failedCheckDiagnostic = new DiagnosticResult
                    {
                        DeviceId = device.DeviceId,
                        Severity = Severity.Warning,
                        Code = "FILESYSTEM_CHECK_FAILED",
                        TitleAr = "فشل فحص نظام الملفات",
                        TitleEn = "File System Check Failed",
                        DescriptionAr = "لم ينجح فحص نظام الملفات لهذا الفوليوم.",
                        DescriptionEn = "The file system check failed for this volume.",
                        Evidence = new List<string>
                        {
                            $"DriveLetter={volumeResult.DriveLetter}",
                            $"Error={volumeResult.ErrorMessage}"
                        },
                        RecommendedAction = "حاول فحص الفوليوم مرة أخرى أو تحقق من الاتصال."
                    };

                    diagnosticsList.Add(failedCheckDiagnostic);
                    highestSeverity = MaxSeverity(highestSeverity, Severity.Warning);
                }
                else if (volumeResult.DirtyBitSet)
                {
                    var dirtyBitDiagnostic = new DiagnosticResult
                    {
                        DeviceId = device.DeviceId,
                        Severity = Severity.Warning,
                        Code = "DIRTY_BIT_SET",
                        TitleAr = "Dirty Bit مُفعّل",
                        TitleEn = "Dirty Bit Set",
                        DescriptionAr = "الفوليوم لم يُغلق بشكل صحيح في آخر استخدام. قد يحتوي على أخطاء.",
                        DescriptionEn = "The volume was not properly closed in the last use. It may contain errors.",
                        Evidence = new List<string> { $"DriveLetter={volumeResult.DriveLetter}" },
                        RecommendedAction = "شغّل فحص نظام الملفات (chkdsk) لإصلاح الأخطاء."
                    };

                    diagnosticsList.Add(dirtyBitDiagnostic);
                    highestSeverity = MaxSeverity(highestSeverity, Severity.Warning);
                }
            }

            // Calculate overall status
            HealthStatus overallStatus;
            if (highestSeverity == Severity.Critical)
            {
                overallStatus = HealthStatus.Critical;
            }
            else if (highestSeverity == Severity.Warning || highestSeverity == Severity.Error)
            {
                overallStatus = HealthStatus.Warning;
            }
            else if (device.OperationalStatus == OperationalStatus.Unknown)
            {
                overallStatus = HealthStatus.Unknown;
            }
            else
            {
                overallStatus = HealthStatus.Healthy;
            }

            // Build summaries
            var summaryAr = overallStatus switch
            {
                HealthStatus.Critical => "الجهاز يعاني من مشاكل حرجة. الإجراء الفوري مطلوب.",
                HealthStatus.Warning => "الجهاز يحتاج إلى انتباه. تفقد التفاصيل.",
                HealthStatus.Healthy => "الجهاز في حالة صحية جيدة.",
                _ => "لا توجد معلومات كافية لتقييم الحالة."
            };

            var summaryEn = overallStatus switch
            {
                HealthStatus.Critical => "The device is experiencing critical issues. Immediate action required.",
                HealthStatus.Warning => "The device needs attention. Review the details.",
                HealthStatus.Healthy => "The device is in good health.",
                _ => "Insufficient information to evaluate the status."
            };

            return new HealthEvaluationResult
            {
                OverallStatus = overallStatus,
                Diagnostics = diagnosticsList,
                SummaryAr = summaryAr,
                SummaryEn = summaryEn
            };
        }

        private static Severity MaxSeverity(Severity firstSeverity, Severity secondSeverity)
        {
            return (Severity)Math.Max((int)firstSeverity, (int)secondSeverity);
        }
    }
}