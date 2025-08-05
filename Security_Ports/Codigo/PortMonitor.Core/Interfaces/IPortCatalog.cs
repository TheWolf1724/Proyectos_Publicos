using PortMonitor.Core.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PortMonitor.Core.Interfaces
{
    /// <summary>
    /// Interfaz para el catálogo de información de puertos
    /// </summary>
    public interface IPortCatalog
    {
        /// <summary>
        /// Obtiene información sobre un puerto específico
        /// </summary>
        Task<PortInfo?> GetPortInfoAsync(int port, ProtocolType protocol);
        
        /// <summary>
        /// Busca puertos por nombre de servicio
        /// </summary>
        Task<IEnumerable<PortInfo>> SearchPortsByServiceAsync(string serviceName);
        
        /// <summary>
        /// Obtiene todos los puertos conocidos como maliciosos
        /// </summary>
        Task<IEnumerable<PortInfo>> GetMaliciousPortsAsync();
        
        /// <summary>
        /// Obtiene puertos por categoría
        /// </summary>
        Task<IEnumerable<PortInfo>> GetPortsByCategoryAsync(PortCategory category);
        
        /// <summary>
        /// Actualiza la base de datos de puertos
        /// </summary>
        Task UpdatePortDatabaseAsync();
        
        /// <summary>
        /// Analiza un evento de puerto y determina el nivel de riesgo
        /// </summary>
        Task<RiskLevel> AnalyzePortRiskAsync(PortEvent portEvent);
        
        /// <summary>
        /// Verifica si un puerto es conocido por estar asociado con malware
        /// </summary>
        Task<bool> IsKnownMalwarePortAsync(int port, ProtocolType protocol);
    }
}
