using AiyoDesk.LocalHost;
using AiyoDesk.Pages;
using Avalonia.Controls;
using System;
using System.Threading.Tasks;

namespace AiyoDesk;

public partial class MainWindow : Window
{
    public ServiceCenter serviceCenter { get; internal set; } = new();
    public PagePackages pagePackages { get; internal set; } = new();
    public PageMustInstall pageMustInstall { get; internal set; } = new();

    public MainWindow()
    {
        InitializeComponent();
        pagePackages.mainWindow = this;
        pagePackages.InitializePackages();
        pageMustInstall.mainWindow = this;
        contentContrainer.Content = pageMustInstall;
    }

    public void StartProcess(string commandString)
    {
        System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
        {
            FileName = commandString,
            UseShellExecute = true
        });
    }

    private void NavDrawerSwitch_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        LeftDrawer.LeftDrawerOpened = !(LeftDrawer.LeftDrawerOpened);
    }
}

