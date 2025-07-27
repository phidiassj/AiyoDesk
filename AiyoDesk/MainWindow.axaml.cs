using AiyoDesk.LocalHost;
using AiyoDesk.Models;
using AiyoDesk.Pages;
using Avalonia.Controls;
using Avalonia.Threading;
using DialogHostAvalonia;
using Material.Styles.Controls;
using Material.Styles.Models;
using Microsoft.FluentUI.AspNetCore.Components;
using System;
using System.Linq;

namespace AiyoDesk;

public partial class MainWindow : Window
{
    public static MainWindow mainWindow { get; private set; } = default!;
    public ServiceCenter serviceCenter { get; internal set; } = default!;
    public PageMain pageMain { get; internal set; } = new();
    public PagePackages pagePackages { get; internal set; } = new();
    public PageModelsManage pageModels { get; internal set; } = new();
    public PageMustInstall pageMustInstall { get; internal set; } = new();
    public PageSettings pageSettings { get; internal set; } = new();
    public PageModules pageModules { get; internal set; } = new();

    private bool _forceClosing = false;

    public MainWindow()
    {
        InitializeComponent();
        mainWindow = this;
        pagePackages.mainWindow = this;
        pageMustInstall.mainWindow = this;
        serviceCenter = new ServiceCenter(() =>
        {
            pagePackages.InitializePackages();
            pageMustInstall.CheckInstalledPackage();
            if (ServiceCenter.databaseManager.GetSystemSetting().PassPackageCheck || 
                (ServiceCenter.condaService.PackageInstalled && ServiceCenter.llamaCppService.PackageInstalled))
            {
                SwitchPage(pageMain);
                _ = pageMain.SystemStartupProcess();
            }
            else
            {
                SwitchPage(pageMustInstall);
            }
        });
        this.Closing += MainWindow_Closing;
        setTrayIcon();
        //contentContrainer.Content = pageMain;
        //showPageName(pageMain);
    }

    void setTrayIcon()
    {
        if (ServiceCenter.databaseManager.GetSystemSetting().MinToSystemTray == false) return;

        var trayIcons = TrayIcon.GetIcons(App.Current!);
        if (trayIcons == null) return;
        var trayIcon = trayIcons.FirstOrDefault();
        if (trayIcon == null || trayIcon.Menu == null || trayIcon.Menu.Count() <= 0) return;
        trayIcon.IsVisible = true;
        foreach (var menuItem in trayIcon.Menu)
        {
            if (menuItem == null || !(menuItem is NativeMenuItem item)) continue;
            if (item.Header == "開啟")
            {
                item.Click += (s, e) => {
                    Dispatcher.UIThread.Invoke(() => { 
                        if (this.WindowState == WindowState.Minimized)
                        {
                            this.WindowState = WindowState.Normal;
                        }
                        this.Show(); 
                    });
                };
            }
            else if (item.Header == "退出")
            {
                item.Click += (s, e) => {
                    Dispatcher.UIThread.Invoke(() => {
                        if (this.WindowState == WindowState.Minimized)
                        {
                            this.WindowState = WindowState.Normal;
                        }
                        this.Close(); 
                    });
                };
            }
        }
    }

    public void SwitchPage(object TargetPage)
    {
        Dispatcher.UIThread.Invoke(() => { 
            contentContrainer.Content = TargetPage;
            showPageName(TargetPage);
        });
    }

    public void showSnackBarMessage(string msg)
    {
        Dispatcher.UIThread.Invoke(() => { 
            SnackbarHost.Post(
                new SnackbarModel(msg, TimeSpan.FromSeconds(8)),
                SnackbarHoster.HostName,
                DispatcherPriority.Normal);
        });
    }

    private void showPageName(object TargetPage)
    {
        if (TargetPage.Equals(pagePackages))
        {
            AppBarTitle.Text = "整合套件";
            pagePackages.manageButtonState();
        }
        else if (TargetPage.Equals(pageModels))
        {
            AppBarTitle.Text = "模型管理";
        }
        else if (TargetPage.Equals(pageMustInstall))
        {
            AppBarTitle.Text = "安裝必須套件";
        }
        else if (TargetPage.Equals(pageSettings))
        {
            AppBarTitle.Text = "系統設定";
        }
        else if (TargetPage.Equals(pageMain))
        {
            AppBarTitle.Text = "資訊首頁";
            pageMain.RefreshSystemInfo();
        }
        else if (TargetPage.Equals(pageModules))
        {
            AppBarTitle.Text = "功能模組";
            pageMain.RefreshSystemInfo();
        }
    }

    private void NavDrawerSwitch_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        LeftDrawer.LeftDrawerOpened = !(LeftDrawer.LeftDrawerOpened);
    }

    private void btnPackages_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        SwitchPage(pagePackages);
    }

    private void btnModels_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        SwitchPage(pageModels);
    }

    private void btnSettings_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        SwitchPage(pageSettings);
    }

    private void btnMain_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        SwitchPage(pageMain);
    }

    private void btnModules_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        SwitchPage(pageModules);
    }

    private async void MainWindow_Closing(object? sender, Avalonia.Controls.WindowClosingEventArgs e)
    {
        if (_forceClosing) return;
        e.Cancel = true;

        if (ServiceCenter.hostedHttpService.PackageRunning || 
            ServiceCenter.llamaCppService.PackageRunning || 
            ServiceCenter.openWebUIService.PackageRunning || 
            ServiceCenter.comfyUIService.PackageRunning)
        {
            var result = await MessageDialogHandler.ShowConfirmAsync("仍有背景服務正在運行中，關閉本軟體將同時關閉這些服務。\n關閉服務可能需要一些時間，請不要強制結束本軟體以免資源無法完全釋放。\n\n確定要立即關閉本軟體嗎?");
            if (result == null || !result.Equals(true))
            {
                e.Cancel = true;
                return;
            }
            this.IsEnabled = false;
            await serviceCenter.Disposing();
        }

        _forceClosing = true;
        this.Close();
    }
}

