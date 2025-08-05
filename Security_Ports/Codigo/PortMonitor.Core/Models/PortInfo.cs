namespace PortMonitor.Core.Models
{
    /// <summary>
    /// Información sobre un puerto conocido
    /// </summary>
    public class PortInfo
    {
        public int Port { get; set; }
        public ProtocolType Protocol { get; set; }
        public string ServiceName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public RiskLevel RiskLevel { get; set; }
        public PortCategory Category { get; set; }
        public bool IsWellKnown { get; set; }
        public string[]? CommonProcesses { get; set; }
        public string[]? MalwareAssociations { get; set; }
        public string? SecurityNotes { get; set; }
    }

    /// <summary>
    /// Categoría del puerto
    /// </summary>
    public enum PortCategory
    {
        System,
        Web,
        Mail,
        Database,
        FileTransfer,
        RemoteAccess,
        Gaming,
        Messaging,
        Security,
        Malware,
        Unknown
    }
}
