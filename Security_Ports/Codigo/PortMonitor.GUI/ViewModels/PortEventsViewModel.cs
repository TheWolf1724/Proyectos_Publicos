using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using PortMonitor.Core.Interfaces;
using PortMonitor.Core.Models;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace PortMonitor.GUI.ViewModels
{
    /// <summary>
    /// ViewModel para la gestión de eventos de puertos
    /// </summary>
    public partial class PortEventsViewModel : ObservableObject
    {
        private readonly ILogger<PortEventsViewModel> _logger;
        private readonly IDataRepository _dataRepository;
        private readonly IPortCatalog _portCatalog;

        [ObservableProperty]
        private ObservableCollection<PortEvent> _portEvents = new();

        [ObservableProperty]
        private PortEvent? _selectedPortEvent;

        [ObservableProperty]
        private bool _isAdvancedMode;

        [ObservableProperty]
        private string _searchText = string.Empty;

        [ObservableProperty]
        private string _selectedRiskFilter = "All";

        [ObservableProperty]
        private string _selectedProtocolFilter = "All";

        [ObservableProperty]
        private bool _isLoading;

        public ObservableCollection<string> RiskFilters { get; } = new()
        {
            "All", "Low", "Medium", "High", "Critical"
        };

        public ObservableCollection<string> ProtocolFilters { get; } = new()
        {
            "All", "TCP", "UDP"
        };

        public PortEventsViewModel(
            ILogger<PortEventsViewModel> logger,
            IDataRepository dataRepository,
            IPortCatalog portCatalog)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _dataRepository = dataRepository ?? throw new ArgumentNullException(nameof(dataRepository));
            _portCatalog = portCatalog ?? throw new ArgumentNullException(nameof(portCatalog));
        }

        [RelayCommand]
        public async Task LoadPortEventsAsync()
        {
            try
            {
                IsLoading = true;
                var events = await _dataRepository.GetPortEventsAsync(0, 1000);
                
                PortEvents.Clear();
                foreach (var evt in events)
                {
                    PortEvents.Add(evt);
                }

                ApplyFilters();
                _logger.LogInformation("Loaded {Count} port events", events.Count());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading port events");
                MessageBox.Show($"Error al cargar eventos de puertos: {ex.Message}", 
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }

        [RelayCommand]
        private async Task ShowPortEventDetailsAsync(PortEvent? portEvent)
        {
            if (portEvent == null) return;

            try
            {
                var portInfo = await _portCatalog.GetPortInfoAsync(portEvent.LocalPort, portEvent.Protocol);
                
                var detailsWindow = new PortMonitor.GUI.Views.PortEventDetailsWindowFixed(portEvent)
                {
                    DataContext = new { PortEvent = portEvent, PortInfo = portInfo },
                    Owner = Application.Current.MainWindow
                };
                
                detailsWindow.ShowDialog();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error showing port event details");
                MessageBox.Show($"Error al mostrar detalles: {ex.Message}", 
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        [RelayCommand]
        private async Task DeletePortEventAsync(PortEvent? portEvent)
        {
            if (portEvent == null) return;

            try
            {
                var result = MessageBox.Show(
                    $"¿Está seguro de que desea eliminar este evento?\n\nProceso: {portEvent.ProcessName}\nPuerto: {portEvent.LocalPort}",
                    "Confirmar eliminación",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    var success = await _dataRepository.DeletePortEventAsync(portEvent.Id);
                    if (success)
                    {
                        PortEvents.Remove(portEvent);
                        _logger.LogInformation("Deleted port event {Id}", portEvent.Id);
                    }
                    else
                    {
                        MessageBox.Show("No se pudo eliminar el evento", "Error", 
                            MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting port event");
                MessageBox.Show($"Error al eliminar evento: {ex.Message}", 
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        [RelayCommand]
        private async Task ClearOldEventsAsync()
        {
            try
            {
                var result = MessageBox.Show(
                    "¿Está seguro de que desea eliminar todos los eventos antiguos (más de 30 días)?",
                    "Confirmar limpieza",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    var success = await _dataRepository.DeleteOldPortEventsAsync(30);
                    if (success)
                    {
                        await LoadPortEventsAsync();
                        MessageBox.Show("Eventos antiguos eliminados correctamente", "Éxito", 
                            MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error clearing old events");
                MessageBox.Show($"Error al limpiar eventos antiguos: {ex.Message}", 
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        [RelayCommand]
        private void ApplyFilters()
        {
            try
            {
                // Esta implementación sería más compleja en un escenario real
                // Por simplicidad, mantenemos todos los eventos visibles
                _logger.LogDebug("Filters applied - Search: {Search}, Risk: {Risk}, Protocol: {Protocol}", 
                    SearchText, SelectedRiskFilter, SelectedProtocolFilter);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error applying filters");
            }
        }

        [RelayCommand]
        private void ClearFilters()
        {
            SearchText = string.Empty;
            SelectedRiskFilter = "All";
            SelectedProtocolFilter = "All";
            ApplyFilters();
        }

        [RelayCommand]
        private Task ExportEventsAsync()
        {
            try
            {
                var saveDialog = new Microsoft.Win32.SaveFileDialog
                {
                    Filter = "CSV files (*.csv)|*.csv|JSON files (*.json)|*.json|All files (*.*)|*.*",
                    FileName = $"PortEvents_{DateTime.Now:yyyyMMdd_HHmmss}.csv"
                };

                if (saveDialog.ShowDialog() == true)
                {
                    // Implementar exportación
                    MessageBox.Show("Funcionalidad de exportación en desarrollo", 
                        "Información", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error exporting events");
                MessageBox.Show($"Error al exportar eventos: {ex.Message}", 
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            return Task.CompletedTask;
        }

        partial void OnSearchTextChanged(string value)
        {
            ApplyFilters();
        }

        partial void OnSelectedRiskFilterChanged(string value)
        {
            ApplyFilters();
        }

        partial void OnSelectedProtocolFilterChanged(string value)
        {
            ApplyFilters();
        }
    }
}
