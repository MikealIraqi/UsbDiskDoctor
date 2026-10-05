using System.ComponentModel;
using System.Linq;
using System.Windows;
using UsbDiskDoctor.App.ViewModels;
using UsbDiskDoctor.App.Views;
using UsbDiskDoctor.Core.Models;

namespace UsbDiskDoctor.App
{
    /// <summary>
    /// Main window for the UsbDiskDoctor application.
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly MainViewModel _viewModel;
        private bool _webViewInitialized;

        public MainWindow(MainViewModel viewModel)
        {
            InitializeComponent();

            _viewModel = viewModel ?? throw new System.ArgumentNullException(nameof(viewModel));
            DataContext = _viewModel;

            Loaded += OnWindowLoaded;
            _viewModel.ReportViewModel.PropertyChanged += OnReportViewModelPropertyChanged;
            _viewModel.ProposalExecuteRequested += OnProposalExecuteRequested;
        }

        private async void OnWindowLoaded(object sender, RoutedEventArgs e)
        {
            try
            {
                await ReportWebView.EnsureCoreWebView2Async();
                _webViewInitialized = true;
                ReportWebView.CoreWebView2.Settings.AreDevToolsEnabled = false;
                ReportWebView.CoreWebView2.Settings.AreDefaultContextMenusEnabled = false;
                ReportWebView.CoreWebView2.Settings.IsStatusBarEnabled = false;
                UpdateReportView();
            }
            catch (System.Exception)
            {
                _webViewInitialized = false;
                ReportEmptyHint.Text = "محرك WebView2 غير متوفر على هذا النظام. ثبّت WebView2 Runtime لعرض التقرير.";
            }
        }

        private void OnReportViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(ReportViewModel.HtmlContent))
            {
                UpdateReportView();
            }
        }

        private void UpdateReportView()
        {
            if (!_webViewInitialized || ReportWebView.CoreWebView2 == null)
            {
                return;
            }

            var html = _viewModel.ReportViewModel.HtmlContent;
            if (string.IsNullOrEmpty(html))
            {
                ReportEmptyHint.Visibility = Visibility.Visible;
                ReportWebView.Visibility = Visibility.Collapsed;
                return;
            }

            ReportWebView.NavigateToString(html);
            ReportEmptyHint.Visibility = Visibility.Collapsed;
            ReportWebView.Visibility = Visibility.Visible;
        }

        private async void OnProposalExecuteRequested(RepairProposal proposal)
        {
            var dialog = new ConfirmExecutionDialog(proposal)
            {
                Owner = this
            };

            if (dialog.ShowDialog() == true)
            {
                var targetDriveLetter = _viewModel.SelectedDevice?.Volumes
                    .Where(v => v.IsMounted && !string.IsNullOrEmpty(v.DriveLetter))
                    .Select(v => v.DriveLetter)
                    .FirstOrDefault();

                await _viewModel.ExecuteProposalAsync(proposal, dialog.ConfirmedToken, targetDriveLetter);
            }
        }
    }
}