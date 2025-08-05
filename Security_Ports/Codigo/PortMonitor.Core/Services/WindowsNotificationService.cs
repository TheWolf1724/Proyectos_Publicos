using PortMonitor.Core.Interfaces;
using PortMonitor.Core.Models;
using System.Threading.Tasks;

namespace PortMonitor.Core.Services
{
    public class WindowsNotificationService : INotificationService
    {
        public event System.Action<PortMonitor.Core.Interfaces.NotificationAction>? NotificationActionReceived;

        public Task ShowPortEventNotificationAsync(PortEvent portEvent, PortInfo? portInfo = null)
        {
            // Implementación mínima
            return Task.CompletedTask;
        }

        public Task ShowInfoNotificationAsync(string title, string message)
        {
            // Implementación mínima
            return Task.CompletedTask;
        }

        public Task ShowWarningNotificationAsync(string title, string message)
        {
            // Implementación mínima
            return Task.CompletedTask;
        }

        public Task ShowErrorNotificationAsync(string title, string message)
        {
            // Implementación mínima
            return Task.CompletedTask;
        }

        public Task<bool> AreNotificationsEnabledAsync() => Task.FromResult(true);
    }
}
