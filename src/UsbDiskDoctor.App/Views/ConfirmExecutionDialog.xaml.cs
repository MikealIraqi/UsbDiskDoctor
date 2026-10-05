using System.Windows;
using UsbDiskDoctor.App.ViewModels;
using UsbDiskDoctor.Core.Models;

namespace UsbDiskDoctor.App.Views
{
    /// <summary>
    /// Dialog window for confirming repair action execution with risk-based token verification.
    /// </summary>
    public partial class ConfirmExecutionDialog : Window
    {
        public ConfirmExecutionDialog(RepairProposal proposal)
        {
            InitializeComponent();
            ViewModel = new ConfirmExecutionDialogViewModel(proposal);
            DataContext = ViewModel;
        }

        public ConfirmExecutionDialogViewModel ViewModel { get; }

        /// <summary>Token entered by user (available after DialogResult = true).</summary>
        public string ConfirmedToken => ViewModel.TokenInput;

        private void OnExecuteClicked(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }

        private void OnCancelClicked(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}