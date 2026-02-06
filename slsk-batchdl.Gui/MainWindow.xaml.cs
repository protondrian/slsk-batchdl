using System.Windows;
using slsk_batchdl.Gui.ViewModels;
using slsk_batchdl.Gui.Views;

namespace slsk_batchdl.Gui;

public partial class MainWindow : Window
{
    private readonly MainViewModel _viewModel;

    public MainWindow()
    {
        InitializeComponent();
        _viewModel = new MainViewModel();
        DataContext = _viewModel;
    }

    private void SettingsButton_Click(object sender, RoutedEventArgs e)
    {
        var settingsVm = new SettingsViewModel();
        var dialog = new SettingsDialog(settingsVm) { Owner = this };

        if (dialog.ShowDialog() == true)
        {
            _viewModel.ApplySettings(settingsVm);
        }
    }
}
