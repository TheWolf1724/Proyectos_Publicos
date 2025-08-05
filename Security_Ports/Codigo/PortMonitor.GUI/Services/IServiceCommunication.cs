using System;
using System.Threading.Tasks;

namespace PortMonitor.GUI.Services
{
    /// <summary>
    /// Interfaz para la comunicación con el servicio de Windows
    /// </summary>
    public interface IServiceCommunication
    {
        /// <summary>
        /// Verifica si el servicio está ejecutándose
        /// </summary>
        Task<bool> IsServiceRunningAsync();
        
        /// <summary>
        /// Inicia el servicio
        /// </summary>
        Task<bool> StartServiceAsync();
        
        /// <summary>
        /// Detiene el servicio
        /// </summary>
        Task<bool> StopServiceAsync();
        
        /// <summary>
        /// Reinicia el servicio
        /// </summary>
        Task<bool> RestartServiceAsync();
        
        /// <summary>
        /// Obtiene el estado del servicio
        /// </summary>
        Task<string> GetServiceStatusAsync();
    }
}
