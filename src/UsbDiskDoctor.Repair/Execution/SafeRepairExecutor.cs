using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Serilog;
using UsbDiskDoctor.Core.Enums;
using UsbDiskDoctor.Core.Models;

namespace UsbDiskDoctor.Repair.Execution
{
    /// <summary>
    /// Secure implementation of <see cref="IRepairExecutor"/>.
    /// Only whitelisted actions with the correct confirmation token may run.
    /// </summary>
    public sealed class SafeRepairExecutor : IRepairExecutor
    {
        private static readonly ILogger _log = Log.ForContext<SafeRepairExecutor>();

        private static readonly string ChkdskPath =
            Path.Combine(
                Environment.SystemDirectory ?? @"C:\Windows\System32",
                "chkdsk.exe");

        private static readonly IReadOnlyDictionary<string, ActionSpec> Whitelist =
            new Dictionary<string, ActionSpec>(StringComparer.Ordinal)
            {
                ["OPERATIONAL_STATUS_ERROR"] = new ActionSpec
                {
                    ActionCode = "OPERATIONAL_STATUS_ERROR",
                    RequiredRiskLevel = RiskLevel.Safe,
                    RequiredConfirmationToken = null,
                    Executable = null,
                    ArgumentsTemplate = null,
                    RequiresDriveLetter = false,
                    MessageAr = "لا يوجد أمر آلي. اتبع الإجراءات المقترحة يدوياً.",
                    MessageEn = "No automated command. Follow the recommended manual steps."
                },
                ["OPERATIONAL_STATUS_DEGRADED"] = new ActionSpec
                {
                    ActionCode = "OPERATIONAL_STATUS_DEGRADED",
                    RequiredRiskLevel = RiskLevel.Safe,
                    RequiredConfirmationToken = null,
                    Executable = null,
                    ArgumentsTemplate = null,
                    RequiresDriveLetter = false,
                    MessageAr = "لا يوجد أمر آلي. راقب الجهاز يدوياً.",
                    MessageEn = "No automated command. Monitor the device manually."
                },
                ["FILESYSTEM_CHECK_FAILED"] = new ActionSpec
                {
                    ActionCode = "FILESYSTEM_CHECK_FAILED",
                    RequiredRiskLevel = RiskLevel.Medium,
                    RequiredConfirmationToken = "CONFIRM",
                    Executable = null,
                    ArgumentsTemplate = null,
                    RequiresDriveLetter = false,
                    MessageAr = "الإجراء المقترح يدوي: أعد توصيل الجهاز أو جرّب منفذاً آخر.",
                    MessageEn = "Manual action: reseat the device or try another port."
                },
                ["DIRTY_BIT_SET"] = new ActionSpec
                {
                    ActionCode = "DIRTY_BIT_SET",
                    RequiredRiskLevel = RiskLevel.Medium,
                    RequiredConfirmationToken = "CONFIRM",
                    Executable = ChkdskPath,
                    ArgumentsTemplate = "{0} /scan",
                    RequiresDriveLetter = true,
                    Timeout = TimeSpan.FromMinutes(30),
                    MessageAr = "سيتم تشغيل chkdsk /scan (قراءة فقط).",
                    MessageEn = "Running chkdsk /scan (read-only)."
                }
            };

        private readonly ICommandRunner _commandRunner;

        public SafeRepairExecutor(ICommandRunner commandRunner)
        {
            _commandRunner = commandRunner ?? throw new ArgumentNullException(nameof(commandRunner));
        }

        public async Task<RepairExecutionResult> ExecuteAsync(
            RepairProposal proposal,
            string? confirmationToken,
            string? targetDriveLetter,
            CancellationToken cancellationToken = default)
        {
            if (proposal == null)
            {
                throw new ArgumentNullException(nameof(proposal));
            }

            // Step 1: Whitelist lookup
            if (!Whitelist.TryGetValue(proposal.ActionCode ?? string.Empty, out var actionSpec))
            {
                _log.Warning("Rejected non-whitelisted action: {ActionCode}", proposal.ActionCode);
                return Reject(
                    "NOT_WHITELISTED",
                    "الإجراء غير مسموح به في قائمة الأوامر الآمنة.",
                    "Action is not in the safe-execution whitelist.");
            }

            // Step 2: Risk level must match exactly
            if (proposal.RiskLevel != actionSpec.RequiredRiskLevel)
            {
                _log.Warning(
                    "Risk level mismatch for {ActionCode}: proposal={ProposalRisk}, whitelist={WhitelistRisk}",
                    proposal.ActionCode, proposal.RiskLevel, actionSpec.RequiredRiskLevel);
                return Reject(
                    "RISK_MISMATCH",
                    "مستوى الخطورة لا يطابق القائمة المسموحة.",
                    "Risk level does not match the whitelist.");
            }

            // Step 3: Confirmation token (exact, ordinal, case-sensitive)
            if (!string.IsNullOrEmpty(actionSpec.RequiredConfirmationToken))
            {
                if (!string.Equals(confirmationToken, actionSpec.RequiredConfirmationToken, StringComparison.Ordinal))
                {
                    _log.Information("Missing or incorrect confirmation token for {ActionCode}", proposal.ActionCode);
                    return Reject(
                        "MISSING_TOKEN",
                        $"يتطلب تأكيداً صريحاً بكتابة: {actionSpec.RequiredConfirmationToken}",
                        $"Explicit confirmation required: {actionSpec.RequiredConfirmationToken}");
                }
            }

            // Step 4: Informational-only actions (no executable) succeed without running anything.
            if (string.IsNullOrEmpty(actionSpec.Executable))
            {
                _log.Information("Informational action accepted: {ActionCode}", proposal.ActionCode);
                return new RepairExecutionResult
                {
                    Executed = true,
                    Succeeded = true,
                    MessageAr = actionSpec.MessageAr,
                    MessageEn = actionSpec.MessageEn
                };
            }

            // Step 5: Drive letter requirement
            if (actionSpec.RequiresDriveLetter && string.IsNullOrWhiteSpace(targetDriveLetter))
            {
                _log.Warning("Drive letter required but not supplied for {ActionCode}", proposal.ActionCode);
                return Reject(
                    "MISSING_DRIVE_LETTER",
                    "حرف القرص مطلوب لهذا الإجراء.",
                    "Drive letter is required for this action.");
            }

            // Step 6: Build arguments from whitelist template (never from CommandPreview)
            var arguments = actionSpec.ArgumentsTemplate!.Replace("{0}", targetDriveLetter ?? string.Empty);

            // Step 7: Execute
            _log.Information(
                "Executing whitelisted action {ActionCode} -> {Executable} {Arguments}",
                proposal.ActionCode, actionSpec.Executable, arguments);

            var commandResult = await _commandRunner.RunAsync(
                actionSpec.Executable!,
                arguments,
                actionSpec.Timeout,
                cancellationToken);

            var succeeded =
                commandResult.StartedSuccessfully
                && !commandResult.TimedOut
                && commandResult.ExitCode == 0;

            return new RepairExecutionResult
            {
                Executed = true,
                Succeeded = succeeded,
                CommandResult = commandResult,
                MessageAr = succeeded ? "تم التنفيذ بنجاح." : "فشل التنفيذ.",
                MessageEn = succeeded ? "Execution succeeded." : "Execution failed."
            };
        }

        private static RepairExecutionResult Reject(string code, string messageAr, string messageEn)
        {
            return new RepairExecutionResult
            {
                Executed = false,
                Succeeded = false,
                RejectionCode = code,
                MessageAr = messageAr,
                MessageEn = messageEn
            };
        }
    }
}