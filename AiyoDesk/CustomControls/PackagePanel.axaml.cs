using AiyoDesk.AppPackages;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;

namespace AiyoDesk.CustomControls;

public partial class PackagePanel : UserControl
{
    public IAppPackage CurrentPackage { get; set; } = default!;

    public PackagePanel()
    {
        InitializeComponent();
    }

    public void InitializePackage(IAppPackage package)
    {
        CurrentPackage = package;
        PackageName.Text = package.PackageName;
        PackageDescription.Text = package.PackageDescription;
        manageButtonState();
        package.RunningStateChanged += packageStateChanged;
        package.InstalledStateChanged += packageStateChanged;
    }

    private void packageStateChanged(object? s, bool state)
    {
        Dispatcher.UIThread.Invoke(() => manageButtonState());
    }

    private void manageButtonState()
    {
        PackageInstall.IsVisible = !(CurrentPackage.PackageInstalled);
        PackageUninstall.IsVisible = CurrentPackage.PackageInstalled;
        PackageRun.IsVisible = !(CurrentPackage.PackageRunning);
        PackageStop.IsVisible = CurrentPackage.PackageRunning;
    }

    private async void PackageRun_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        await CurrentPackage.PackageActivate();
    }

    private async void PackageStop_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        await CurrentPackage.PackageStop();
    }

    private void PackageSetting_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
    }

    private void PackageInstall_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
    }

    private void PackageUninstall_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
    }

    private void PackageSource_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
    }
}