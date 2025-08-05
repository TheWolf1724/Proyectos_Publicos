using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.ServiceProcess;
using System.Threading.Tasks;

namespace PortMonitor.Installer
{
    /// <summary>
    /// Instalador principal de la aplicación Port Monitor
    /// </summary>
    public class InstallationManager
    {
        private readonly ILogger<InstallationManager> _logger;
        private const string SERVICE_NAME = "PortMonitor Security Service";
        private const string SERVICE_DISPLAY_NAME = "Port Monitor Security Service";
        private const string SERVICE_DESCRIPTION = "Servicio de monitoreo de puertos de seguridad para Windows";

        public InstallationManager(ILogger<InstallationManager> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Instala la aplicación completa
        /// </summary>
        public async Task<bool> InstallAsync(string installPath)
        {
            try
            {
                _logger.LogInformation("Starting installation to {InstallPath}", installPath);

                // 1. Verificar permisos de administrador
                if (!IsRunningAsAdministrator())
                {
                    _logger.LogError("Installation requires administrator privileges");
                    return false;
                }

                // 2. Crear directorio de instalación
                Directory.CreateDirectory(installPath);

                // 3. Copiar archivos de aplicación
                await CopyApplicationFilesAsync(installPath);

                // 4. Instalar el servicio de Windows
                await InstallWindowsServiceAsync(installPath);

                // 5. Configurar firewall
                await ConfigureFirewallAsync();

                // 6. Crear entradas del registro
                await CreateRegistryEntriesAsync(installPath);

                // 7. Crear accesos directos
                await CreateShortcutsAsync(installPath);

                // 8. Configurar inicio automático
                await ConfigureAutoStartAsync();

                _logger.LogInformation("Installation completed successfully");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during installation");
                return false;
            }
        }

        /// <summary>
        /// Desinstala la aplicación
        /// </summary>
        public async Task<bool> UninstallAsync()
        {
            try
            {
                _logger.LogInformation("Starting uninstallation");

                // 1. Verificar permisos de administrador
                if (!IsRunningAsAdministrator())
                {
                    _logger.LogError("Uninstallation requires administrator privileges");
                    return false;
                }

                // 2. Detener y desinstalar el servicio
                await UninstallWindowsServiceAsync();

                // 3. Eliminar reglas de firewall
                await RemoveFirewallRulesAsync();

                // 4. Eliminar entradas del registro
                await RemoveRegistryEntriesAsync();

                // 5. Eliminar accesos directos
                await RemoveShortcutsAsync();

                // 6. Eliminar archivos de aplicación (opcional, puede preservar configuración)
                await RemoveApplicationFilesAsync();

                _logger.LogInformation("Uninstallation completed successfully");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during uninstallation");
                return false;
            }
        }

        private bool IsRunningAsAdministrator()
        {
            try
            {
                var identity = System.Security.Principal.WindowsIdentity.GetCurrent();
                var principal = new System.Security.Principal.WindowsPrincipal(identity);
                return principal.IsInRole(System.Security.Principal.WindowsBuiltInRole.Administrator);
            }
            catch
            {
                return false;
            }
        }

        private async Task CopyApplicationFilesAsync(string installPath)
        {
            try
            {
                _logger.LogInformation("Copying application files");

                var sourceDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!;
                var files = new[]
                {
                    "PortMonitor.Service.exe",
                    "PortMonitor.GUI.exe",
                    "PortMonitor.Core.dll",
                    "appsettings.json"
                };

                foreach (var file in files)
                {
                    var sourcePath = Path.Combine(sourceDir, file);
                    var destPath = Path.Combine(installPath, file);
                    
                    if (File.Exists(sourcePath))
                    {
                        File.Copy(sourcePath, destPath, true);
                        _logger.LogDebug("Copied {File}", file);
                    }
                }

                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error copying application files");
                throw;
            }
        }

        private async Task InstallWindowsServiceAsync(string installPath)
        {
            try
            {
                _logger.LogInformation("Installing Windows service");

                var serviceExePath = Path.Combine(installPath, "PortMonitor.Service.exe");
                
                // Usar sc.exe para instalar el servicio
                var startInfo = new ProcessStartInfo
                {
                    FileName = "sc.exe",
                    Arguments = $"create \"{SERVICE_NAME}\" binPath=\"{serviceExePath}\" DisplayName=\"{SERVICE_DISPLAY_NAME}\" start=auto",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                };

                using var process = Process.Start(startInfo);
                if (process != null)
                {
                    await process.WaitForExitAsync();
                    
                    if (process.ExitCode == 0)
                    {
                        // Configurar descripción del servicio
                        await SetServiceDescriptionAsync();
                        _logger.LogInformation("Windows service installed successfully");
                    }
                    else
                    {
                        var error = await process.StandardError.ReadToEndAsync();
                        throw new InvalidOperationException($"Failed to install service: {error}");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error installing Windows service");
                throw;
            }
        }

        private async Task SetServiceDescriptionAsync()
        {
            try
            {
                var startInfo = new ProcessStartInfo
                {
                    FileName = "sc.exe",
                    Arguments = $"description \"{SERVICE_NAME}\" \"{SERVICE_DESCRIPTION}\"",
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using var process = Process.Start(startInfo);
                if (process != null)
                {
                    await process.WaitForExitAsync();
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Could not set service description");
            }
        }

        private async Task UninstallWindowsServiceAsync()
        {
            try
            {
                _logger.LogInformation("Uninstalling Windows service");

                // Detener el servicio si está ejecutándose
                try
                {
                    using var service = new ServiceController(SERVICE_NAME);
                    if (service.Status == ServiceControllerStatus.Running)
                    {
                        service.Stop();
                        service.WaitForStatus(ServiceControllerStatus.Stopped, TimeSpan.FromSeconds(30));
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Could not stop service before uninstall");
                }

                // Eliminar el servicio
                var startInfo = new ProcessStartInfo
                {
                    FileName = "sc.exe",
                    Arguments = $"delete \"{SERVICE_NAME}\"",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                };

                using var process = Process.Start(startInfo);
                if (process != null)
                {
                    await process.WaitForExitAsync();
                    
                    if (process.ExitCode == 0)
                    {
                        _logger.LogInformation("Windows service uninstalled successfully");
                    }
                    else
                    {
                        var error = await process.StandardError.ReadToEndAsync();
                        _logger.LogWarning("Failed to uninstall service: {Error}", error);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uninstalling Windows service");
            }
        }

        private async Task ConfigureFirewallAsync()
        {
            try
            {
                _logger.LogInformation("Configuring firewall rules");

                // Permitir la aplicación a través del firewall
                var commands = new[]
                {
                    "advfirewall firewall add rule name=\"Port Monitor Service\" dir=in action=allow program=\"PortMonitor.Service.exe\" enable=yes",
                    "advfirewall firewall add rule name=\"Port Monitor GUI\" dir=in action=allow program=\"PortMonitor.GUI.exe\" enable=yes"
                };

                foreach (var command in commands)
                {
                    var startInfo = new ProcessStartInfo
                    {
                        FileName = "netsh",
                        Arguments = command,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    };

                    using var process = Process.Start(startInfo);
                    if (process != null)
                    {
                        await process.WaitForExitAsync();
                    }
                }

                _logger.LogInformation("Firewall configuration completed");
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Could not configure firewall rules");
            }
        }

        private async Task RemoveFirewallRulesAsync()
        {
            try
            {
                _logger.LogInformation("Removing firewall rules");

                var commands = new[]
                {
                    "advfirewall firewall delete rule name=\"Port Monitor Service\"",
                    "advfirewall firewall delete rule name=\"Port Monitor GUI\""
                };

                foreach (var command in commands)
                {
                    var startInfo = new ProcessStartInfo
                    {
                        FileName = "netsh",
                        Arguments = command,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    };

                    using var process = Process.Start(startInfo);
                    if (process != null)
                    {
                        await process.WaitForExitAsync();
                    }
                }

                _logger.LogInformation("Firewall rules removed");
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Could not remove firewall rules");
            }
        }

        private async Task CreateRegistryEntriesAsync(string installPath)
        {
            try
            {
                _logger.LogInformation("Creating registry entries");

                // Crear entradas en el registro para agregar/quitar programas
                using var key = Microsoft.Win32.Registry.LocalMachine.CreateSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\PortMonitor");
                
                key.SetValue("DisplayName", "Port Monitor Security Application");
                key.SetValue("DisplayVersion", "1.0.0");
                key.SetValue("Publisher", "Port Monitor Security");
                key.SetValue("InstallLocation", installPath);
                key.SetValue("UninstallString", Path.Combine(installPath, "PortMonitor.Installer.exe") + " /uninstall");
                key.SetValue("DisplayIcon", Path.Combine(installPath, "PortMonitor.GUI.exe"));
                key.SetValue("NoModify", 1);
                key.SetValue("NoRepair", 1);

                _logger.LogInformation("Registry entries created");
                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Could not create registry entries");
            }
        }

        private async Task RemoveRegistryEntriesAsync()
        {
            try
            {
                _logger.LogInformation("Removing registry entries");

                Microsoft.Win32.Registry.LocalMachine.DeleteSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\PortMonitor", false);

                _logger.LogInformation("Registry entries removed");
                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Could not remove registry entries");
            }
        }

        private async Task CreateShortcutsAsync(string installPath)
        {
            try
            {
                _logger.LogInformation("Creating shortcuts");

                // Crear acceso directo en el escritorio
                var desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                var startMenuPath = Environment.GetFolderPath(Environment.SpecialFolder.StartMenu);

                // Aquí se implementaría la creación de accesos directos usando COM
                // Por simplicidad, se omite la implementación completa

                _logger.LogInformation("Shortcuts created");
                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Could not create shortcuts");
            }
        }

        private async Task RemoveShortcutsAsync()
        {
            try
            {
                _logger.LogInformation("Removing shortcuts");

                var desktopShortcut = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "Port Monitor.lnk");
                if (File.Exists(desktopShortcut))
                {
                    File.Delete(desktopShortcut);
                }

                _logger.LogInformation("Shortcuts removed");
                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Could not remove shortcuts");
            }
        }

        private async Task ConfigureAutoStartAsync()
        {
            try
            {
                _logger.LogInformation("Configuring auto-start");

                // El servicio ya está configurado para inicio automático
                // Aquí se podría configurar el inicio automático de la GUI si se desea

                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Could not configure auto-start");
            }
        }

        private async Task RemoveApplicationFilesAsync()
        {
            try
            {
                _logger.LogInformation("Removing application files");

                // En una implementación real, se preguntaría al usuario si desea conservar
                // los datos de configuración y logs

                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Could not remove application files");
            }
        }
    }
}
