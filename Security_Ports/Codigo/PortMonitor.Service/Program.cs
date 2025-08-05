using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using PortMonitor.Core.Interfaces;
using PortMonitor.Core.Models;
using PortMonitor.Core.Services;
using PortMonitor.Core.Services;
using System;
using System.Threading.Tasks;

namespace PortMonitor.Service
{
    /// <summary>
    /// Punto de entrada principal del servicio de Windows
    /// </summary>
    public class Program
    {
        public static async Task Main(string[] args)
        {
            try
            {
                var builder = Host.CreateApplicationBuilder(args);

                // Configuración
                builder.Configuration
                    .SetBasePath(AppContext.BaseDirectory)
                    .AddJsonFile("appsettings.json", optional: false)
                    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true)
                    .AddCommandLine(args);

                // Configurar servicios
                ConfigureServices(builder.Services, builder.Configuration);

                // Configurar logging
                ConfigureLogging(builder.Services, builder.Configuration);

                // Configurar como servicio de Windows
                builder.Services.AddWindowsService(options =>
                {
                    options.ServiceName = "PortMonitor Security Service";
                });

                var host = builder.Build();

                await host.RunAsync();
            }
            catch (Exception ex)
            {
                // Log crítico antes de terminar
                Console.WriteLine($"Critical error during service startup: {ex}");
                Environment.Exit(1);
            }
        }

        private static void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            // Configuración de la aplicación
            var appConfig = new AppConfiguration();
            configuration.GetSection("AppConfiguration").Bind(appConfig);
            services.AddSingleton(appConfig);

            // Servicios principales
            services.AddSingleton<IConfigurationService, ConfigurationService>();
            services.AddSingleton<IDataRepository, PortMonitor.Core.Services.SQLiteDataRepository>();
            services.AddSingleton<IPortCatalog, PortCatalog>();
            services.AddSingleton<IPortMonitor, WindowsPortMonitor>();
            services.AddSingleton<IFirewallManager, WindowsFirewallManager>();
            services.AddSingleton<INotificationService, WindowsNotificationService>();

            // Servicio principal como hosted service
            services.AddHostedService<PortMonitorService>();

            // Configuración adicional
            services.AddSingleton<IConfiguration>(configuration);
        }

        private static void ConfigureLogging(IServiceCollection services, IConfiguration configuration)
        {
            services.AddLogging(builder =>
            {
                builder.ClearProviders();
                
                // Console logging para desarrollo
                builder.AddConsole();
                
                // Event Log para producción
                builder.AddEventLog(settings =>
                {
                    settings.SourceName = "PortMonitor Security Service";
                    settings.LogName = "Application";
                });

                // File logging (removido - requiere paquete adicional)
                // builder.AddFile(configuration.GetSection("Logging:File"));

                // Configurar niveles de log
                builder.SetMinimumLevel(LogLevel.Information);
                builder.AddFilter("Microsoft", LogLevel.Warning);
                builder.AddFilter("System", LogLevel.Warning);
            });
        }
    }
}
