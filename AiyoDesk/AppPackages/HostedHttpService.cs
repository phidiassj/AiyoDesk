
using System;
using System.Threading.Tasks;
using AiyoCoveX.Host.Services;
using AiyoDesk.Data;
using AiyoDesk.LocalHost;

namespace AiyoDesk.AppPackages;

public class HostedHttpService : IAppPackage
{
    public EventHandler<bool>? RunningStateChanged { get; set; }
    public EventHandler<bool>? InstalledStateChanged { get; set; }

    public string PackageName { get; set; } = "自託管 HTTP 服務";
    public string PackageDescription { get; set; } = "本系統所有模組的啟動點，必須啟動本服務才能提供客戶端連接，及執行已安裝的模組或其他套件。";
    public bool PackageRunning { get { return isHttpServiceRunning(); } }
    public bool PackageActivating { get; internal set; } = false;
    public bool PackageInstalled { get; internal set; } = true;
    public bool PackageCanUninstall { get; internal set; } = false;
    public bool PackageCanActivateAndStop { get; internal set; } = true;
    public string PackageOfficialUrl { get; internal set; } = @"";
    public string PackageLicenseUrl { get; internal set; } = string.Empty;
    public bool PackageIsMustInstall { get; internal set; } = true;
    public PackageSetting? PackageSetting { get; set; }
    public bool PackageHasActivateParameters { get; set; } = true;

    public int ServicePort { get; set; } = 16888;
    private HostedService BlazorService { get; set; } = new();

    public HostedHttpService()
    {
        activateProcess();
    }

    public HostedHttpService(int port)
    {
        ServicePort = port;
        activateProcess();
    }

    private void activateProcess()
    {
        _ = loadSetting();
        HostedService.RequestMenuItems = () => ServiceCenter.serviceCenter.GetServiceMenu();
    }

    public async Task PackageActivate()
    {
        if (PackageRunning) return;
        if (BlazorService.ServiceRunning) return;
        PackageActivating = true;
        await BlazorService.StartAsync(ServicePort);
        await Task.Delay(1000);
        PackageActivating = false;
        if (RunningStateChanged != null) RunningStateChanged.Invoke(this, PackageRunning);
    }

    public async Task PackageStop()
    {
        if (!PackageRunning) return;
        if (!BlazorService.ServiceRunning) return;
        await BlazorService.StopAsync();
        await Task.Delay(1000);
        if (RunningStateChanged != null) RunningStateChanged.Invoke(this, PackageRunning);
    }

    public async Task PackageInstall()
    {
        await Task.Delay(1);
    }

    public async Task PackageUninstall()
    {
        await Task.Delay(1);
    }

    private bool isHttpServiceRunning()
    {
        return BlazorService.ServiceRunning;
    }

    private async Task loadSetting()
    {
        var dbSetting = await ServiceCenter.databaseManager.GetPackageSetting(PackageName);
        if (dbSetting == null)
        {
            PackageSetting = new();
            PackageSetting.PackageName = PackageName;
            PackageSetting.LocalPort = ServicePort;
            PackageSetting.ActivateCommand = string.Empty;
        }
        else
        {
            PackageSetting = dbSetting;
        }
    }



}
