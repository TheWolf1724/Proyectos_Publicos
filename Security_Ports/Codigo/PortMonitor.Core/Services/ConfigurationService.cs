using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using PortMonitor.Core.Configuration;
using PortMonitor.Core.Interfaces;
using System;
using System.IO;
using System.Threading.Tasks;

namespace PortMonitor.Core.Services
{
    /// <summary>
    /// Implementación del servicio de configuración
    /// </summary>
    public class ConfigurationService : IConfigurationService
    {
        private readonly ILogger<ConfigurationService> _logger;
        private readonly string _configFilePath;
        private AppConfiguration _cachedConfiguration = new AppConfiguration();

        public ConfigurationService(ILogger<ConfigurationService> logger)
        {
            _logger = logger;
            _configFilePath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "PortMonitor",
                "config.json"
            );
        }

        public async Task<AppConfiguration> GetConfigurationAsync()
        {
            if (_cachedConfiguration != null)
                return _cachedConfiguration;

            try
            {
                if (File.Exists(_configFilePath))
                {
                    var json = await File.ReadAllTextAsync(_configFilePath);
                    _cachedConfiguration = JsonConvert.DeserializeObject<AppConfiguration>(json) ?? new AppConfiguration();
                }
                else
                {
                    _cachedConfiguration = new AppConfiguration();
                    await SaveConfigurationAsync(_cachedConfiguration);
                }

                return _cachedConfiguration;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cargar la configuración. Usando configuración por defecto.");
                _cachedConfiguration = new AppConfiguration();
                return _cachedConfiguration;
            }
        }

        public async Task SaveConfigurationAsync(AppConfiguration configuration)
        {
            try
            {
                if (!ValidateConfiguration(configuration))
                {
                    throw new ArgumentException("La configuración no es válida");
                }

                var directory = Path.GetDirectoryName(_configFilePath);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                var json = JsonConvert.SerializeObject(configuration, Formatting.Indented);
                await File.WriteAllTextAsync(_configFilePath, json);

                _cachedConfiguration = configuration;
                _logger.LogInformation("Configuración guardada exitosamente");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al guardar la configuración");
                throw;
            }
        }

        public async Task ResetToDefaultAsync()
        {
            _cachedConfiguration = new AppConfiguration();
            await SaveConfigurationAsync(_cachedConfiguration);
            _logger.LogInformation("Configuración restaurada a valores por defecto");
        }

        public bool ValidateConfiguration(AppConfiguration configuration)
        {
            if (configuration == null)
                return false;

            // Validar intervalos
            if (configuration.MonitoringIntervalMs < 100 || configuration.MonitoringIntervalMs > 60000)
                return false;

            // Validar idioma
            if (string.IsNullOrWhiteSpace(configuration.Language))
                return false;

            // Validar configuración de notificaciones
            if (configuration.Notifications?.NotificationDisplayDurationMs < 1000 ||
                configuration.Notifications?.NotificationDisplayDurationMs > 30000)
                return false;

            if (configuration.Security?.MaxPortsBeforeWarning < 1 ||
                configuration.Security?.MaxPortsBeforeWarning > 1000)
                return false;

            return true;
        }
    }
}
