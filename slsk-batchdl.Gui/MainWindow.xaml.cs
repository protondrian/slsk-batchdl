using System.Windows;
using slsk_batchdl.Gui.ViewModels;

namespace slsk_batchdl.Gui;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        DataContext = new MainViewModel();
    }
}
