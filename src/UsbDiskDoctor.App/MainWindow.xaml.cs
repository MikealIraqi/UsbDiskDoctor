using System.Windows;
using UsbDiskDoctor.App.ViewModels;

namespace UsbDiskDoctor.App
{
    /// <summary>
    /// Main window for the UsbDiskDoctor application.
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow(MainViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
        }
    }
}