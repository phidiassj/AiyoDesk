using AiyoDesk.LocalHost;
using AiyoDesk.Pages;
using Avalonia.Controls;
using Avalonia.Threading;
using Material.Styles.Controls;
using Material.Styles.Models;
using System;
using System.Threading.Tasks;

namespace AiyoDesk;

public partial class MainWindow : Window
{
    public static MainWindow mainWindow { get; private set; } = default!;
    public ServiceCenter serviceCenter { get; internal set; } = default!;
    public PagePackages pagePackages { get; internal set; } = new();
    public PageModelsManage pageModels { get; internal set; } = new();
    public PageMustInstall pageMustInstall { get; internal set; } = new();
    public PageSettings pageSettings { get; internal set; } = new();

    public MainWindow()
    {
        InitializeComponent();
        mainWindow = this;
        pagePackages.mainWindow = this;
        serviceCenter = new ServiceCenter(() =>
        {
            pagePackages.InitializePackages();
        });
        pageMustInstall.mainWindow = this;
        contentContrainer.Content = pageMustInstall;
        showPageName(pageMustInstall);
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
    }

    private void NavDrawerSwitch_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        LeftDrawer.LeftDrawerOpened = !(LeftDrawer.LeftDrawerOpened);
    }

    private void btnPackages_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        contentContrainer.Content = pagePackages;
        showPageName(pagePackages);
    }

    private void btnModels_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        contentContrainer.Content = pageModels;
        showPageName(pageModels);
    }

    private void btnSettings_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        contentContrainer.Content = pageSettings;
        showPageName(pageSettings);
    }
}

