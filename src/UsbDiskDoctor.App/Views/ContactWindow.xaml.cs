using System.Diagnostics;
using System.Windows;

namespace UsbDiskDoctor.App.Views;

/// <summary>
/// Contact dialog showing programmer information with WhatsApp, email, and phone actions.
/// </summary>
public partial class ContactWindow : Window
{
    public ContactWindow()
    {
        InitializeComponent();
    }

    private void OnWhatsAppClicked(object sender, RoutedEventArgs e)
    {
        OpenUrl("https://wa.me/9647730393399");
    }

    private void OnEmailClicked(object sender, RoutedEventArgs e)
    {
        OpenUrl("mailto:tearscantstop@gmail.com?subject=UsbDiskDoctor");
    }

    private void OnCopyPhoneClicked(object sender, RoutedEventArgs e)
    {
        try
        {
            Clipboard.SetText("009647730393399");
            MessageBox.Show(this, "تم نسخ رقم الهاتف إلى الحافظة.", "نسخ",
                MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch
        {
            // Clipboard can fail; ignore silently.
        }
    }

    private void OnCloseClicked(object sender, RoutedEventArgs e)
    {
        Close();
    }

    private static void OpenUrl(string url)
    {
        try
        {
            Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
        }
        catch
        {
            // Ignore — no handler registered.
        }
    }
}
