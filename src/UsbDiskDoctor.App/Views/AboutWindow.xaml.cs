using System.Windows;

namespace UsbDiskDoctor.App.Views;

/// <summary>
/// About dialog showing product description and features.
/// </summary>
public partial class AboutWindow : Window
{
    public AboutWindow()
    {
        InitializeComponent();
    }

    private void OnCloseClicked(object sender, RoutedEventArgs e)
    {
        Close();
    }
}
