using System;
using System.Net;

namespace PortMonitor.Core.Models
{
    /// <summary>
    /// Representa un evento de apertura o cierre de puerto
    /// </summary>
    public class PortEvent
    {
        public int Id { get; set; }
        public DateTime Timestamp { get; set; }
        public int ProcessId { get; set; }
        public string ProcessName { get; set; } = string.Empty;
        public string ExecutablePath { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public IPAddress LocalIpAddress { get; set; } = IPAddress.Any;
        public int LocalPort { get; set; }
        public IPAddress? RemoteIpAddress { get; set; }
        public int? RemotePort { get; set; }
        public ProtocolType Protocol { get; set; }
        public PortEventType EventType { get; set; }
        public PortStatus Status { get; set; }
        public RiskLevel RiskLevel { get; set; }
        public string? Description { get; set; }
        public bool IsAllowed { get; set; }
    }

    /// <summary>
    /// Tipo de evento de puerto
    /// </summary>
    public enum PortEventType
    {
        Opened,
        Closed,
        Modified
    }

    /// <summary>
    /// Protocolo de red
    /// </summary>
    public enum ProtocolType
    {
        TCP,
        UDP
    }

    /// <summary>
    /// Estado del puerto
    /// </summary>
    public enum PortStatus
    {
        Listening,
        Established,
        TimeWait,
        CloseWait,
        FinWait1,
        FinWait2,
        SynSent,
        SynReceived,
        Closed
    }

    /// <summary>
    /// Nivel de riesgo del puerto
    /// </summary>
    public enum RiskLevel
    {
        Low,
        Medium,
        High,
        Critical
    }
}
