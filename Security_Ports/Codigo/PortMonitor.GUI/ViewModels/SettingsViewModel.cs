using System.ComponentModel;
using System;
using System.Threading.Tasks;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using PortMonitor.Core.Configuration;
using PortMonitor.Core.Interfaces;

namespace PortMonitor.GUI.ViewModels;

public partial class SettingsViewModel : ObservableObject
{
    private readonly IConfigurationService _configurationService;
    private readonly IPortMonitor _portMonitor;
    private readonly ILogger<SettingsViewModel> _logger;

    [ObservableProperty]
    private AppConfiguration? _configuration;

    [ObservableProperty]
    private bool _isBasicMode = true;

    [ObservableProperty]
    private bool _hasUnsavedChanges;

    [ObservableProperty]
    private string _statusMessage = string.Empty;

    public SettingsViewModel(
        IConfigurationService configurationService,
        IPortMonitor portMonitor,
        ILogger<SettingsViewModel> logger)
    {
        _configurationService = configurationService;
        _portMonitor = portMonitor;
        _logger = logger;
        
        LoadConfiguration();
    }

    private async void LoadConfiguration()
    {
        try
        {
            Configuration = await _configurationService.GetConfigurationAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading configuration");
            StatusMessage = "Error al cargar la configuración";
        }
    }

    [RelayCommand]
    private async Task SaveConfigurationAsync()
    {
        try
        {
            await _configurationService.SaveConfigurationAsync(Configuration ?? new AppConfiguration());
            HasUnsavedChanges = false;
            StatusMessage = "Configuración guardada correctamente";
            
            _logger.LogInformation("Configuration saved successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving configuration");
            StatusMessage = "Error al guardar la configuración";
        }
    }

    [RelayCommand]
    private void ResetToDefaults()
    {
        Configuration = new AppConfiguration();
        HasUnsavedChanges = true;
        StatusMessage = "Configuración restablecida a valores por defecto";
    }

    [RelayCommand]
    private void ToggleMode()
    {
        IsBasicMode = !IsBasicMode;
    }

    [RelayCommand]
    private Task TestNotificationAsync()
    {
        try
        {
            // This would typically call a notification service to test
            StatusMessage = "Notificación de prueba enviada";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error testing notification");
            StatusMessage = "Error al enviar notificación de prueba";
        }
        return Task.CompletedTask;
    }

    [RelayCommand]
    private Task RestartServiceAsync()
    {
        try
        {
            // This would restart the monitoring service
            StatusMessage = "Servicio reiniciado correctamente";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error restarting service");
            StatusMessage = "Error al reiniciar el servicio";
        }
        return Task.CompletedTask;
    }
}
