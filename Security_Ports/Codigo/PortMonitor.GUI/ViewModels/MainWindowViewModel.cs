using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using PortMonitor.Core.Interfaces;
using PortMonitor.Core.Models;
using PortMonitor.GUI.Services;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace PortMonitor.GUI.ViewModels
{
    /// <summary>
    /// ViewModel principal de la aplicación
    /// </summary>
    public partial class MainWindowViewModel : ObservableObject
    {
        private readonly ILogger<MainWindowViewModel> _logger;
        private readonly IDataRepository _dataRepository;
        private readonly IFirewallManager _firewallManager;
        private readonly IServiceCommunication _serviceCommunication;
        private readonly ITrayIconService _trayIconService;
        private readonly AppConfiguration _configuration;

        [ObservableProperty]
        private bool _isServiceConnected;

        [ObservableProperty]
        private bool _isMonitoring;

        [ObservableProperty]
        private string _serviceStatus = "Desconectado";

        [ObservableProperty]
        private int _totalPortEvents;

        [ObservableProperty]
        private int _activeRules;

        [ObservableProperty]
        private int _blockedPorts;

        [ObservableProperty]
        private string _selectedView = "Dashboard";

        [ObservableProperty]
        private bool _isAdvancedMode;

        [ObservableProperty]
        private ObservableCollection<PortEvent> _recentEvents = new();

        [ObservableProperty]
        private ObservableCollection<FirewallRule> _recentRules = new();

        public PortEventsViewModel PortEventsViewModel { get; }
        public FirewallRulesViewModel FirewallRulesViewModel { get; }
        public SettingsViewModel SettingsViewModel { get; }

        public MainWindowViewModel(
            ILogger<MainWindowViewModel> logger,
            IDataRepository dataRepository,
            IFirewallManager firewallManager,
            IServiceCommunication serviceCommunication,
            ITrayIconService trayIconService,
            AppConfiguration configuration,
            PortEventsViewModel portEventsViewModel,
            FirewallRulesViewModel firewallRulesViewModel,
            SettingsViewModel settingsViewModel)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _dataRepository = dataRepository ?? throw new ArgumentNullException(nameof(dataRepository));
            _firewallManager = firewallManager ?? throw new ArgumentNullException(nameof(firewallManager));
            _serviceCommunication = serviceCommunication ?? throw new ArgumentNullException(nameof(serviceCommunication));
            _trayIconService = trayIconService ?? throw new ArgumentNullException(nameof(trayIconService));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));

            PortEventsViewModel = portEventsViewModel ?? throw new ArgumentNullException(nameof(portEventsViewModel));
            FirewallRulesViewModel = firewallRulesViewModel ?? throw new ArgumentNullException(nameof(firewallRulesViewModel));
            SettingsViewModel = settingsViewModel ?? throw new ArgumentNullException(nameof(settingsViewModel));

            IsAdvancedMode = _configuration.UseAdvancedMode;

            // Inicializar servicios
            InitializeAsync();
        }

        [RelayCommand]
        private void NavigateToView(string viewName)
        {
            SelectedView = viewName;
            _logger.LogDebug("Navigated to view: {ViewName}", viewName);
        }

        [RelayCommand]
        private async Task ToggleServiceAsync()
        {
            try
            {
                if (IsServiceConnected)
                {
                    await _serviceCommunication.StopServiceAsync();
                    ServiceStatus = "Detenido";
                    IsMonitoring = false;
                }
                else
                {
                    await _serviceCommunication.StartServiceAsync();
                    ServiceStatus = "Ejecutándose";
                    IsMonitoring = true;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error toggling service");
                MessageBox.Show($"Error al cambiar el estado del servicio: {ex.Message}", 
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        [RelayCommand]
        private async Task RefreshDataAsync()
        {
            try
            {
                await LoadDashboardDataAsync();
                await PortEventsViewModel.LoadPortEventsAsync();
                await FirewallRulesViewModel.LoadFirewallRulesAsync();
                
                _logger.LogInformation("Data refreshed successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error refreshing data");
                MessageBox.Show($"Error al actualizar los datos: {ex.Message}", 
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        [RelayCommand]
        private void MinimizeToTray()
        {
            try
            {
                if (_configuration.MinimizeToTray)
                {
                    Application.Current.MainWindow.Hide();
                    _trayIconService.ShowTrayIcon();
                    _logger.LogDebug("Application minimized to tray");
                }
                else
                {
                    Application.Current.MainWindow.WindowState = WindowState.Minimized;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error minimizing to tray");
            }
        }

        [RelayCommand]
        private void ToggleAdvancedMode()
        {
            IsAdvancedMode = !IsAdvancedMode;
            _configuration.UseAdvancedMode = IsAdvancedMode;
            
            // Notificar a otros ViewModels del cambio
            PortEventsViewModel.IsAdvancedMode = IsAdvancedMode;
            FirewallRulesViewModel.IsAdvancedMode = IsAdvancedMode;
            
            _logger.LogInformation("Advanced mode {Status}", IsAdvancedMode ? "enabled" : "disabled");
        }

        [RelayCommand]
        private async Task OpenSettingsAsync()
        {
            try
            {
                var settingsWindow = new PortMonitor.GUI.Views.SettingsWindowFixed(SettingsViewModel)
                {
                    DataContext = SettingsViewModel,
                    Owner = Application.Current.MainWindow
                };
                
                settingsWindow.ShowDialog();
                
                // Recargar configuración si se guardaron cambios
                await RefreshDataAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error opening settings");
                MessageBox.Show($"Error al abrir la configuración: {ex.Message}", 
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        [RelayCommand]
        private void ShowAbout()
        {
            try
            {
                var aboutWindow = new PortMonitor.GUI.Views.AboutWindow
                {
                    Owner = Application.Current.MainWindow
                };
                
                aboutWindow.ShowDialog();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error showing about dialog");
            }
        }

        [RelayCommand]
        private Task ExportDataAsync()
        {
            try
            {
                var saveDialog = new Microsoft.Win32.SaveFileDialog
                {
                    Filter = "JSON files (*.json)|*.json|All files (*.*)|*.*",
                    FileName = $"PortMonitor_Export_{DateTime.Now:yyyyMMdd_HHmmss}.json"
                };

                if (saveDialog.ShowDialog() == true)
                {
                    // Aquí implementarías la exportación de datos
                    MessageBox.Show("Funcionalidad de exportación en desarrollo", 
                        "Información", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error exporting data");
                MessageBox.Show($"Error al exportar datos: {ex.Message}", 
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            return Task.CompletedTask;
        }

        private async void InitializeAsync()
        {
            try
            {
                // Verificar conexión con el servicio
                IsServiceConnected = await _serviceCommunication.IsServiceRunningAsync();
                ServiceStatus = IsServiceConnected ? "Ejecutándose" : "Detenido";
                IsMonitoring = IsServiceConnected;

                // Cargar datos del dashboard
                await LoadDashboardDataAsync();

                // Configurar el icono de la bandeja del sistema
                _trayIconService.Initialize();
                _trayIconService.TrayIconClicked += OnTrayIconClicked;

                _logger.LogInformation("MainWindowViewModel initialized successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error initializing MainWindowViewModel");
            }
        }

        private async Task LoadDashboardDataAsync()
        {
            try
            {
                // Cargar estadísticas generales
                var allEvents = await _dataRepository.GetPortEventsAsync(0, 1000);
                TotalPortEvents = allEvents.Count();

                var allRules = await _dataRepository.GetFirewallRulesAsync();
                ActiveRules = allRules.Count(r => r.IsEnabled);
                BlockedPorts = allRules.Count(r => r.Action == RuleAction.Block && r.IsEnabled);

                // Cargar eventos recientes
                var recentEventsList = await _dataRepository.GetPortEventsAsync(0, 10);
                RecentEvents.Clear();
                foreach (var evt in recentEventsList)
                {
                    RecentEvents.Add(evt);
                }

                // Cargar reglas recientes
                var recentRulesList = allRules.Take(10);
                RecentRules.Clear();
                foreach (var rule in recentRulesList)
                {
                    RecentRules.Add(rule);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading dashboard data");
            }
        }

        private void OnTrayIconClicked()
        {
            try
            {
                if (Application.Current.MainWindow.IsVisible)
                {
                    Application.Current.MainWindow.Hide();
                }
                else
                {
                    Application.Current.MainWindow.Show();
                    Application.Current.MainWindow.WindowState = WindowState.Normal;
                    Application.Current.MainWindow.Activate();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error handling tray icon click");
            }
        }

        partial void OnSelectedViewChanged(string value)
        {
            _logger.LogDebug("Selected view changed to: {View}", value);
        }
    }
}
