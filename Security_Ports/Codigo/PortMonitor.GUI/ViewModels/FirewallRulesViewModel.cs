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
    /// ViewModel para la gestión de reglas de firewall
    /// </summary>
    public partial class FirewallRulesViewModel : ObservableObject
    {
        private readonly ILogger<FirewallRulesViewModel> _logger;
        private readonly IDataRepository _dataRepository;
        private readonly IFirewallManager _firewallManager;

        [ObservableProperty]
        private ObservableCollection<FirewallRule> _firewallRules = new();

        [ObservableProperty]
        private FirewallRule? _selectedRule;

        [ObservableProperty]
        private bool _isAdvancedMode;

        [ObservableProperty]
        private string _searchText = string.Empty;

        [ObservableProperty]
        private string _selectedActionFilter = "All";

        [ObservableProperty]
        private string _selectedStatusFilter = "All";

        [ObservableProperty]
        private bool _isLoading;

        public ObservableCollection<string> ActionFilters { get; } = new()
        {
            "All", "Allow", "Block"
        };

        public ObservableCollection<string> StatusFilters { get; } = new()
        {
            "All", "Enabled", "Disabled"
        };

        public FirewallRulesViewModel(
            ILogger<FirewallRulesViewModel> logger,
            IDataRepository dataRepository,
            IFirewallManager firewallManager)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _dataRepository = dataRepository ?? throw new ArgumentNullException(nameof(dataRepository));
            _firewallManager = firewallManager ?? throw new ArgumentNullException(nameof(firewallManager));
        }

        [RelayCommand]
        public async Task LoadFirewallRulesAsync()
        {
            try
            {
                IsLoading = true;
                var rules = await _dataRepository.GetFirewallRulesAsync();
                
                FirewallRules.Clear();
                foreach (var rule in rules)
                {
                    FirewallRules.Add(rule);
                }

                ApplyFilters();
                _logger.LogInformation("Loaded {Count} firewall rules", rules.Count());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading firewall rules");
                MessageBox.Show($"Error al cargar reglas de firewall: {ex.Message}", 
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }

        [RelayCommand]
        private async Task ToggleRuleEnabledAsync(FirewallRule? rule)
        {
            if (rule == null) return;

            try
            {
                var newStatus = !rule.IsEnabled;
                var success = await _firewallManager.SetRuleEnabledAsync(rule.Id, newStatus);
                
                if (success)
                {
                    rule.IsEnabled = newStatus;
                    rule.ModifiedDate = DateTime.Now;
                    
                    await _dataRepository.UpdateFirewallRuleAsync(rule);
                    
                    _logger.LogInformation("Rule {RuleId} {Status}", rule.Id, 
                        newStatus ? "enabled" : "disabled");
                }
                else
                {
                    MessageBox.Show("No se pudo cambiar el estado de la regla", "Error", 
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error toggling rule enabled state");
                MessageBox.Show($"Error al cambiar el estado de la regla: {ex.Message}", 
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        [RelayCommand]
        private async Task DeleteRuleAsync(FirewallRule? rule)
        {
            if (rule == null) return;

            try
            {
                var result = MessageBox.Show(
                    $"¿Está seguro de que desea eliminar esta regla?\n\nNombre: {rule.Name}\nDescripción: {rule.Description}",
                    "Confirmar eliminación",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    var success = await _firewallManager.DeleteRuleAsync(rule.Id);
                    if (success)
                    {
                        FirewallRules.Remove(rule);
                        _logger.LogInformation("Deleted firewall rule {Id}", rule.Id);
                    }
                    else
                    {
                        MessageBox.Show("No se pudo eliminar la regla", "Error", 
                            MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting firewall rule");
                MessageBox.Show($"Error al eliminar regla: {ex.Message}", 
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        [RelayCommand]
        private async Task CreateNewRuleAsync()
        {
            try
            {
                var newRule = new FirewallRule
                {
                    Name = "Nueva Regla",
                    Description = "Regla creada manualmente",
                    Action = RuleAction.Block,
                    Direction = RuleDirection.Both,
                    Scope = RuleScope.Custom,
                    IsUserCreated = true,
                    CreatedDate = DateTime.Now
                };

                // Aquí abriríamos un diálogo para editar la regla
                var editDialog = new PortMonitor.GUI.Views.EditRuleDialog(newRule)
                {
                    Owner = Application.Current.MainWindow
                };

                if (editDialog.ShowDialog() == true)
                {
                    var success = await _firewallManager.CreateRuleAsync(editDialog.Result ?? newRule);
                    if (success)
                    {
                        FirewallRules.Add(editDialog.Result ?? newRule);
                        _logger.LogInformation("Created new firewall rule: {Name}", (editDialog.Result ?? newRule).Name);
                    }
                    else
                    {
                        MessageBox.Show("No se pudo crear la regla", "Error", 
                            MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating new rule");
                MessageBox.Show($"Error al crear nueva regla: {ex.Message}", 
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        [RelayCommand]
        private async Task EditRuleAsync(FirewallRule? rule)
        {
            if (rule == null) return;

            try
            {
                var editDialog = new PortMonitor.GUI.Views.EditRuleDialog(rule.Clone())
                {
                    Owner = Application.Current.MainWindow
                };

                if (editDialog.ShowDialog() == true)
                {
                    var resultRule = editDialog.Result;
                    if (resultRule != null)
                    {
                        var success = await _firewallManager.UpdateRuleAsync(resultRule);
                        if (success)
                        {
                            rule.Name = resultRule.Name;
                            rule.Description = resultRule.Description;
                            rule.Action = resultRule.Action;
                            rule.Direction = resultRule.Direction;
                            rule.Scope = resultRule.Scope;
                            rule.ProcessPath = resultRule.ProcessPath;
                            rule.ProcessId = resultRule.ProcessId;
                            rule.LocalPort = resultRule.LocalPort;
                            rule.RemotePort = resultRule.RemotePort;
                            rule.Protocol = resultRule.Protocol;
                            rule.LocalIpAddress = resultRule.LocalIpAddress;
                            rule.RemoteIpAddress = resultRule.RemoteIpAddress;
                            rule.IpRange = resultRule.IpRange;
                            rule.IsUserCreated = resultRule.IsUserCreated;
                            rule.IsPersistent = resultRule.IsPersistent;
                            rule.Tags = resultRule.Tags;
                            rule.ModifiedDate = DateTime.Now;
                            await _dataRepository.UpdateFirewallRuleAsync(rule);
                            _logger.LogInformation("Updated firewall rule: {Name}", rule.Name);
                        }
                        else
                        {
                            MessageBox.Show("No se pudo actualizar la regla", "Error", 
                                MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error editing rule");
                MessageBox.Show($"Error al editar regla: {ex.Message}", 
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        [RelayCommand]
        private async Task RestoreDefaultRulesAsync()
        {
            try
            {
                var result = MessageBox.Show(
                    "¿Está seguro de que desea restaurar las reglas por defecto?\n\nEsto eliminará todas las reglas personalizadas.",
                    "Confirmar restauración",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);

                if (result == MessageBoxResult.Yes)
                {
                    var success = await _firewallManager.RestoreDefaultRulesAsync();
                    if (success)
                    {
                        await LoadFirewallRulesAsync();
                        MessageBox.Show("Reglas por defecto restauradas correctamente", "Éxito", 
                            MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    else
                    {
                        MessageBox.Show("No se pudieron restaurar las reglas por defecto", "Error", 
                            MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error restoring default rules");
                MessageBox.Show($"Error al restaurar reglas por defecto: {ex.Message}", 
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        [RelayCommand]
        private void ApplyFilters()
        {
            try
            {
                // Implementación simple de filtros
                _logger.LogDebug("Filters applied - Search: {Search}, Action: {Action}, Status: {Status}", 
                    SearchText, SelectedActionFilter, SelectedStatusFilter);
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
            SelectedActionFilter = "All";
            SelectedStatusFilter = "All";
            ApplyFilters();
        }

        [RelayCommand]
        private Task ExportRulesAsync()
        {
            try
            {
                var saveDialog = new Microsoft.Win32.SaveFileDialog
                {
                    Filter = "JSON files (*.json)|*.json|XML files (*.xml)|*.xml|All files (*.*)|*.*",
                    FileName = $"FirewallRules_{DateTime.Now:yyyyMMdd_HHmmss}.json"
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
                _logger.LogError(ex, "Error exporting rules");
                MessageBox.Show($"Error al exportar reglas: {ex.Message}", 
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            return Task.CompletedTask;
        }

        partial void OnSearchTextChanged(string value)
        {
            ApplyFilters();
        }

        partial void OnSelectedActionFilterChanged(string value)
        {
            ApplyFilters();
        }

        partial void OnSelectedStatusFilterChanged(string value)
        {
            ApplyFilters();
        }
    }
}
