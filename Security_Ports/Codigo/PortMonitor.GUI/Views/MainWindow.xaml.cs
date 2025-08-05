using PortMonitor.GUI.ViewModels;
using System;
using System.Windows;
using System.Windows.Input;

namespace PortMonitor.GUI.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        public MainWindow(MainWindowViewModel viewModel) : this()
        {
            DataContext = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
        }

        private void CloseCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            try
            {
                // Verificar si debe minimizar a la bandeja en lugar de cerrar
                var viewModel = DataContext as MainWindowViewModel;
                if (viewModel?.MinimizeToTrayCommand.CanExecute(null) == true)
                {
                    viewModel.MinimizeToTrayCommand.Execute(null);
                    e.Handled = true;
                    return;
                }

                // Confirmar cierre de la aplicación
                var result = MessageBox.Show(
                    "¿Está seguro de que desea cerrar Port Monitor?\n\nEsto detendrá el monitoreo de puertos.",
                    "Confirmar cierre",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    Application.Current.Shutdown();
                }
                else
                {
                    e.Handled = true;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cerrar la aplicación: {ex.Message}", 
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        protected override void OnStateChanged(EventArgs e)
        {
            try
            {
                if (WindowState == WindowState.Minimized)
                {
                    var viewModel = DataContext as MainWindowViewModel;
                    if (viewModel?.MinimizeToTrayCommand.CanExecute(null) == true)
                    {
                        viewModel.MinimizeToTrayCommand.Execute(null);
                    }
                }
                
                base.OnStateChanged(e);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cambiar el estado de la ventana: {ex.Message}", 
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
