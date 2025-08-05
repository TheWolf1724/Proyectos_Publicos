using System;

namespace PortMonitor.Core.Models
{
    /// <summary>
    /// Configuración de la aplicación
    /// </summary>
    public class AppConfiguration
    {
        // Configuración de monitorización
        public bool EnableRealTimeMonitoring { get; set; } = true;
        public int MonitoringIntervalMs { get; set; } = 1000;
        public bool MonitorTcpPorts { get; set; } = true;
        public bool MonitorUdpPorts { get; set; } = true;
        
        // Configuración de notificaciones
        public bool EnableToastNotifications { get; set; } = true;
        public int NotificationTimeoutSeconds { get; set; } = 30;
        public bool ShowLowRiskNotifications { get; set; } = false;
        public bool ShowMediumRiskNotifications { get; set; } = true;
        public bool ShowHighRiskNotifications { get; set; } = true;
        public bool ShowCriticalRiskNotifications { get; set; } = true;
        
        // Configuración de seguridad
        public bool RequireConfirmationForCriticalActions { get; set; } = true;
        public bool LogAllEvents { get; set; } = true;
        public int MaxLogRetentionDays { get; set; } = 30;
        
        // Configuración de interfaz
        public string Language { get; set; } = "en-US";
        public bool UseAdvancedMode { get; set; } = false;
        public bool StartMinimized { get; set; } = false;
        public bool MinimizeToTray { get; set; } = true;
        
        // Configuración de reglas
        public bool AutoCreateRulesFromNotifications { get; set; } = true;
        public RuleScope DefaultRuleScope { get; set; } = RuleScope.ProcessAndPort;
        public bool BackupRulesOnChange { get; set; } = true;
        
        // Configuración del servicio
        public bool StartServiceWithWindows { get; set; } = true;
        public bool RestartServiceOnFailure { get; set; } = true;
        public int ServiceRestartDelayMs { get; set; } = 5000;
    }
}
