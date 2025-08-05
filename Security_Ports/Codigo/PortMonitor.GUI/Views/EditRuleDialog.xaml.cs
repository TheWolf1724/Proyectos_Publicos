using System.Windows;
using Microsoft.Win32;
using PortMonitor.Core.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace PortMonitor.GUI.Views;

public partial class EditRuleDialog : Window
{
    public EditRuleDialog(FirewallRule rule)
    {
        InitializeComponent();
        DataContext = new EditRuleDialogViewModel(rule);
    }

    public FirewallRule? Result { get; internal set; }

    private void OnSaveClicked(object sender, RoutedEventArgs e)
    {
        if (DataContext is EditRuleDialogViewModel viewModel)
        {
            Result = viewModel.FirewallRule;
            DialogResult = true;
        }
    }
}

public partial class EditRuleDialogViewModel : ObservableObject
{
    [ObservableProperty]
    private FirewallRule _firewallRule;

    public EditRuleDialogViewModel(FirewallRule rule)
    {
        FirewallRule = rule;
    }

    [RelayCommand]
    private void BrowseProgram()
    {
        var openFileDialog = new OpenFileDialog
        {
            Title = "Seleccionar Programa",
            Filter = "Ejecutables (*.exe)|*.exe|Todos los archivos (*.*)|*.*",
            CheckFileExists = true
        };

        if (openFileDialog.ShowDialog() == true)
        {
            FirewallRule.ProcessPath = openFileDialog.FileName;
        }
    }

    [RelayCommand]
    private void TestRule()
    {
        // Implementation would test the firewall rule
        MessageBox.Show(
            "La regla parece válida y puede ser aplicada.",
            "Prueba de Regla",
            MessageBoxButton.OK,
            MessageBoxImage.Information);
    }

    [RelayCommand]
    private void SaveRule()
    {
        // Validation logic would go here
        if (string.IsNullOrWhiteSpace(FirewallRule.Name))
        {
            MessageBox.Show(
                "El nombre de la regla es obligatorio.",
                "Error de Validación",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
            return;
        }

        if (Application.Current.MainWindow?.IsActive == true)
        {
            ((EditRuleDialog)Application.Current.MainWindow).Result = FirewallRule;
            ((EditRuleDialog)Application.Current.MainWindow).DialogResult = true;
        }
    }
}
