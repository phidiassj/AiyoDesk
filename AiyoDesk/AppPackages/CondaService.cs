using AiyoDesk.CommanandTools;
using AiyoDesk.Data;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace AiyoDesk.AppPackages;

public class CondaService : IAppPackage
{
    public EventHandler<bool>? RunningStateChanged { get; set; }
    public EventHandler<bool>? InstalledStateChanged { get; set; }

    public string PackageName { get; set; } = "Conda MiniForge";
    public string PackageDescription { get; set; } = "Conda Miniforge 讓使用者可以建立、管理 Python 虛擬環境。\nAiyoDesk 會將大多數整合套件安裝在 Conda Miniforge 虛擬環境內，達到乾淨安裝、易於遷移、簡單移除的目的。\n即使您原有的環境已經有安裝 Conda Miniforge，也建議您在 AiyoDesk 內再次安裝，兩者並不會互相干擾。";
    public bool PackageRunning { get; internal set; }
    public bool PackageActivating { get; internal set; } = false;
    public bool PackageInstalled { get; internal set; }
    public bool PackageCanUninstall { get; internal set; } = true;
    public bool PackageCanActivateAndStop { get; internal set; } = false;
    public string PackageOfficialUrl { get; internal set; } = @"https://conda-forge.org";
    public string PackageLicenseUrl { get; internal set; } = @"https://github.com/conda-forge/miniforge?tab=License-1-ov-file#readme";
    public bool PackageIsMustInstall { get; internal set; } = true;
    public PackageSetting? PackageSetting { get; set; }
    public bool PackageHasActivateParameters { get; set;} = false;

    public int ServicePort { get; set; } = 0;

    public CondaService()
    {
        PackageInstalled = checkCondaInstalled();
        PackageRunning = PackageInstalled;
    }

    public async Task PackageActivate()
    {
        await Task.Delay(1);
    }

    public async Task PackageStop()
    {
        await Task.Delay(1);
    }

    public async Task PackageInstall()
    {
        await Task.Delay(1);
        if (PackageInstalled) return;
        string scriptPath = Path.Combine(CommandLineExecutor.GetScriptRootPath(), "ins_miniforge.ps1");
        if (!File.Exists(scriptPath)) throw new FileNotFoundException(scriptPath);

        string targetPath = Path.Combine(CommandLineExecutor.GetPackageRootPath(), "conda");
        ProcessStartInfo psi = new ProcessStartInfo
        {
            FileName = "powershell",
            Arguments = $"-ExecutionPolicy Bypass -File \"{scriptPath}\" -CondaPath \"{targetPath}\"",
            UseShellExecute = false
        };
        var proc = Process.Start(psi);
        if (proc == null) throw new Exception("執行安裝 script 發生錯誤");
        proc.WaitForExit();
        PackageInstalled = true;
        PackageRunning = true;
        if (InstalledStateChanged != null) InstalledStateChanged.Invoke(this, true);
    }

    public async Task PackageUninstall()
    {
        await Task.Delay(1);
        if (!PackageInstalled) return;
        string targetPath = Path.Combine(CommandLineExecutor.GetPackageRootPath(), "conda");
        if (!Directory.Exists(targetPath)) return;
        Directory.Delete(targetPath, true);
        PackageInstalled = false;
        PackageRunning = false;
        if (InstalledStateChanged != null) InstalledStateChanged.Invoke(this, false);
    }

    private bool checkCondaInstalled()
    {
        string packagePath = Path.Combine(CommandLineExecutor.GetPackageRootPath(), "conda", "Scripts", "conda.exe");
        return File.Exists(packagePath);
    }
}
