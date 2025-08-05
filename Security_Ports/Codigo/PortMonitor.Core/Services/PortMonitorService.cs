using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using PortMonitor.Core.Interfaces;
using PortMonitor.Core.Models;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace PortMonitor.Core.Services
{
    /// <summary>
    /// Servicio principal que orquesta el monitoreo de puertos y las respuestas automáticas
    /// </summary>
    public class PortMonitorService : BackgroundService
    {
        private readonly ILogger<PortMonitorService> _logger;
        private readonly IPortMonitor _portMonitor;
        private readonly INotificationService _notificationService;
        private readonly IFirewallManager _firewallManager;
        private readonly IPortCatalog _portCatalog;
        private readonly IDataRepository _dataRepository;
        private readonly AppConfiguration _configuration;

        public PortMonitorService(
            ILogger<PortMonitorService> logger,
            IPortMonitor portMonitor,
            INotificationService notificationService,
            IFirewallManager firewallManager,
            IPortCatalog portCatalog,
            IDataRepository dataRepository,
            AppConfiguration configuration)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _portMonitor = portMonitor ?? throw new ArgumentNullException(nameof(portMonitor));
            _notificationService = notificationService ?? throw new ArgumentNullException(nameof(notificationService));
            _firewallManager = firewallManager ?? throw new ArgumentNullException(nameof(firewallManager));
            _portCatalog = portCatalog ?? throw new ArgumentNullException(nameof(portCatalog));
            _dataRepository = dataRepository ?? throw new ArgumentNullException(nameof(dataRepository));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        public override async Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Port Monitor Service is starting...");

            try
            {
                // Inicializar la base de datos
                await _dataRepository.InitializeDatabaseAsync();

                // Verificar que el firewall esté habilitado
                var firewallEnabled = await _firewallManager.IsFirewallEnabledAsync();
                if (!firewallEnabled)
                {
                    _logger.LogWarning("Windows Firewall is not enabled. The application may not function correctly.");
                    await _notificationService.ShowWarningNotificationAsync(
                        "Firewall Deshabilitado",
                        "El Firewall de Windows está deshabilitado. La aplicación puede no funcionar correctamente.");
                }

                // Verificar notificaciones
                var notificationsEnabled = await _notificationService.AreNotificationsEnabledAsync();
                if (!notificationsEnabled)
                {
                    _logger.LogWarning("Notifications are not enabled. User will not receive port alerts.");
                }

                // Configurar eventos del monitor de puertos
                _portMonitor.PortChanged += OnPortChanged;

                // Configurar eventos del servicio de notificaciones
                _notificationService.NotificationActionReceived += OnNotificationActionReceived;

                // Iniciar el monitoreo
                if (_configuration.EnableRealTimeMonitoring)
                {
                    await _portMonitor.StartMonitoringAsync();
                }

                await base.StartAsync(cancellationToken);
                
                _logger.LogInformation("Port Monitor Service started successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to start Port Monitor Service");
                throw;
            }
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Port Monitor Service is stopping...");

            try
            {
                await _portMonitor.StopMonitoringAsync();
                
                // Desconectar eventos
                _portMonitor.PortChanged -= OnPortChanged;
                _notificationService.NotificationActionReceived -= OnNotificationActionReceived;

                await base.StopAsync(cancellationToken);
                
                _logger.LogInformation("Port Monitor Service stopped successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error stopping Port Monitor Service");
            }
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Port Monitor Service is running");

            try
            {
                // Tareas de mantenimiento periódico
                while (!stoppingToken.IsCancellationRequested)
                {
                    await Task.Delay(TimeSpan.FromHours(1), stoppingToken);

                    // Limpiar eventos antiguos
                    if (_configuration.MaxLogRetentionDays > 0)
                    {
                        await _dataRepository.DeleteOldPortEventsAsync(_configuration.MaxLogRetentionDays);
                    }

                    // Optimizar base de datos periódicamente
                    if (DateTime.Now.Hour == 2) // A las 2 AM
                    {
                        await _dataRepository.OptimizeDatabaseAsync();
                    }
                }
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("Port Monitor Service execution was cancelled");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in Port Monitor Service execution");
            }
        }

        private async void OnPortChanged(PortEvent portEvent)
        {
            try
            {
                _logger.LogDebug("Port change detected: {ProcessName} ({ProcessId}) {EventType} {Protocol} port {LocalPort}",
                    portEvent.ProcessName, portEvent.ProcessId, portEvent.EventType, portEvent.Protocol, portEvent.LocalPort);

                // Solo procesar eventos de apertura de puerto
                if (portEvent.EventType != PortEventType.Opened)
                {
                    return;
                }

                // Analizar el riesgo del puerto
                portEvent.RiskLevel = await _portCatalog.AnalyzePortRiskAsync(portEvent);

                // Obtener información del puerto
                var portInfo = await _portCatalog.GetPortInfoAsync(portEvent.LocalPort, portEvent.Protocol);

                // Guardar el evento en la base de datos
                var eventId = await _dataRepository.SavePortEventAsync(portEvent);
                portEvent.Id = eventId;

                // Mostrar notificación si está habilitada
                if (_configuration.EnableToastNotifications)
                {
                    await _notificationService.ShowPortEventNotificationAsync(portEvent, portInfo);
                }

                // Aplicar reglas automáticas si está configurado
                if (_configuration.AutoCreateRulesFromNotifications)
                {
                    await ApplyAutomaticRulesAsync(portEvent, portInfo);
                }

                _logger.LogInformation("Port event processed: {ProcessName} opened {Protocol} port {LocalPort} (Risk: {RiskLevel})",
                    portEvent.ProcessName, portEvent.Protocol, portEvent.LocalPort, portEvent.RiskLevel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing port change event");
            }
        }

        private async void OnNotificationActionReceived(NotificationAction action)
        {
            try
            {
                _logger.LogInformation("Notification action received: {ActionType} for port event {PortEventId}",
                    action.ActionType, action.PortEventId);

                var portEvent = await _dataRepository.GetPortEventAsync(action.PortEventId);
                if (portEvent == null)
                {
                    _logger.LogWarning("Port event not found for notification action: {PortEventId}", action.PortEventId);
                    return;
                }

                switch (action.ActionType)
                {
                    case NotificationActionType.Allow:
                        await HandleAllowActionAsync(portEvent);
                        break;

                    case NotificationActionType.Block:
                        await HandleBlockActionAsync(portEvent);
                        break;

                    case NotificationActionType.ShowDetails:
                        await HandleShowDetailsActionAsync(portEvent);
                        break;

                    case NotificationActionType.Dismiss:
                        await HandleDismissActionAsync(portEvent);
                        break;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error handling notification action");
            }
        }

        private async Task HandleAllowActionAsync(PortEvent portEvent)
        {
            try
            {
                var success = await _firewallManager.AllowProcessPortAsync(
                    portEvent.ExecutablePath, 
                    portEvent.LocalPort, 
                    portEvent.Protocol);

                if (success)
                {
                    portEvent.IsAllowed = true;
                    await _dataRepository.SavePortEventAsync(portEvent);

                    await _notificationService.ShowInfoNotificationAsync(
                        "Puerto Permitido",
                        $"Se ha permitido el acceso de {portEvent.ProcessName} al puerto {portEvent.LocalPort}");

                    _logger.LogInformation("Port access allowed: {ProcessName} on port {LocalPort}",
                        portEvent.ProcessName, portEvent.LocalPort);
                }
                else
                {
                    await _notificationService.ShowErrorNotificationAsync(
                        "Error",
                        "No se pudo crear la regla de firewall para permitir el acceso");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error handling allow action");
                await _notificationService.ShowErrorNotificationAsync(
                    "Error",
                    "Error interno al procesar la acción de permitir");
            }
        }

        private async Task HandleBlockActionAsync(PortEvent portEvent)
        {
            try
            {
                var success = await _firewallManager.BlockProcessPortAsync(
                    portEvent.ExecutablePath, 
                    portEvent.LocalPort, 
                    portEvent.Protocol);

                if (success)
                {
                    portEvent.IsAllowed = false;
                    await _dataRepository.SavePortEventAsync(portEvent);

                    await _notificationService.ShowInfoNotificationAsync(
                        "Puerto Bloqueado",
                        $"Se ha bloqueado el acceso de {portEvent.ProcessName} al puerto {portEvent.LocalPort}");

                    _logger.LogInformation("Port access blocked: {ProcessName} on port {LocalPort}",
                        portEvent.ProcessName, portEvent.LocalPort);
                }
                else
                {
                    await _notificationService.ShowErrorNotificationAsync(
                        "Error",
                        "No se pudo crear la regla de firewall para bloquear el acceso");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error handling block action");
                await _notificationService.ShowErrorNotificationAsync(
                    "Error",
                    "Error interno al procesar la acción de bloquear");
            }
        }

        private async Task HandleShowDetailsActionAsync(PortEvent portEvent)
        {
            try
            {
                // Esta acción típicamente abriría la GUI principal con los detalles del evento
                // Por ahora, registramos la acción
                _logger.LogInformation("Show details requested for port event: {PortEventId}", portEvent.Id);

                // En una implementación completa, esto enviaría un mensaje a la GUI principal
                // para mostrar la ventana de detalles del evento
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error handling show details action");
            }
        }

        private async Task HandleDismissActionAsync(PortEvent portEvent)
        {
            try
            {
                // Registrar que el usuario descartó la notificación
                _logger.LogDebug("Notification dismissed for port event: {PortEventId}", portEvent.Id);

                // Podríamos marcar el evento como "visto" en la base de datos
                // o realizar alguna otra acción según la configuración
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error handling dismiss action");
            }
        }

        private async Task ApplyAutomaticRulesAsync(PortEvent portEvent, PortInfo? portInfo)
        {
            try
            {
                // Aplicar reglas automáticas basadas en el nivel de riesgo
                switch (portEvent.RiskLevel)
                {
                    case RiskLevel.Critical:
                        // Bloquear automáticamente puertos críticos
                        if (_configuration.RequireConfirmationForCriticalActions)
                        {
                            // Solo mostrar notificación con recomendación
                            await _notificationService.ShowWarningNotificationAsync(
                                "RIESGO CRÍTICO DETECTADO",
                                $"El puerto {portEvent.LocalPort} abierto por {portEvent.ProcessName} tiene riesgo crítico. Se recomienda bloquearlo inmediatamente.");
                        }
                        else
                        {
                            // Bloquear automáticamente
                            await _firewallManager.BlockProcessPortAsync(
                                portEvent.ExecutablePath, 
                                portEvent.LocalPort, 
                                portEvent.Protocol);
                        }
                        break;

                    case RiskLevel.High:
                        // Mostrar advertencia para puertos de alto riesgo
                        await _notificationService.ShowWarningNotificationAsync(
                            "RIESGO ALTO DETECTADO",
                            $"El puerto {portEvent.LocalPort} abierto por {portEvent.ProcessName} tiene riesgo alto. Revise si es necesario.");
                        break;

                    case RiskLevel.Low when portInfo?.IsWellKnown == true:
                        // Permitir automáticamente puertos conocidos de bajo riesgo
                        await _firewallManager.AllowProcessPortAsync(
                            portEvent.ExecutablePath, 
                            portEvent.LocalPort, 
                            portEvent.Protocol);
                        portEvent.IsAllowed = true;
                        break;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error applying automatic rules");
            }
        }

        public override void Dispose()
        {
            try
            {
                _portMonitor.PortChanged -= OnPortChanged;
                _notificationService.NotificationActionReceived -= OnNotificationActionReceived;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error disposing PortMonitorService");
            }

            base.Dispose();
        }
    }
}
