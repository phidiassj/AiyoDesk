
using AiyoDesk.AIModels;
using AiyoDesk.AppPackages;
using AiyoDesk.CommanandTools;
using AiyoDesk.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace AiyoDesk.LocalHost;

public class ServiceCenter
{
    public static DatabaseManager databaseManager { get; internal set; } = new();

    public static CommandLineExecutor systemCommander = new();
    public static HostedHttpService hostedHttpService { get; set; } = default!;
    public static CondaService condaService { get; set; } = default!;
    public static LlamaCppService llamaCppService { get; set; } = default!;
    public static OpenWebUIService openWebUIService { get; set; } = default!;

    public static ModelManager modelManager { get; internal set; } = new();

    public static bool CondaEnvExists { get; internal set; }

    public Action? InitFinishProcess { get; set; }

    public ServiceCenter()
    {
        _ = systemCommander.ActivateCondaEnv();
        hostedHttpService = new();
        condaService = new();
        llamaCppService = new();
        openWebUIService = new();
    }

    public ServiceCenter(Action afterProcess)
    {
        _ = systemCommander.ActivateCondaEnv();
        hostedHttpService = new();
        condaService = new();
        llamaCppService = new();
        openWebUIService = new();
        InitFinishProcess = afterProcess;
        InitFinishProcess?.Invoke();
    }


}
