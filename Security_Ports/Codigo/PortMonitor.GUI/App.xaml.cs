using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using PortMonitor.Core.Interfaces;
using PortMonitor.Core.Services;
using PortMonitor.Core.Models;
// ...existing code...
// ...existing code...
using PortMonitor.GUI.Services;
using PortMonitor.GUI.ViewModels;
using PortMonitor.GUI.Views;
using System;
using System.IO;
using System.Windows;

namespace PortMonitor.GUI
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private IHost? _host;
        private IServiceProvider? _serviceProvider;

        protected override async void OnStartup(StartupEventArgs e)
        {
            try
            {
                // Configurar el host de la aplicación
                var builder = Host.CreateApplicationBuilder();

                // Configuración
                builder.Configuration
                    .SetBasePath(AppContext.BaseDirectory)
                    .AddJsonFile("appsettings.json", optional: false)
                    .AddCommandLine(e.Args);

                // Configurar servicios
                ConfigureServices(builder.Services, builder.Configuration);

                _host = builder.Build();
                _serviceProvider = _host.Services;

                // Inicializar la base de datos
                var dataRepository = _serviceProvider.GetRequiredService<IDataRepository>();
                await dataRepository.InitializeDatabaseAsync();

                // Mostrar la ventana principal
                var mainWindow = _serviceProvider.GetRequiredService<MainWindow>();
                MainWindow = mainWindow;

                // Verificar argumentos de línea de comandos
                if (e.Args.Length > 0 && e.Args[0] == "--minimized")
                {
                    mainWindow.WindowState = WindowState.Minimized;
                    mainWindow.ShowInTaskbar = false;
                }

                mainWindow.Show();

                base.OnStartup(e);
            }
            catch (Exception ex)
            {
                try
                {
                    string logDir = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "PortMonitor", "Logs");
                    System.IO.Directory.CreateDirectory(logDir);
                    string logPath = System.IO.Path.Combine(logDir, "startup_error.log");
                    System.IO.File.AppendAllText(logPath, $"[{DateTime.Now}] Error de inicio: {ex}\n");
                    Console.WriteLine($"Error al iniciar la aplicación: {ex.Message}\nVerifica el log en: {logPath}");
                }
                catch { }
                MessageBox.Show($"Error al iniciar la aplicación: {ex.Message}", "Error", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
                Shutdown(1);
            }
        }

        protected override async void OnExit(ExitEventArgs e)
        {
            if (_host != null)
            {
                await _host.StopAsync();
                _host.Dispose();
            }

            base.OnExit(e);
        }

        private void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            // Configuración de la aplicación
            var appConfig = new AppConfiguration();
            configuration.GetSection("AppConfiguration").Bind(appConfig);
            services.AddSingleton(appConfig);

            // Servicios principales
            services.AddSingleton<IDataRepository, PortMonitor.Core.Services.SQLiteDataRepository>();
            services.AddSingleton<IPortCatalog, PortCatalog>();
            services.AddSingleton<IFirewallManager, WindowsFirewallManager>();
            services.AddSingleton<PortMonitor.Core.Interfaces.IConfigurationService, PortMonitor.Core.Services.ConfigurationService>();
            services.AddSingleton<PortMonitor.Core.Interfaces.IPortMonitor, PortMonitor.Core.Services.WindowsPortMonitor>();
            
            // Servicios de la GUI
            services.AddSingleton<IServiceCommunication, ServiceCommunication>();
            services.AddSingleton<ITrayIconService, TrayIconService>();

            // ViewModels
            services.AddTransient<MainWindowViewModel>();
            services.AddTransient<PortEventsViewModel>();
            services.AddTransient<FirewallRulesViewModel>();
            services.AddTransient<SettingsViewModel>();
            services.AddTransient<AboutViewModel>();

            // Ventanas
            services.AddTransient<MainWindow>();
            services.AddTransient<SettingsWindowFixed>();
            services.AddTransient<PortEventDetailsWindowFixed>();

            // Logging
            services.AddLogging(builder =>
            {
                builder.ClearProviders();
                builder.AddConsole();
                builder.SetMinimumLevel(LogLevel.Information);
            });

            // Configuración
            services.AddSingleton<IConfiguration>(configuration);
        }
    }
}
