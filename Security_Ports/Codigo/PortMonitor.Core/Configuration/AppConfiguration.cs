using System;
using System.Collections.Generic;

namespace PortMonitor.Core.Configuration
{
    /// <summary>
    /// Configuración principal de la aplicación
    /// </summary>
    public class AppConfiguration
    {
        public int MonitoringIntervalMs { get; set; } = 5000;
        public string Language { get; set; } = "es";
        public bool EnableToastNotifications { get; set; } = true;
        public bool EnableFirewallIntegration { get; set; } = true;
        public bool AutoStartWithWindows { get; set; } = false;
        public bool MinimizeToTray { get; set; } = true;
        public bool ShowDetailedLogs { get; set; } = false;
        public List<string> IgnoredPorts { get; set; } = new List<string>();
        public List<string> TrustedApplications { get; set; } = new List<string>();
        
        /// <summary>
        /// Configuración de notificaciones
        /// </summary>
        public NotificationSettings Notifications { get; set; } = new NotificationSettings();
        
        /// <summary>
        /// Configuración de seguridad
        /// </summary>
        public SecuritySettings Security { get; set; } = new SecuritySettings();
    }

    /// <summary>
    /// Configuración de notificaciones
    /// </summary>
    public class NotificationSettings
    {
        public bool EnablePortOpenNotifications { get; set; } = true;
        public bool EnablePortCloseNotifications { get; set; } = false;
        public bool EnableSuspiciousActivityNotifications { get; set; } = true;
        public int NotificationDisplayDurationMs { get; set; } = 5000;
    }

    /// <summary>
    /// Configuración de seguridad
    /// </summary>
    public class SecuritySettings
    {
        public bool AutoBlockSuspiciousPorts { get; set; } = false;
        public bool RequireAdminForChanges { get; set; } = true;
        public int MaxPortsBeforeWarning { get; set; } = 50;
        public List<int> AlwaysAllowedPorts { get; set; } = new List<int> { 80, 443, 22, 21 };
    }
}
