using PortMonitor.Core.Models;
using CommunityToolkit.Mvvm.ComponentModel;

namespace PortMonitor.GUI.ViewModels;

public partial class PortEventDetailsViewModel : ObservableObject
{
    [ObservableProperty]
    private PortEvent _portEvent;

    public PortEventDetailsViewModel(PortEvent portEvent)
    {
        PortEvent = portEvent;
    }
}
