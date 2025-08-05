
using System;
using System.Net;

namespace PortMonitor.Core.Models
{
    /// <summary>
    /// Representa una regla de firewall para controlar el acceso a puertos
    /// </summary>
    public class FirewallRule
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime CreatedDate { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public bool IsEnabled { get; set; } = true;
        public FirewallRule Clone()
        {
            return (FirewallRule)this.MemberwiseClone();
        }
        
        // Configuración de la regla
        public RuleAction Action { get; set; }
        public RuleDirection Direction { get; set; }
        public RuleScope Scope { get; set; }
        
        // Criterios de coincidencia
        public string? ProcessPath { get; set; }
        public int? ProcessId { get; set; }
        public int? LocalPort { get; set; }
        public int? RemotePort { get; set; }
        public ProtocolType? Protocol { get; set; }
        public IPAddress? LocalIpAddress { get; set; }
        public IPAddress? RemoteIpAddress { get; set; }
        public string? IpRange { get; set; }
        
        // Metadatos
        public bool IsUserCreated { get; set; }
        public bool IsPersistent { get; set; } = true;
        public string? Tags { get; set; }
    }

    /// <summary>
    /// Acción de la regla de firewall
    /// </summary>
    public enum RuleAction
    {
        Allow,
        Block
    }

    /// <summary>
    /// Dirección de la regla de firewall
    /// </summary>
    public enum RuleDirection
    {
        Inbound,
        Outbound,
        Both
    }

    /// <summary>
    /// Alcance de la regla de firewall
    /// </summary>
    public enum RuleScope
    {
        ProcessAndPort,  // Solo proceso específico y puerto específico
        ProcessAllPorts, // Todo el proceso, todos sus puertos
        PortAllProcesses, // Todos los procesos en el puerto específico
        Custom          // Regla personalizada con criterios específicos
    }
}
