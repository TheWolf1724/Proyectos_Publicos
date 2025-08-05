using Microsoft.Extensions.Logging;
using System;
using System.ServiceProcess;
using System.Threading.Tasks;

namespace PortMonitor.GUI.Services
{
    /// <summary>
    /// Implementación de la comunicación con el servicio de Windows
    /// </summary>
    public class ServiceCommunication : IServiceCommunication
    {
        private readonly ILogger<ServiceCommunication> _logger;
        private const string SERVICE_NAME = "PortMonitor Security Service";

        public ServiceCommunication(ILogger<ServiceCommunication> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<bool> IsServiceRunningAsync()
        {
            try
            {
                return await Task.Run(() =>
                {
                    using var service = new ServiceController(SERVICE_NAME);
                    return service.Status == ServiceControllerStatus.Running;
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if service is running");
                return false;
            }
        }

        public async Task<bool> StartServiceAsync()
        {
            try
            {
                return await Task.Run(() =>
                {
                    using var service = new ServiceController(SERVICE_NAME);
                    
                    if (service.Status == ServiceControllerStatus.Running)
                    {
                        _logger.LogInformation("Service is already running");
                        return true;
                    }

                    service.Start();
                    service.WaitForStatus(ServiceControllerStatus.Running, TimeSpan.FromSeconds(30));
                    
                    _logger.LogInformation("Service started successfully");
                    return service.Status == ServiceControllerStatus.Running;
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error starting service");
                return false;
            }
        }

        public async Task<bool> StopServiceAsync()
        {
            try
            {
                return await Task.Run(() =>
                {
                    using var service = new ServiceController(SERVICE_NAME);
                    
                    if (service.Status == ServiceControllerStatus.Stopped)
                    {
                        _logger.LogInformation("Service is already stopped");
                        return true;
                    }

                    service.Stop();
                    service.WaitForStatus(ServiceControllerStatus.Stopped, TimeSpan.FromSeconds(30));
                    
                    _logger.LogInformation("Service stopped successfully");
                    return service.Status == ServiceControllerStatus.Stopped;
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error stopping service");
                return false;
            }
        }

        public async Task<bool> RestartServiceAsync()
        {
            try
            {
                var stopped = await StopServiceAsync();
                if (stopped)
                {
                    await Task.Delay(2000); // Esperar 2 segundos
                    return await StartServiceAsync();
                }
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error restarting service");
                return false;
            }
        }

        public async Task<string> GetServiceStatusAsync()
        {
            try
            {
                return await Task.Run(() =>
                {
                    using var service = new ServiceController(SERVICE_NAME);
                    return service.Status.ToString();
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting service status");
                return "Unknown";
            }
        }
    }
}
