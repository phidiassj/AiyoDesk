
using AiyoDesk.CommanandTools;
using AiyoDesk.Data;
using AiyoDesk.LocalHost;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace AiyoDesk.AppPackages;

public class OpenWebUIService : IAppPackage
{

    public EventHandler<bool>? RunningStateChanged { get; set; }
    public EventHandler<bool>? InstalledStateChanged { get; set; }

    public string PackageName { get; set; } = "Open-WebUI";
    public string PackageDescription { get; set; } = "Open WebUI 是一個開源的 AI 交談框架，支援文字、圖像、語音交談。安裝之後，您可以在本系統客戶端直接啟動，直接與您安裝的 AI 交談。Open WebUI 同時支援線上模型如 ChatGPT、Claude、Gemini 等，經由簡單的設定即可進行線上與本地模型的切換或協作。";
    public bool PackageRunning { get; internal set; }
    public bool PackageActivating { get; internal set; } = false;
    public bool PackageInstalled { get; internal set; }
    public bool PackageCanUninstall { get; internal set; } = true;
    public bool PackageCanActivateAndStop { get; internal set; } = true;
    public string PackageOfficialUrl { get; internal set; } = @"https://docs.openwebui.com";
    public string PackageLicenseUrl { get; internal set; } = @"https://github.com/open-webui/open-webui/blob/main/LICENSE";
    public bool PackageIsMustInstall { get; internal set; } = false;
    public PackageSetting? PackageSetting { get; set; }
    public bool PackageHasActivateParameters { get; set; } = false;

    public int ServicePort { get; set; } = 16890;

    public static string GetLocalDBFilePath()
    {
        return Path.Combine(CommandLineExecutor.GetPackageRootPath(), "conda", "envs", "aiyodesk", "Lib", "site-packages", "open_webui", "data", "webui.db");
    }
    public static bool CheckLocalDBFileExists()
    {
        return File.Exists(GetLocalDBFilePath());
    }

    internal CommandLineExecutor ServiceCli = new();
    private string defaultActivateCommand = string.Empty;

    public OpenWebUIService()
    {
        _ = loadSetting();
        _ = checkOpenWebUIExists();
        _ = CheckOpenWebUIRunning();
        _ = ServiceCli.ActivateCondaEnv();
    }

    public async Task PackageActivate()
    {
        if (await CheckOpenWebUIRunning()) return;

        PackageActivating = true;

        string actCommandString = $" serve --port {ServicePort}";
        if (PackageInstalled)
        {
            string packagePath = Path.Combine(CommandLineExecutor.GetCondaScriptPath(), "open-webui.exe");
            actCommandString = packagePath + actCommandString;
        }
        else
        {
            actCommandString = "open-webui" + actCommandString;
        }

        var tsk = Task.Run(() =>
        {
            _ = ServiceCli.ExecuteCommandWithRealtimeOutputAsync(actCommandString, line => { });
        });

        await Task.Delay(3000);
        Stopwatch sw = Stopwatch.StartNew();
        while (true)
        {
            var isrun = await CheckOpenWebUIRunning();
            if (isrun || sw.Elapsed > TimeSpan.FromSeconds(300)) break;
            await Task.Delay(500);
        }
        sw.Stop();

        if (CheckLocalDBFileExists())
        {
            await checkAdminSettingAsync();
        }

        PackageActivating = false;
    }

    private async Task checkAdminSettingAsync()
    {
        using OpenWebUiClient openWebUiClient = new($"http://127.0.0.1:{ServicePort}");
        AdminDetails? adminDetails = null;
        try
        {
            adminDetails = await openWebUiClient.GetAdminDetailsAsync();
        }
        catch { }
        if (adminDetails == null || string.IsNullOrWhiteSpace(adminDetails.Email))
        {
            AuthSignUpResult? authResponse = null;
            try
            {
                authResponse = await openWebUiClient.AddAuthAsync(new AddAuthBody
                {
                    Email = "desk@aiyo.app",
                    Name = "AiyoDesk",
                    Password = "12365478",
                    ProfileImageUrl = string.Empty,
                    Role = "admin"
                });
            }
            catch { }
            if (authResponse != null && ServiceCenter.databaseManager.GetSystemSetting().OpenWebUIToken != authResponse.token)
            {
                await ServiceCenter.databaseManager.SaveOpenWebUIToken(authResponse.token);
            }
        }

        SystemSetting systemSetting = ServiceCenter.databaseManager.GetSystemSetting();
        using OpenWebUiClient openWebUiClient2 = new($"http://127.0.0.1:{ServicePort}");
        var signInRet = await openWebUiClient2.SignInAsync(new SignInRequest { email = "desk@aiyo.app", password = "12365478" });
        if (signInRet != null)
        {
            if (signInRet.token != systemSetting.OpenWebUIToken)
            {
                systemSetting.OpenWebUIToken = signInRet.token;
                await ServiceCenter.databaseManager.SaveOpenWebUIToken(signInRet.token);
            }
            openWebUiClient2.AddTokenHeader(signInRet.token);
        }

        OpenAiGlobalConfig? openAiGlobalConfig = null;
        try
        {
            openAiGlobalConfig = await openWebUiClient2.GetOpenAiConfigAsync();
        }
        catch { }
        if (openAiGlobalConfig != null)
        {
            string llamaUrl = $"http://127.0.0.1:{ServiceCenter.llamaCppService.ServicePort}";
            if (!openAiGlobalConfig.BaseUrls.Contains(llamaUrl))
            {
                openAiGlobalConfig.BaseUrls.Add(llamaUrl);
                openAiGlobalConfig.ConnectionConfigs[openAiGlobalConfig.BaseUrls.Count.ToString()] = new OpenAiConnectionConfig
                {
                    Enable = true,
                    Tags = ["llama.cpp"],
                    PrefixId = string.Empty,
                    ModelIds = new(),
                    ConnectionType = "external"
                };
                await openWebUiClient2.UpdateOpenAiConfigAsync(openAiGlobalConfig);
            }
        }

        if (ServiceCenter.comfyUIService.PackageInstalled)
        {
            GenerativeSetting? generativeSetting = null;
            try
            {
                generativeSetting = await openWebUiClient2.GetImagesConfigAsync();
            }
            catch { }
            if (generativeSetting != null)
            {
                string comfyAddr = $"http://127.0.0.1:{ServiceCenter.comfyUIService.ServicePort}";
                if (!generativeSetting.enabled || generativeSetting.engine != "comfyui" || generativeSetting.comfyui.COMFYUI_BASE_URL != comfyAddr)
                {
                    if (string.IsNullOrEmpty(generativeSetting.openai.OPENAI_API_KEY)) generativeSetting.openai.OPENAI_API_KEY = " ";
                    generativeSetting.enabled = true;
                    generativeSetting.engine = "comfyui";
                    generativeSetting.comfyui.COMFYUI_BASE_URL = comfyAddr;
                    generativeSetting.comfyui.COMFYUI_API_KEY = " ";
                    generativeSetting.comfyui.COMFYUI_WORKFLOW = OpenWebUiClient.NitroSD_Workflow_Template;
                    var promptNode = generativeSetting.comfyui.COMFYUI_WORKFLOW_NODES.FirstOrDefault(x => x.type == "prompt");
                    if (promptNode == null)
                    {
                        promptNode = new COMFYUI_WORKFLOW_NODE { type = "prompt", key = "text" };
                        promptNode.node_ids.Add("4");
                        generativeSetting.comfyui.COMFYUI_WORKFLOW_NODES.Add(promptNode);
                    }
                    var modelNode = generativeSetting.comfyui.COMFYUI_WORKFLOW_NODES.FirstOrDefault(x => x.type == "model");
                    if (modelNode == null)
                    {
                        modelNode = new COMFYUI_WORKFLOW_NODE { type = "model", key = "ckpt_name" };
                        modelNode.node_ids.Add("1");
                        generativeSetting.comfyui.COMFYUI_WORKFLOW_NODES.Add(modelNode);
                    }
                    var widthNode = generativeSetting.comfyui.COMFYUI_WORKFLOW_NODES.FirstOrDefault(x => x.type == "width");
                    if (widthNode == null)
                    {
                        widthNode = new COMFYUI_WORKFLOW_NODE { type = "width", key = "width" };
                        widthNode.node_ids.Add("6");
                        generativeSetting.comfyui.COMFYUI_WORKFLOW_NODES.Add(widthNode);
                    }
                    var heightNode = generativeSetting.comfyui.COMFYUI_WORKFLOW_NODES.FirstOrDefault(x => x.type == "height");
                    if (heightNode == null)
                    {
                        heightNode = new COMFYUI_WORKFLOW_NODE { type = "height", key = "height" };
                        heightNode.node_ids.Add("6");
                        generativeSetting.comfyui.COMFYUI_WORKFLOW_NODES.Add(heightNode);
                    }
                    var stepsNode = generativeSetting.comfyui.COMFYUI_WORKFLOW_NODES.FirstOrDefault(x => x.type == "steps");
                    if (stepsNode == null)
                    {
                        stepsNode = new COMFYUI_WORKFLOW_NODE { type = "steps", key = "steps" };
                        stepsNode.node_ids.Add("7");
                        generativeSetting.comfyui.COMFYUI_WORKFLOW_NODES.Add(stepsNode);
                    }
                    var seedNode = generativeSetting.comfyui.COMFYUI_WORKFLOW_NODES.FirstOrDefault(x => x.type == "seed");
                    if (seedNode == null)
                    {
                        seedNode = new COMFYUI_WORKFLOW_NODE { type = "seed", key = "seed" };
                        seedNode.node_ids.Add("7");
                        generativeSetting.comfyui.COMFYUI_WORKFLOW_NODES.Add(seedNode);
                    }
                    var result = await openWebUiClient2.UpdateImagesConfigAsync(generativeSetting);
                    ImageSetting imageSetting = new ImageSetting { MODEL = "nitrosd-vibrant_comfyui.safetensors", IMAGE_SIZE = "512x512", IMAGE_STEPS = 2 };
                    _ = await openWebUiClient2.UpdateImageOutputConfigAsync(imageSetting);
                }
            }
        }

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
            var isrun = await CheckOpenWebUIRunning();
            if (!isrun || sw.Elapsed > TimeSpan.FromSeconds(20)) break;
            await Task.Delay(500);
        }
        sw.Stop();
    }

    public async Task PackageInstall()
    {
        if (PackageInstalled) return;
        if (!ServiceCenter.condaService.PackageInstalled || !ServiceCenter.llamaCppService.PackageInstalled) 
            throw new Exception("請先安裝 conda 及 llama.cpp 套件");
        runInstallOrUninstallScript("ins_openwebui.bat");

        await checkOpenWebUIExists();
    }

    public async Task PackageUninstall()
    {
        if (!PackageInstalled) return;
        runInstallOrUninstallScript("del_openwebui.bat");
        await checkOpenWebUIExists();
    }

    public async Task<bool> CheckOpenWebUIRunning()
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

    private void runInstallOrUninstallScript(string scriptFilename)
    {
        string scriptPath = CommandLineExecutor.GetScriptRootPath();
        string activatePath = scriptPath;
        string condaEnv = "aiyodesk";
        if (ServiceCenter.condaService.PackageInstalled)
        {
            activatePath = Path.Combine(CommandLineExecutor.GetPackageRootPath(), "conda", "condabin");
            condaEnv = Path.Combine(CommandLineExecutor.GetPackageRootPath(), "conda", "envs", "aiyodesk");
        }

        ProcessStartInfo psi = new ProcessStartInfo
        {
            FileName = Path.Combine(scriptPath, scriptFilename),
            Arguments = $"\"{activatePath}\" \"{condaEnv}\"",
            UseShellExecute = false,
            WorkingDirectory = scriptPath,
            CreateNoWindow = false
        };
        var proc = Process.Start(psi);
        if (proc == null) throw new Exception("執行安裝 script 發生錯誤");
        proc.WaitForExit();
    }

    private async Task<bool> checkOpenWebUIExists()
    {
        string execCommand = Path.Combine(CommandLineExecutor.GetScriptRootPath(), "chk_openwebui.bat");

        List<string> resultList = new();
        await ServiceCenter.systemCommander.ExecuteCommandWithRealtimeOutputAsync(execCommand, resultLine =>
        {
            resultLine = resultLine.TrimEnd('\n');
            if (!string.IsNullOrWhiteSpace(resultLine)) resultList.Add(resultLine);
        });
        string results = string.Join('\n', resultList);
        if (!string.IsNullOrWhiteSpace(results) && results.Trim().EndsWith("YES"))
        {
            PackageInstalled = true;
            if (InstalledStateChanged != null) InstalledStateChanged.Invoke(this, true);
            return true;
        }
        PackageInstalled = false;
        if (InstalledStateChanged != null) InstalledStateChanged.Invoke(this, false);
        return false;
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
