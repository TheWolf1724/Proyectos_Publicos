using PortMonitor.Core.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PortMonitor.Core.Interfaces
{
    /// <summary>
    /// Interfaz para el repositorio de datos
    /// </summary>
    public interface IDataRepository
    {
        // Gestión de eventos de puerto
        Task<int> SavePortEventAsync(PortEvent portEvent);
        Task<PortEvent?> GetPortEventAsync(int id);
        Task<IEnumerable<PortEvent>> GetPortEventsAsync(int skip = 0, int take = 100);
        Task<IEnumerable<PortEvent>> GetPortEventsByProcessAsync(int processId);
        Task<bool> DeletePortEventAsync(int id);
        Task<bool> DeleteOldPortEventsAsync(int daysOld);
        
        // Gestión de reglas de firewall
        Task<int> SaveFirewallRuleAsync(FirewallRule rule);
        Task<FirewallRule?> GetFirewallRuleAsync(int id);
        Task<IEnumerable<FirewallRule>> GetFirewallRulesAsync();
        Task<IEnumerable<FirewallRule>> GetActiveFirewallRulesAsync();
        Task<bool> UpdateFirewallRuleAsync(FirewallRule rule);
        Task<bool> DeleteFirewallRuleAsync(int id);
        Task<bool> DeleteAllUserCreatedRulesAsync();
        
        // Gestión de configuración
        Task<AppConfiguration> GetConfigurationAsync();
        Task<bool> SaveConfigurationAsync(AppConfiguration configuration);
        
        // Gestión de información de puertos
        Task<bool> SavePortInfoAsync(PortInfo portInfo);
        Task<PortInfo?> GetPortInfoAsync(int port, ProtocolType protocol);
        Task<IEnumerable<PortInfo>> GetAllPortInfoAsync();
        Task<bool> UpdatePortInfoDatabaseAsync(IEnumerable<PortInfo> portInfos);
        
        // Utilidades
        Task<bool> InitializeDatabaseAsync();
        Task<bool> BackupDataAsync(string backupPath);
        Task<bool> RestoreDataAsync(string backupPath);
        Task<long> GetDatabaseSizeAsync();
        Task<bool> OptimizeDatabaseAsync();
    }
}
