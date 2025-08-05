using System.Windows;
using PortMonitor.Core.Models;
using PortMonitor.GUI.ViewModels;

namespace PortMonitor.GUI.Views;

public partial class PortEventDetailsWindowFixed : Window
{
    public PortEventDetailsWindowFixed(PortEvent portEvent)
    {
        InitializeComponent();
        DataContext = new PortEventDetailsViewModel(portEvent);
    }
}
