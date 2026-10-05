using System;
using CommunityToolkit.Mvvm.ComponentModel;
using UsbDiskDoctor.Core.Enums;
using UsbDiskDoctor.Core.Models;

namespace UsbDiskDoctor.App.ViewModels
{
    /// <summary>
    /// View model for the execution confirmation dialog, handling risk assessment
    /// and token-based confirmation for dangerous operations.
    /// </summary>
    public sealed partial class ConfirmExecutionDialogViewModel : ObservableObject
    {
        [ObservableProperty]
        private RepairProposal _proposal;

        [ObservableProperty]
        private string _tokenInput = string.Empty;

        public ConfirmExecutionDialogViewModel(RepairProposal proposal)
        {
            _proposal = proposal ?? throw new ArgumentNullException(nameof(proposal));
        }

        public string ActionCodeText => Proposal.ActionCode;

        public string DescriptionText => Proposal.DescriptionAr;

        public string CommandPreviewText => Proposal.CommandPreview ?? "لا يوجد أمر.";

        public bool RequiresToken => Proposal.RequiresConfirmation;

        public string RequiredTokenText =>
            Proposal.RiskLevel == RiskLevel.Dangerous ? "FORMAT" : "CONFIRM";

        public string RiskLevelTextAr => Proposal.RiskLevel switch
        {
            RiskLevel.Safe => "آمن",
            RiskLevel.Medium => "متوسط",
            RiskLevel.Dangerous => "خطر",
            _ => "غير معروف"
        };

        public string WarningTextAr => Proposal.RiskLevel switch
        {
            RiskLevel.Dangerous => "⚠️ تحذير شديد: هذه العملية قد تسبب فقدان بيانات!",
            RiskLevel.Medium => "⚠️ عملية تحتاج تأكيداً.",
            _ => "عملية آمنة."
        };

        /// <summary>Hex color for the risk level text (bound directly to Foreground).</summary>
        public string RiskForegroundHex => Proposal.RiskLevel switch
        {
            RiskLevel.Dangerous => "#991b1b",
            RiskLevel.Medium => "#f59e0b",
            RiskLevel.Safe => "#10b981",
            _ => "#374151"
        };

        /// <summary>Hex color for the warning text.</summary>
        public string WarningForegroundHex => Proposal.RiskLevel switch
        {
            RiskLevel.Dangerous => "#991b1b",
            _ => "#4b5563"
        };

        public bool CanExecute =>
            !RequiresToken || string.Equals(TokenInput, RequiredTokenText, StringComparison.Ordinal);

        partial void OnProposalChanged(RepairProposal value)
        {
            OnPropertyChanged(nameof(ActionCodeText));
            OnPropertyChanged(nameof(DescriptionText));
            OnPropertyChanged(nameof(CommandPreviewText));
            OnPropertyChanged(nameof(RequiresToken));
            OnPropertyChanged(nameof(RequiredTokenText));
            OnPropertyChanged(nameof(RiskLevelTextAr));
            OnPropertyChanged(nameof(WarningTextAr));
            OnPropertyChanged(nameof(RiskForegroundHex));
            OnPropertyChanged(nameof(WarningForegroundHex));
            OnPropertyChanged(nameof(CanExecute));
        }

        partial void OnTokenInputChanged(string value)
        {
            OnPropertyChanged(nameof(CanExecute));
        }
    }
}