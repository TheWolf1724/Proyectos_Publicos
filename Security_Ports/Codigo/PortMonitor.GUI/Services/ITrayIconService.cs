using System;

namespace PortMonitor.GUI.Services
{
    /// <summary>
    /// Interfaz para el servicio del icono de la bandeja del sistema
    /// </summary>
    public interface ITrayIconService
    {
        /// <summary>
        /// Inicializa el icono de la bandeja del sistema
        /// </summary>
        void Initialize();
        
        /// <summary>
        /// Muestra el icono en la bandeja del sistema
        /// </summary>
        void ShowTrayIcon();
        
        /// <summary>
        /// Oculta el icono de la bandeja del sistema
        /// </summary>
        void HideTrayIcon();
        
        /// <summary>
        /// Muestra una notificación balloon
        /// </summary>
        void ShowBalloonTip(string title, string text, int timeout = 5000);
        
        /// <summary>
        /// Evento que se dispara cuando se hace clic en el icono
        /// </summary>
        event Action TrayIconClicked;
        
        /// <summary>
        /// Libera los recursos
        /// </summary>
        void Dispose();
    }
}
