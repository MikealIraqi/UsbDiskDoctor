using System.Windows;
using UsbDiskDoctor.App.Localization;

namespace UsbDiskDoctor.App.Views;

/// <summary>
/// About dialog showing product description and features.
/// </summary>
public partial class AboutWindow : Window
{
    public AboutWindow()
    {
        InitializeComponent();
        this.FlowDirection = LocalizationService.CurrentFlowDirection;
    }

    private void OnCloseClicked(object sender, RoutedEventArgs e)
    {
        Close();
    }
}
