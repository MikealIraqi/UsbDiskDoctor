using System.ComponentModel;
using System.Windows;
using Microsoft.Web.WebView2.Core;
using UsbDiskDoctor.App.ViewModels;

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
                // WebView2 Runtime may be missing. Fall back to hint text.
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
    }
}