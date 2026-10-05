using CommunityToolkit.Mvvm.ComponentModel;

namespace UsbDiskDoctor.App.ViewModels
{
    /// <summary>
    /// View model for displaying generated diagnostic reports in HTML format.
    /// </summary>
    public sealed partial class ReportViewModel : ObservableObject
    {
        [ObservableProperty]
        private string _htmlContent = string.Empty;

        /// <summary>
        /// True when there is HTML content to display.
        /// </summary>
        public bool HasContent => !string.IsNullOrEmpty(HtmlContent);

        /// <summary>
        /// Called by generated code when HtmlContent changes.
        /// </summary>
        partial void OnHtmlContentChanged(string value)
        {
            OnPropertyChanged(nameof(HasContent));
        }

        /// <summary>
        /// Replaces current report content.
        /// </summary>
        public void SetContent(string html)
        {
            HtmlContent = html ?? string.Empty;
        }

        /// <summary>
        /// Clears current report content.
        /// </summary>
        public void Clear()
        {
            HtmlContent = string.Empty;
        }
    }
}