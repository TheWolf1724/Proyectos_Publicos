using PortMonitor.Core.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PortMonitor.Core.Interfaces
{
    /// <summary>
    /// Interfaz para el monitoreo de puertos en tiempo real
    /// </summary>
    public interface IPortMonitor
    {
        /// <summary>
        /// Inicia el monitoreo de puertos
        /// </summary>
        Task StartMonitoringAsync();
        
        /// <summary>
        /// Detiene el monitoreo de puertos
        /// </summary>
        Task StopMonitoringAsync();
        
        /// <summary>
        /// Obtiene el estado actual de todos los puertos
        /// </summary>
        Task<IEnumerable<PortEvent>> GetCurrentPortStatusAsync();
        
        /// <summary>
        /// Verifica si el monitoreo está activo
        /// </summary>
        bool IsMonitoring { get; }
        
        /// <summary>
        /// Evento que se dispara cuando se detecta un cambio en los puertos
        /// </summary>
        event System.Action<PortEvent> PortChanged;
    }
}
