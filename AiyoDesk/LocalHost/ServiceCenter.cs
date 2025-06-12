
using AiyoDesk.AppPackages;
using AiyoDesk.CommanandTools;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace AiyoDesk.LocalHost;

public class ServiceCenter
{
    public static CommandLineExecutor systemCommander = new();
    public static HostedHttpService hostedHttpService { get; set; } = default!;
    public static CondaService condaService { get; set; } = default!;
    public static LlamaCppService llamaCppService { get; set; } = default!;
    public static OpenWebUIService openWebUIService { get; set; } = default!;

    public static bool CondaEnvExists { get; internal set; }

    public Action? InitFinishProcess { get; set; }

    public ServiceCenter()
    {
        _ = activateCondaEnv();
        _ = checkCondaEnvExists();
        hostedHttpService = new();
        condaService = new();
        llamaCppService = new();
        openWebUIService = new();
    }

    public ServiceCenter(Action afterProcess)
    {
        _ = activateCondaEnv();
        _ = checkCondaEnvExists();
        hostedHttpService = new();
        condaService = new();
        llamaCppService = new();
        openWebUIService = new();
        InitFinishProcess = afterProcess;
        InitFinishProcess?.Invoke();
    }

    private async Task activateCondaEnv()
    {
        string execCommand = string.Empty;
        if (checkCondaInstalled())
        {
            string activatePath = Path.Combine(CommandLineExecutor.GetPackageRootPath(), "conda", "condabin", "activate.bat");
            string envPath = Path.Combine(CommandLineExecutor.GetPackageRootPath(), "conda", "envs", "aiyodesk");
            execCommand = $"{activatePath} \"{envPath}\"";
        }
        else
        {
            execCommand = "conda activate aiyodesk";
        }

        List<string> resultList = new();
        await systemCommander.ExecuteCommandWithRealtimeOutputAsync(execCommand, resultLine =>
        {
            resultLine = resultLine.TrimEnd('\n');
            if (!string.IsNullOrWhiteSpace(resultLine)) resultList.Add(resultLine);
        });
        //string results = string.Join('\n', resultList);
        //await Task.Delay(1);
    }

    private async Task checkCondaEnvExists()
    {
        string execCommand = "conda --version";

        List<string> resultList = new();
        await systemCommander.ExecuteCommandWithRealtimeOutputAsync(execCommand, resultLine =>
        {
            resultLine = resultLine.TrimEnd('\n');
            if (!string.IsNullOrWhiteSpace(resultLine)) resultList.Add(resultLine);
        });
        if (resultList.Count > 0 && Regex.IsMatch(resultList.Last().ToLower(), @"^conda\s+(\d+(\.\d+)*?)$"))
        {
            CondaEnvExists = true;
        }
    }

    private bool checkCondaInstalled()
    {
        string packagePath = Path.Combine(CommandLineExecutor.GetPackageRootPath(), "conda", "Scripts", "conda.exe");
        return File.Exists(packagePath);
    }

}
