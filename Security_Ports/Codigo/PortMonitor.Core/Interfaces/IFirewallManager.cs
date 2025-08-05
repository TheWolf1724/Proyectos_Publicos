using PortMonitor.Core.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PortMonitor.Core.Interfaces
{
    /// <summary>
    /// Interfaz para la gestión del firewall de Windows
    /// </summary>
    public interface IFirewallManager
    {
        /// <summary>
        /// Crea una nueva regla de firewall
        /// </summary>
        Task<bool> CreateRuleAsync(FirewallRule rule);
        
        /// <summary>
        /// Elimina una regla de firewall
        /// </summary>
        Task<bool> DeleteRuleAsync(int ruleId);
        
        /// <summary>
        /// Actualiza una regla de firewall existente
        /// </summary>
        Task<bool> UpdateRuleAsync(FirewallRule rule);
        
        /// <summary>
        /// Obtiene todas las reglas creadas por la aplicación
        /// </summary>
        Task<IEnumerable<FirewallRule>> GetApplicationRulesAsync();
        
        /// <summary>
        /// Habilita o deshabilita una regla
        /// </summary>
        Task<bool> SetRuleEnabledAsync(int ruleId, bool enabled);
        
        /// <summary>
        /// Permite rápidamente un proceso y puerto específico
        /// </summary>
        Task<bool> AllowProcessPortAsync(string processPath, int port, ProtocolType protocol);
        
        /// <summary>
        /// Bloquea rápidamente un proceso y puerto específico
        /// </summary>
        Task<bool> BlockProcessPortAsync(string processPath, int port, ProtocolType protocol);
        
        /// <summary>
        /// Verifica si el firewall de Windows está habilitado
        /// </summary>
        Task<bool> IsFirewallEnabledAsync();
        
        /// <summary>
        /// Restaura las reglas por defecto (elimina todas las reglas personalizadas)
        /// </summary>
        Task<bool> RestoreDefaultRulesAsync();
    }
}
