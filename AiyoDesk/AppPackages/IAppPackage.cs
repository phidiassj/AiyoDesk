
using AiyoDesk.Data;
using System;
using System.Threading.Tasks;

namespace AiyoDesk.AppPackages;
public interface IAppPackage
{
    string PackageDescription { get; set; }
    string PackageName { get; set; }
    int ServicePort { get; set; }
    bool PackageInstalled { get; }
    bool PackageRunning { get; }
    EventHandler<bool>? RunningStateChanged { get; set; }
    EventHandler<bool>? InstalledStateChanged { get; set; }
    bool PackageCanUninstall { get; }
    bool PackageCanActivateAndStop { get; }
    string PackageOfficialUrl { get; }
    string PackageLicenseUrl { get; }
    bool PackageIsMustInstall { get; }
    PackageSetting? PackageSetting { get; set; }
    bool PackageHasActivateParameters { get; set; }
    bool PackageActivating { get; }

    Task PackageActivate();
    Task PackageInstall();
    Task PackageStop();
    Task PackageUninstall();
}
