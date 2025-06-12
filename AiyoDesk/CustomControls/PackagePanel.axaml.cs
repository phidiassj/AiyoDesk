using AiyoDesk.AppPackages;
using AiyoDesk.CommanandTools;
using AiyoDesk.Models;
using Avalonia.Controls;
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
        PackageRun.IsEnabled = CurrentPackage.PackageCanActivateAndStop;
        PackageStop.IsEnabled = CurrentPackage.PackageCanActivateAndStop;
        PackageUninstall.IsEnabled = CurrentPackage.PackageCanUninstall;
        PackageSource.IsEnabled = !(string.IsNullOrWhiteSpace(CurrentPackage.PackageOfficialUrl));
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

    private async void PackageInstall_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (CurrentPackage.PackageIsMustInstall)
        {
            MainWindow.mainWindow.SwitchPage(MainWindow.mainWindow.pageMustInstall);
        }
        else
        {
            var confirm = await MessageDialogHandler.ShowConfirmAsync($"即將開始安裝 {CurrentPackage.PackageName}，確定執行嗎?", "安裝確認");
            if (confirm == null || !confirm.Equals(true)) return;
            var result = await MessageDialogHandler.ShowLicenseAsync(CurrentPackage);
            if (result == null || !result.Equals(true)) return;
            await CurrentPackage.PackageInstall();
        }
    }

    private async void PackageUninstall_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        var confirm = await MessageDialogHandler.ShowConfirmAsync($"即將開始移除 {CurrentPackage.PackageName}，確定執行嗎?", "移除確認");
        if (confirm == null || !confirm.Equals(true)) return;
        await CurrentPackage.PackageUninstall();
    }

    private void PackageSource_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(CurrentPackage.PackageOfficialUrl)) return;
        CommandLineExecutor.StartProcess(CurrentPackage.PackageOfficialUrl);
    }
}