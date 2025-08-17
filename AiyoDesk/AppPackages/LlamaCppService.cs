
using AiyoDesk.AIModels;
using AiyoDesk.CommanandTools;
using AiyoDesk.Data;
using AiyoDesk.LocalHost;
using System;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace AiyoDesk.AppPackages;
public class LlamaCppService : IAppPackage
{
    public EventHandler<bool>? RunningStateChanged { get; set; }
    public EventHandler<bool>? InstalledStateChanged { get; set; }

    public string PackageName { get; set; } = "llama.cpp";
    public string PackageDescription { get; set; } = "llama.cpp 提供一個輕量、跨平台、易於部署的方式來執行 AI 模型，廣泛支援各種 CPU 及 GPU，並且特別針對低資源環境進行優化，是在本地設備上高效執行 AI 模型的優良選擇。\n本系統所有 AI 功能都依賴 llama.cpp 運作。";
    public bool PackageRunning { get; internal set; }
    public bool PackageActivating { get; internal set; } = false;
    public bool PackageInstalled { get; internal set; }
    public bool PackageCanUninstall { get; internal set; } = true;
    public bool PackageCanActivateAndStop { get; internal set; } = true;
    public string PackageOfficialUrl { get; internal set; } = @"https://github.com/ggml-org/llama.cpp";
    public string PackageLicenseUrl { get; internal set; } = string.Empty;
    public bool PackageIsMustInstall { get; internal set; } = true;
    public PackageSetting? PackageSetting { get; set; }
    public bool PackageHasActivateParameters { get; set; } = true;

    public BackendType? UsingBackend {  get; set; }

    public int ServicePort { get; set; } = 16889;

    public CommandLineExecutor ServiceCli = new();
    private string defaultActivateCommand = "-t 2 -c 32000 -ngl 35 -sm layer --host 0.0.0.0";
    private string defaultCPUCommand = "-t 2 -c 32000 --host 0.0.0.0";

    public LlamaCppService()
    {
        _ = loadSetting();
        PackageInstalled = checkLlamaCppInstalled();
        _ = CheckLlamaCppRunning();
        _ = ServiceCli.ActivateCondaEnv();
    }

    public async Task PackageActivate()
    {
        if (await CheckLlamaCppRunning()) return;

        if (ServiceCenter.modelManager.UsingLlmModel == null)
        {
            throw new Exception($"{PackageName} 啟動失敗，未設定預設語言模型");
        }

        PackageActivating = true;

        InstalledModelItem usingModel = ServiceCenter.modelManager.UsingLlmModel;
        string actCommandString = $" -m \"{usingModel.PathName}\"";
        if (usingModel.Vision && !string.IsNullOrWhiteSpace(usingModel.VisionModel))
        {
            actCommandString += $" --mmproj \"{usingModel.VisionModel}\"";
        }
        if (usingModel.FunctionCall)
        {
            actCommandString += " --jinja -fa";
        }
        if (PackageSetting != null && !string.IsNullOrWhiteSpace(PackageSetting.ActivateCommand))
        {
            actCommandString += $" {PackageSetting.ActivateCommand}";
        }
        actCommandString += $" --port {ServicePort}";

        if (PackageInstalled)
        {
            string packagePath = Path.Combine(CommandLineExecutor.GetPackageRootPath(), "llamacpp", "llama-server.exe");
            actCommandString = packagePath + actCommandString;
        }
        else
        {
            actCommandString = "llama-server.exe" + actCommandString;
        }

        var tsk = Task.Run(() =>
        {
            _ = ServiceCli.ExecuteCommandWithRealtimeOutputAsync(actCommandString, line => { });
        });

        await Task.Delay(3000);
        Stopwatch sw = Stopwatch.StartNew();
        while (true)
        {
            var isrun = await CheckLlamaCppRunning();
            if (isrun || sw.Elapsed > TimeSpan.FromSeconds(120)) break;
            await Task.Delay(500);
        }
        sw.Stop();

        PackageActivating = false;
        //_ = await CheckLlamaCppRunning();
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
            var isrun = await CheckLlamaCppRunning();
            if (!isrun || sw.Elapsed > TimeSpan.FromSeconds(20)) break;
            await Task.Delay(500);
        }
        sw.Stop();
    }

    public async Task PackageInstall()
    {
        if (UsingBackend == null) throw new Exception("尚未設定要使用的後端類型");
        await Task.Delay(1);
        if (PackageInstalled) return;
        string scriptPath = Path.Combine(CommandLineExecutor.GetScriptRootPath(), "ins_llama.ps1");
        if (!File.Exists(scriptPath)) throw new FileNotFoundException(scriptPath);

        string backentText = "cpu";
        if (UsingBackend == BackendType.cuda)
        {
            backentText = "cuda-12.4";
        }
        else if (UsingBackend == BackendType.hip)
        {
            backentText = "hip-radeon";
        }
        else if (UsingBackend == BackendType.sycl)
        {
            backentText = "sycl";
        }

        string targetPath = Path.Combine(CommandLineExecutor.GetPackageRootPath(), "llamacpp");
        ProcessStartInfo psi = new ProcessStartInfo
        {
            FileName = "powershell",
            Arguments = $"-ExecutionPolicy Bypass -File \"{scriptPath}\" -Backend \"{backentText}\" -OutputDir \"{targetPath}\"",
            UseShellExecute = false,
            CreateNoWindow = false
        };
        var proc = Process.Start(psi);
        if (proc == null) throw new Exception("執行安裝 script 發生錯誤");
        proc.WaitForExit();
        PackageInstalled = checkLlamaCppInstalled();
        if (PackageInstalled) await loadSetting();
        if (InstalledStateChanged != null) InstalledStateChanged.Invoke(this, PackageInstalled);
    }

    public async Task PackageUninstall()
    {
        await Task.Delay(1);
        if (!PackageInstalled) return;
        string targetPath = Path.Combine(CommandLineExecutor.GetPackageRootPath(), "llamacpp");
        if (!Directory.Exists(targetPath)) return;
        Directory.Delete(targetPath, true);
        PackageInstalled = false;
        PackageRunning = false;
        if (InstalledStateChanged != null) InstalledStateChanged.Invoke(this, false);
    }

    public async Task<bool> CheckLlamaCppRunning()
    {
        using HttpClient httpClient = new();
        string epUrl = $"http://127.0.0.1:{ServicePort}/health";
        try
        {
            HttpResponseMessage result = await httpClient.GetAsync(epUrl);
            PackageRunning = (result.IsSuccessStatusCode);
        }
        catch
        {
            PackageRunning = false;
        }
        RunningStateChanged?.Invoke(this, PackageRunning);
        return PackageRunning;
    }

    private bool checkLlamaCppInstalled()
    {
        string packagePath = Path.Combine(CommandLineExecutor.GetPackageRootPath(), "llamacpp", "llama-server.exe");
        return File.Exists(packagePath);
    }

    private async Task loadSetting()
    {
        SystemSetting sysSetting = ServiceCenter.databaseManager.GetSystemSetting();

        var dbSetting = await ServiceCenter.databaseManager.GetPackageSetting(PackageName);
        if (dbSetting == null)
        {
            PackageSetting = new();
            PackageSetting.PackageName = PackageName;
            PackageSetting.LocalPort = ServicePort;
            PackageSetting.ActivateCommand = (sysSetting.BackendUseGPU) ? defaultActivateCommand : defaultCPUCommand;
        }
        else
        {
            PackageSetting = dbSetting;
            if (dbSetting.LocalPort > 0) ServicePort = dbSetting.LocalPort;
        }
    }

    public enum BackendType
    {
        cpu,
        cuda,
        hip,
        sycl
    }

}
