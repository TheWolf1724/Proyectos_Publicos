using PortMonitor.Core.Models;
using System.Threading.Tasks;

namespace PortMonitor.Core.Interfaces
{
    /// <summary>
    /// Interfaz para el sistema de notificaciones toast
    /// </summary>
    public interface INotificationService
    {
        /// <summary>
        /// Muestra una notificación toast para un evento de puerto
        /// </summary>
        Task ShowPortEventNotificationAsync(PortEvent portEvent, PortInfo? portInfo = null);
        
        /// <summary>
        /// Muestra una notificación de información general
        /// </summary>
        Task ShowInfoNotificationAsync(string title, string message);
        
        /// <summary>
        /// Muestra una notificación de advertencia
        /// </summary>
        Task ShowWarningNotificationAsync(string title, string message);
        
        /// <summary>
        /// Muestra una notificación de error
        /// </summary>
        Task ShowErrorNotificationAsync(string title, string message);
        
        /// <summary>
        /// Verifica si las notificaciones están habilitadas en el sistema
        /// </summary>
        Task<bool> AreNotificationsEnabledAsync();
        
        /// <summary>
        /// Evento que se dispara cuando el usuario interactúa con una notificación
        /// </summary>
        event System.Action<NotificationAction> NotificationActionReceived;
    }

    /// <summary>
    /// Acción realizada desde una notificación
    /// </summary>
    public class NotificationAction
    {
        public int PortEventId { get; set; }
        public NotificationActionType ActionType { get; set; }
        public string? AdditionalData { get; set; }
    }

    /// <summary>
    /// Tipo de acción de notificación
    /// </summary>
    public enum NotificationActionType
    {
        Allow,
        Block,
        ShowDetails,
        Dismiss
    }
}
