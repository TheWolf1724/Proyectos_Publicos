using PortMonitor.Core.Configuration;
using System.Threading.Tasks;

namespace PortMonitor.Core.Interfaces
{
    /// <summary>
    /// Servicio para gestionar la configuración de la aplicación
    /// </summary>
    public interface IConfigurationService
    {
        /// <summary>
        /// Obtiene la configuración actual
        /// </summary>
        Task<AppConfiguration> GetConfigurationAsync();
        
        /// <summary>
        /// Guarda la configuración
        /// </summary>
        Task SaveConfigurationAsync(AppConfiguration configuration);
        
        /// <summary>
        /// Restaura la configuración por defecto
        /// </summary>
        Task ResetToDefaultAsync();
        
        /// <summary>
        /// Valida la configuración
        /// </summary>
        bool ValidateConfiguration(AppConfiguration configuration);
    }
}
