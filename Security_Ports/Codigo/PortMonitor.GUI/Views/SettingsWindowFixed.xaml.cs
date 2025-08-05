using System.Windows;
using PortMonitor.GUI.ViewModels;

namespace PortMonitor.GUI.Views;

public partial class SettingsWindowFixed : Window
{
    public SettingsWindowFixed(SettingsViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}
