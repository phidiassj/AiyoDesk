using AiyoDesk.LocalHost;
using Avalonia.Controls;

namespace AiyoDesk;

public partial class MainWindow : Window
{
    private ServiceCenter serviceCenter = new();
    public MainWindow()
    {
        InitializeComponent();

        contentContrainer.Content = new Pages.PageSettings();
    }

    private void NavDrawerSwitch_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        LeftDrawer.LeftDrawerOpened = !(LeftDrawer.LeftDrawerOpened);
    }
}

