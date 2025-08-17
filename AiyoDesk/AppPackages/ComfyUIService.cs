using AiyoDesk.CommanandTools;
using AiyoDesk.Data;
using AiyoDesk.LocalHost;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace AiyoDesk.AppPackages;
public class ComfyUIService : IAppPackage
{
    public EventHandler<bool>? RunningStateChanged { get; set; }
    public EventHandler<bool>? InstalledStateChanged { get; set; }

    public string PackageName { get; set; } = "ComfyUI";
    public string PackageDescription { get; set; } = "ComfyUI 是一個基於節點（node-based）的使用者介面，專門用來建立與執行 Stable Diffusion 圖像生成流程。簡單而言，安裝 ComfyUI 可以賦予本軟體生成圖片的功能。\n安裝本組件會同時安裝 TimestepShiftModel 並設定為預設生成圖片的模型。";
    public bool PackageRunning { get; internal set; }
    public bool PackageActivating { get; internal set; } = false;
    public bool PackageInstalled { get; internal set; }
    public bool PackageCanUninstall { get; internal set; } = true;
    public bool PackageCanActivateAndStop { get; internal set; } = true;
    public string PackageOfficialUrl { get; internal set; } = @"https://docs.comfy.org";
    public string PackageLicenseUrl { get; internal set; } = @"https://github.com/comfyanonymous/ComfyUI?tab=GPL-3.0-1-ov-file#readme";
    public bool PackageIsMustInstall { get; internal set; } = false;
    public PackageSetting? PackageSetting { get; set; }
    public bool PackageHasActivateParameters { get; set; } = true;

    public int ServicePort { get; set; } = 16891;

    public CommandLineExecutor ServiceCli = new();
    private string defaultActivateCommand = "--listen 0.0.0.0 --lowvram --disable-smart-memory";

    public ComfyUIService()
    {
        _ = loadSetting();
        PackageInstalled = checkComfyUIInstalled();
        _ = checkComfyUIRunningAsync();
        _ = ServiceCli.ActivateCondaEnv();
    }

    public async Task PackageActivate()
    {
        if (!PackageInstalled) throw new Exception("ComfyUI 必須從本軟體安裝才能在本軟體執行");
        if (await checkComfyUIRunningAsync()) return;

        PackageActivating = true;

        string targetPath = Path.Combine(CommandLineExecutor.GetPackageRootPath(), "ComfyUI");
        string settingPath = Path.Combine(targetPath, "ComfyUI-master", "user", "default", "comfy.settings.json");
        if (File.Exists(settingPath))
        {
            ComfyUIAPI.SetLocale(settingPath, "en");
        }

        string actCommandString = $" --port {ServicePort}";
        if (PackageSetting != null && !string.IsNullOrWhiteSpace(PackageSetting.ActivateCommand))
        {
            actCommandString += $" {PackageSetting.ActivateCommand}";
        }

        string pythonPath = Path.Combine(CommandLineExecutor.GetCondaEnvPath(), "python.exe");
        string packagePath = Path.Combine(CommandLineExecutor.GetPackageRootPath(), "ComfyUI", "ComfyUI-master", "main.py");
        actCommandString = $"{pythonPath} {packagePath}{actCommandString}";

        var tsk = Task.Run(() =>
        {
            _ = ServiceCli.ExecuteCommandWithRealtimeOutputAsync(actCommandString, line => { });
        });

        await Task.Delay(3000);
        Stopwatch sw = Stopwatch.StartNew();
        while (true)
        {
            var isrun = await checkComfyUIRunningAsync();
            if (isrun || sw.Elapsed > TimeSpan.FromSeconds(300)) break;
            await Task.Delay(500);
        }
        sw.Stop();

        PackageActivating = false;
    }

    public async Task PackageStop()
    {
        var tsk = Task.Run(() =>
        {
            ServiceCli.SendInterrupt();
        });
        await Task.Delay(3000);
        Stopwatch sw = Stopwatch.StartNew();
        while (true)
        {
            var isrun = await checkComfyUIRunningAsync();
            if (!isrun || sw.Elapsed > TimeSpan.FromSeconds(20)) break;
            await Task.Delay(500);
        }
        sw.Stop();
    }

    public async Task PackageInstall()
    {
        await Task.Delay(1);
        if (PackageInstalled) return;

        string scriptPath = Path.Combine(CommandLineExecutor.GetScriptRootPath(), "ins_vc.ps1");
        ProcessStartInfo psi = new ProcessStartInfo
        {
            FileName = "powershell",
            Arguments = $"-ExecutionPolicy Bypass -File \"{scriptPath}\"",
            UseShellExecute = false,
            CreateNoWindow = false
        };
        var proc = Process.Start(psi);
        if (proc == null) throw new Exception("安裝 VC++ 執行階段發生錯誤");
        proc.WaitForExit();

        scriptPath = Path.Combine(CommandLineExecutor.GetScriptRootPath(), "ins_comfyui.ps1");
        string targetPath = Path.Combine(CommandLineExecutor.GetPackageRootPath(), "ComfyUI");
        ProcessStartInfo psi2 = new ProcessStartInfo
        {
            FileName = "powershell",
            Arguments = $"-ExecutionPolicy Bypass -File \"{scriptPath}\" -OutputDir \"{targetPath}\"",
            UseShellExecute = false,
            CreateNoWindow = false
        };
        var proc2 = Process.Start(psi2);
        if (proc2 == null) throw new Exception("執行安裝 script 發生錯誤");
        proc2.WaitForExit();

        string settingPath = Path.Combine(targetPath, "ComfyUI-master", "user", "default", "comfy.settings.json");
        if (File.Exists(settingPath))
        {
            ComfyUIAPI.SetLocale(settingPath, "en");
        }

        scriptPath = Path.Combine(CommandLineExecutor.GetScriptRootPath(), "hf_download.ps1");
        if (!File.Exists(scriptPath)) throw new FileNotFoundException(scriptPath);
        targetPath = Path.Combine(CommandLineExecutor.GetPackageRootPath(), "ComfyUI", "ComfyUI-master", "models", "checkpoints", "nitrosd-vibrant_comfyui.safetensors");
        string DownloadUrl = @"https://huggingface.co/ChenDY/NitroFusion/resolve/main/nitrosd-vibrant_comfyui.safetensors";
        ProcessStartInfo psi3 = new ProcessStartInfo
        {
            FileName = "powershell",
            Arguments = $"-ExecutionPolicy Bypass -File \"{scriptPath}\" -ModelUrl \"{DownloadUrl}\" -TargetPath \"{targetPath}\"",
            UseShellExecute = false,
            CreateNoWindow = false
        };
        var proc3 = Process.Start(psi3);
        if (proc3 == null) throw new Exception("執行安裝 圖形模型 發生錯誤");
        proc3.WaitForExit();

        PackageInstalled = checkComfyUIInstalled();
        if (InstalledStateChanged != null) InstalledStateChanged.Invoke(this, false);
    }

    public async Task PackageUninstall()
    {
        await Task.Delay(1);
        if (!PackageInstalled) return;
        string targetPath = Path.Combine(CommandLineExecutor.GetPackageRootPath(), "ComfyUI");
        if (!Directory.Exists(targetPath)) return;
        try
        {
            Directory.Delete(targetPath, true);
        } catch { }
        PackageInstalled = false;
        PackageRunning = false;
        if (InstalledStateChanged != null) InstalledStateChanged.Invoke(this, false);
    }

    private async Task<bool> checkComfyUIRunningAsync()
    {
        var isRunning = await LocalIpAddressHelper.IsPortRunningAsync(ServicePort, 1500);
        if (isRunning != PackageRunning)
        {
            PackageRunning = isRunning;
            RunningStateChanged?.Invoke(this, PackageRunning);
        }
        return isRunning;
    }

    private bool checkComfyUIInstalled()
    {
        string packagePath = Path.Combine(CommandLineExecutor.GetPackageRootPath(), "ComfyUI", "ComfyUI-master", "main.py");
        return File.Exists(packagePath);
    }

    private async Task loadSetting()
    {
        var dbSetting = await ServiceCenter.databaseManager.GetPackageSetting(PackageName);
        if (dbSetting == null)
        {
            PackageSetting = new();
            PackageSetting.PackageName = PackageName;
            PackageSetting.LocalPort = ServicePort;
            PackageSetting.ActivateCommand = defaultActivateCommand;
        }
        else
        {
            PackageSetting = dbSetting;
            if (dbSetting.LocalPort > 0) ServicePort = dbSetting.LocalPort;
        }
    }

}
