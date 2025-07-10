using AiyoDesk.AppPackages;
using AiyoDesk.CommanandTools;
using AiyoDesk.LocalHost;
using AiyoDesk.Models;
using Avalonia.Controls;
using Avalonia.Threading;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

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
        PackageSetting.IsEnabled = (package.PackageHasActivateParameters || package.PackageCanActivateAndStop);
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
        Dispatcher.UIThread.Invoke(() => {
            if (state) PackageRun.IsEnabled = true;
            manageButtonState();
        });
    }

    internal void resetActButtons()
    {
        PackageRun.IsEnabled = CurrentPackage.PackageCanActivateAndStop;
        PackageStop.IsEnabled = CurrentPackage.PackageCanActivateAndStop;
        manageButtonState();
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
        PackageRun.IsEnabled = false;
        var tsk = Task.Run(async() => {
            try
            {
                await CurrentPackage.PackageActivate();
            }
            catch (Exception ex)
            {
                await Dispatcher.UIThread.Invoke(async () =>
                {
                    await MessageDialogHandler.ShowMessageAsync(ex.Message);
                    resetActButtons();
                });
            }
        });

        await Task.Delay(1);
    }

    private async void PackageStop_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        PackageStop.IsEnabled = false;
        await CurrentPackage.PackageStop();
    }

    private async void PackageSetting_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        var currentSetting = CurrentPackage.PackageSetting;
        if (currentSetting == null)
        {
            currentSetting = new Data.PackageSetting();
            currentSetting.PackageName = CurrentPackage.PackageName;
            currentSetting.LocalPort = currentSetting.LocalPort;
            currentSetting.ActivateCommand = string.Empty;
        }
        var confirm = await MessageDialogHandler.ShowPackageSettingAsync(currentSetting, CurrentPackage.PackageCanActivateAndStop, CurrentPackage.PackageCanActivateAndStop, CurrentPackage.PackageHasActivateParameters);
        if (confirm != null && confirm.Equals(true)) await MessageDialogHandler.ShowMessageAsync("您可能必須重新啟動本軟體，設定才會生效。");
    }

    private async void PackageInstall_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (CurrentPackage.PackageIsMustInstall)
        {
            MainWindow.mainWindow.SwitchPage(MainWindow.mainWindow.pageMustInstall);
        }
        else
        {
            if (CurrentPackage.Equals(ServiceCenter.openWebUIService) && 
                (!ServiceCenter.llamaCppService.PackageInstalled || !ServiceCenter.condaService.PackageInstalled))
            {
                await MessageDialogHandler.ShowMessageAsync("您必須先在本軟體安裝 conda 及 llama.cpp 才能透過本軟體安裝 Open-WebUI。");
                return;
            }
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