using System;
using System.Collections.Generic;
using Serilog;
using UsbDiskDoctor.Core.Enums;
using UsbDiskDoctor.Core.Models;
using UsbDiskDoctor.Diagnostics.Models;
using UsbDiskDoctor.Knowledge.Services;

namespace UsbDiskDoctor.Repair.Planning
{
    /// <summary>
    /// Plans repair actions by analyzing diagnostic results and consulting the knowledge base.
    /// </summary>
    public sealed class RepairPlanner : IRepairPlanner
    {
        private static readonly ILogger _log = Log.ForContext<RepairPlanner>();
        private readonly IKnowledgeService _knowledgeService;

        public RepairPlanner(IKnowledgeService knowledgeService)
        {
            _knowledgeService = knowledgeService ?? throw new ArgumentNullException(nameof(knowledgeService));
        }

        public IReadOnlyList<RepairProposal> Plan(DeviceDiagnosticReport report)
        {
            if (report == null)
            {
                throw new ArgumentNullException(nameof(report));
            }

            if (report.HealthEvaluation == null)
            {
                return Array.Empty<RepairProposal>();
            }

            var diagnostics = report.HealthEvaluation.Diagnostics;
            if (diagnostics == null || diagnostics.Count == 0)
            {
                return Array.Empty<RepairProposal>();
            }

            var proposals = new List<RepairProposal>();

            foreach (var diagnosticItem in diagnostics)
            {
                if (diagnosticItem == null)
                {
                    continue;
                }

                var article = _knowledgeService.GetArticle(diagnosticItem.Code);

                var risk = ClassifyRisk(diagnosticItem.Code, article);

                var requiresConfirmation = 
                    (risk == RiskLevel.Dangerous) || 
                    (article?.RequiresConfirmation == true);

                var commandPreview = BuildCommandPreview(diagnosticItem.Code, risk);

                string descriptionAr;
                string descriptionEn;

                if (article != null)
                {
                    descriptionAr = article.TitleAr;
                    descriptionEn = article.TitleEn;
                }
                else
                {
                    descriptionAr = diagnosticItem.TitleAr;
                    descriptionEn = diagnosticItem.TitleEn;
                }

                var proposal = new RepairProposal
                {
                    DiagnosticResultId = diagnosticItem.Id,
                    ActionCode = diagnosticItem.Code,
                    RiskLevel = risk,
                    RequiresConfirmation = requiresConfirmation,
                    DescriptionAr = descriptionAr,
                    DescriptionEn = descriptionEn,
                    CommandPreview = commandPreview,
                    IsExecutableNow = false
                };

                proposals.Add(proposal);
            }

            _log.Information("Built {Count} repair proposals.", proposals.Count);
            return proposals;
        }

        private static RiskLevel ClassifyRisk(string code, KnowledgeArticle? article)
        {
            if (string.IsNullOrWhiteSpace(code))
            {
                return RiskLevel.Safe;
            }

            var upperCode = code.ToUpperInvariant();

            if (upperCode.Contains("SMART_PREDICT_FAILURE"))
            {
                return RiskLevel.Dangerous;
            }

            if (upperCode.Contains("RAW_FILESYSTEM"))
            {
                return RiskLevel.Dangerous;
            }

            if (upperCode.Contains("OPERATIONAL_STATUS_"))
            {
                return RiskLevel.Safe;
            }

            if (upperCode.Contains("FILESYSTEM_CHECK_FAILED"))
            {
                return RiskLevel.Medium;
            }

            if (upperCode.Contains("DIRTY_BIT"))
            {
                return RiskLevel.Medium;
            }

            if (article?.RequiresConfirmation == true)
            {
                return RiskLevel.Dangerous;
            }

            return RiskLevel.Safe;
        }

        private static string? BuildCommandPreview(string code, RiskLevel risk)
        {
            if (string.IsNullOrWhiteSpace(code))
            {
                return null;
            }

            var upperCode = code.ToUpperInvariant();

            return upperCode switch
            {
                "SMART_PREDICT_FAILURE" => 
                    "Backup data before any write operation. (no auto-execution)",
                "RAW_FILESYSTEM" => 
                    "Do NOT format. Consider data recovery tools. (no auto-execution)",
                "OPERATIONAL_STATUS_ERROR" => 
                    "Reseat device, try another USB port, check cables.",
                "OPERATIONAL_STATUS_DEGRADED" => 
                    "Reduce load, verify power supply, monitor.",
                "FILESYSTEM_CHECK_FAILED" => 
                    "chkdsk <drive> /scan   (read-only, requires manual review)",
                "DIRTY_BIT_SET" => 
                    "chkdsk <drive> /scan   (read-only scan first)",
                _ => null
            };
        }
    }
}