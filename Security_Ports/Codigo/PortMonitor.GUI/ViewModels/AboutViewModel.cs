using System;
using System.Diagnostics;
using System.Reflection;
using System.IO;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace PortMonitor.GUI.ViewModels;

public partial class AboutViewModel : ObservableObject
{
    [ObservableProperty]
    private string _appName = "Port Monitor Security";

    [ObservableProperty]
    private string _version = string.Empty;

    [ObservableProperty]
    private string _buildDate = string.Empty;

    [ObservableProperty]
    private string _copyright = "© 2025 Port Monitor Security. Todos los derechos reservados.";

    [ObservableProperty]
    private string _description = "Aplicación de monitoreo de puertos de seguridad para Windows que detecta y controla conexiones de red en tiempo real.";

    public AboutViewModel()
    {
        LoadAppInfo();
    }

    private void LoadAppInfo()
    {
        try
        {
            var assembly = Assembly.GetExecutingAssembly();
            var versionInfo = FileVersionInfo.GetVersionInfo(assembly.Location);
            
            Version = $"Versión {versionInfo.ProductVersion}";
            
            var buildDateTime = GetBuildDateTime(assembly);
            BuildDate = $"Compilado: {buildDateTime:dd/MM/yyyy HH:mm}";
        }
        catch
        {
            Version = "Versión 1.0.0";
            BuildDate = "Compilado: Agosto 2025";
        }
    }

    private DateTime GetBuildDateTime(Assembly assembly)
    {
        var attribute = assembly.GetCustomAttribute<AssemblyMetadataAttribute>();
        if (attribute != null && attribute.Key == "BuildTime" && DateTime.TryParse(attribute.Value, out var buildTime))
        {
            return buildTime;
        }
        
        // Fallback to file creation time
        return System.IO.File.GetCreationTime(assembly.Location);
    }

    [RelayCommand]
    private void OpenLicense()
    {
        try
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = "https://github.com/user/port-monitor/blob/main/LICENSE",
                UseShellExecute = true
            });
        }
        catch (Exception ex)
        {
            // Log error but don't crash
            System.Diagnostics.Debug.WriteLine($"Error opening license: {ex.Message}");
        }
    }

    [RelayCommand]
    private void OpenRepository()
    {
        try
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = "https://github.com/user/port-monitor",
                UseShellExecute = true
            });
        }
        catch (Exception ex)
        {
            // Log error but don't crash
            System.Diagnostics.Debug.WriteLine($"Error opening repository: {ex.Message}");
        }
    }

    [RelayCommand]
    private void SendFeedback()
    {
        try
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = "mailto:support@portmonitor.com?subject=Port Monitor Feedback",
                UseShellExecute = true
            });
        }
        catch (Exception ex)
        {
            // Log error but don't crash
            System.Diagnostics.Debug.WriteLine($"Error opening email client: {ex.Message}");
        }
    }

    [RelayCommand]
    private void CheckForUpdates()
    {
        // This would typically check for updates
        // For now, just show a placeholder message
        System.Windows.MessageBox.Show(
            "Actualmente ejecutando la versión más reciente.",
            "Verificar Actualizaciones",
            System.Windows.MessageBoxButton.OK,
            System.Windows.MessageBoxImage.Information);
    }
}
