using Hardcodet.Wpf.TaskbarNotification;
using Microsoft.Extensions.Logging;
using System;
using System.Windows;
using System.Windows.Controls;

namespace PortMonitor.GUI.Services
{
    /// <summary>
    /// Implementación del servicio del icono de la bandeja del sistema
    /// </summary>
    public class TrayIconService : ITrayIconService, IDisposable
    {
        private readonly ILogger<TrayIconService> _logger;
        private TaskbarIcon? _trayIcon;
        private bool _disposed;

        public event Action? TrayIconClicked;

        public TrayIconService(ILogger<TrayIconService> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public void Initialize()
        {
            try
            {
                if (_trayIcon != null) return;

                _trayIcon = new TaskbarIcon
                {
                    Icon = new System.Drawing.Icon("Resources/app.ico"),
                    ToolTipText = "Port Monitor - Monitoreo de Puertos de Seguridad"
                };

                // Crear menú contextual
                var contextMenu = new ContextMenu();
                
                var showMenuItem = new MenuItem
                {
                    Header = "Mostrar Ventana Principal"
                };
                showMenuItem.Click += (s, e) => TrayIconClicked?.Invoke();
                
                var exitMenuItem = new MenuItem
                {
                    Header = "Salir"
                };
                exitMenuItem.Click += (s, e) => Application.Current.Shutdown();

                contextMenu.Items.Add(showMenuItem);
                contextMenu.Items.Add(new Separator());
                contextMenu.Items.Add(exitMenuItem);

                _trayIcon.ContextMenu = contextMenu;

                // Configurar eventos
                _trayIcon.TrayLeftMouseDown += (s, e) => TrayIconClicked?.Invoke();
                _trayIcon.TrayRightMouseDown += (s, e) => _trayIcon.ContextMenu.IsOpen = true;

                _logger.LogInformation("Tray icon initialized successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error initializing tray icon");
            }
        }

        public void ShowTrayIcon()
        {
            try
            {
                if (_trayIcon != null)
                {
                    _trayIcon.Visibility = Visibility.Visible;
                    _logger.LogDebug("Tray icon shown");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error showing tray icon");
            }
        }

        public void HideTrayIcon()
        {
            try
            {
                if (_trayIcon != null)
                {
                    _trayIcon.Visibility = Visibility.Hidden;
                    _logger.LogDebug("Tray icon hidden");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error hiding tray icon");
            }
        }

        public void ShowBalloonTip(string title, string text, int timeout = 5000)
        {
            try
            {
                _trayIcon?.ShowBalloonTip(title, text, BalloonIcon.Info);
                _logger.LogDebug("Balloon tip shown: {Title}", title);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error showing balloon tip");
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed && disposing)
            {
                try
                {
                    _trayIcon?.Dispose();
                    _trayIcon = null;
                    _logger.LogDebug("Tray icon disposed");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error disposing tray icon");
                }

                _disposed = true;
            }
        }
    }
}
