
using AiyoDesk.AIModels;
using AiyoDesk.AppPackages;
using AiyoDesk.CommanandTools;
using AiyoDesk.Data;
using Avalonia.Controls;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
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
    public static ComfyUIService comfyUIService { get; set; } = default!;
    public static ServiceCenter serviceCenter { get; set; } = default!;

    public static ModelManager modelManager { get; internal set; } = new();

    public static bool CondaEnvExists { get; internal set; }
    public static IPAddress? ServiceIP { get; set; }

    public Action? InitFinishProcess { get; set; }

    public ServiceCenter()
    {
        serviceCenter = this;
        _ = systemCommander.ActivateCondaEnv();
        hostedHttpService = new();
        condaService = new();
        llamaCppService = new();
        openWebUIService = new();
    }

    public ServiceCenter(Action afterProcess)
    {
        serviceCenter = this;
        _ = systemCommander.ActivateCondaEnv();
        hostedHttpService = new();
        condaService = new();
        llamaCppService = new();
        openWebUIService = new();
        comfyUIService = new();
        InitFinishProcess = afterProcess;
        InitFinishProcess?.Invoke();
    }

    public async IAsyncEnumerable<string> ActivateAutoStartServices()
    {
        var llamaSetting = await databaseManager.GetPackageSetting(llamaCppService.PackageName);
        if (llamaSetting != null && llamaSetting.AutoActivate && !llamaCppService.PackageRunning)
        {
            string errMsg = string.Empty;
            yield return $"正在嘗試啟動 {llamaCppService.PackageName} ...";
            try
            {
                await llamaCppService.PackageActivate();
            }
            catch(Exception ex)
            {
                errMsg = ex.Message;
            }
            if (errMsg != string.Empty)
            {
                yield return $"{llamaCppService.PackageName} 啟動失敗，錯誤:\n{errMsg}";
            }
            else
            {
                yield return $"正在等候 {llamaCppService.PackageName} 完成啟動...";
                await Task.Delay(1000);
                Stopwatch sw = Stopwatch.StartNew();
                while (true)
                {
                    if (llamaCppService.PackageRunning || sw.Elapsed > TimeSpan.FromSeconds(120)) break;
                    await Task.Delay(500);
                }
                sw.Stop();
                if (llamaCppService.PackageRunning)
                {
                    yield return $"{llamaCppService.PackageName} 啟動成功";
                }
                else
                {
                    yield return $"{llamaCppService.PackageName} 因超過最大等候時間啟動失敗，請檢查系統訊息確認失敗原因";
                }
            }
        }
        
        var cmSetting = await databaseManager.GetPackageSetting(comfyUIService.PackageName);
        if (cmSetting != null && cmSetting.AutoActivate && !comfyUIService.PackageRunning)
        {
            string errMsg = string.Empty;
            yield return $"正在嘗試啟動 {cmSetting.PackageName} ...";
            try
            {
                await comfyUIService.PackageActivate();
            }
            catch (Exception ex)
            {
                errMsg = ex.Message;
            }
            if (errMsg != string.Empty)
            {
                yield return $"{comfyUIService.PackageName} 啟動失敗，錯誤:\n{errMsg}";
            }
            else
            {
                yield return $"正在等候 {comfyUIService.PackageName} 完成啟動...";
                await Task.Delay(1000);
                Stopwatch sw = Stopwatch.StartNew();
                while (true)
                {
                    if (comfyUIService.PackageRunning || sw.Elapsed > TimeSpan.FromSeconds(300)) break;
                    await Task.Delay(500);
                }
                sw.Stop();
                if (comfyUIService.PackageRunning)
                {
                    yield return $"{comfyUIService.PackageName} 啟動成功";
                }
                else
                {
                    yield return $"{comfyUIService.PackageName} 因超過最大等候時間啟動失敗，請檢查系統訊息確認失敗原因";
                }
            }
        }
        
        var owuSetting = await databaseManager.GetPackageSetting(openWebUIService.PackageName);
        if (owuSetting != null && owuSetting.AutoActivate && !openWebUIService.PackageRunning)
        {
            string errMsg = string.Empty;
            yield return $"正在嘗試啟動 {openWebUIService.PackageName} ...";
            try
            {
                await openWebUIService.PackageActivate();
            }
            catch (Exception ex)
            {
                errMsg = ex.Message;
            }
            if (errMsg != string.Empty)
            {
                yield return $"{openWebUIService.PackageName} 啟動失敗，錯誤:\n{errMsg}";
            }
            else
            {
                yield return $"正在等候 {openWebUIService.PackageName} 完成啟動...";
                await Task.Delay(1000);
                Stopwatch sw = Stopwatch.StartNew();
                while (true)
                {
                    if (openWebUIService.PackageRunning || sw.Elapsed > TimeSpan.FromSeconds(180)) break;
                    await Task.Delay(500);
                }
                sw.Stop();
                if (openWebUIService.PackageRunning)
                {
                    yield return $"{openWebUIService.PackageName} 啟動成功";
                }
                else
                {
                    yield return $"{openWebUIService.PackageName} 因超過最大等候時間啟動失敗，請檢查系統訊息確認失敗原因";
                }
            }
        }

        var httpSetting = await databaseManager.GetPackageSetting(hostedHttpService.PackageName);
        if (httpSetting != null && httpSetting.AutoActivate && !hostedHttpService.PackageRunning)
        {
            string errMsg = string.Empty;
            yield return $"正在嘗試啟動 {hostedHttpService.PackageName} ...";
            try
            {
                await hostedHttpService.PackageActivate();
            }
            catch (Exception ex)
            {
                errMsg = ex.Message;
            }
            if (errMsg != string.Empty)
            {
                yield return $"{hostedHttpService.PackageName} 啟動失敗，錯誤:\n{errMsg}";
            }
            else
            {
                yield return $"正在等候 {hostedHttpService.PackageName} 完成啟動...";
                await Task.Delay(1000);
                Stopwatch sw = Stopwatch.StartNew();
                while (true)
                {
                    if (hostedHttpService.PackageRunning || sw.Elapsed > TimeSpan.FromSeconds(20)) break;
                    await Task.Delay(500);
                }
                sw.Stop();
                if (hostedHttpService.PackageRunning)
                {
                    yield return $"{hostedHttpService.PackageName} 啟動成功";
                }
                else
                {
                    yield return $"{hostedHttpService.PackageName} 因超過最大等候時間啟動失敗，請檢查系統訊息確認失敗原因";
                }
            }
        }

        yield return $"自動啟動作業完成";
        yield break;
    }

    public Dictionary<string, Dictionary<string, string>> GetServiceMenu()
    {
        Dictionary<string, Dictionary<string, string>> result = new();

        result.Add("交談介面", new Dictionary<string, string>());
        if (llamaCppService.PackageRunning)
        {
            string serviceUrl = string.Empty;
            if (ServiceIP != null) serviceUrl = $"http://{ServiceIP}:{llamaCppService.ServicePort}";
            result["交談介面"].Add(llamaCppService.PackageName, serviceUrl);
        }
        else
        {
            result["交談介面"].Add(llamaCppService.PackageName, string.Empty);
        }
        if (openWebUIService.PackageRunning)
        {
            string serviceUrl = string.Empty;
            if (ServiceIP != null) serviceUrl = $"http://{ServiceIP}:{openWebUIService.ServicePort}";
            result["交談介面"].Add(openWebUIService.PackageName, serviceUrl);
        }
        else
        {
            result["交談介面"].Add(openWebUIService.PackageName, string.Empty);
        }

        result.Add("圖片生成", new Dictionary<string, string>());
        if (comfyUIService.PackageRunning)
        {
            string serviceUrl = string.Empty;
            if (ServiceIP != null) serviceUrl = $"http://{ServiceIP}:{comfyUIService.ServicePort}";
            result["圖片生成"].Add(comfyUIService.PackageName, serviceUrl);
        }
        else
        {
            result["圖片生成"].Add(comfyUIService.PackageName, string.Empty);
        }

        return result;
    }

    public async Task<PackageSetting> SavePackageSetting(PackageSetting packageSetting)
    {
        if (packageSetting.PackageName == llamaCppService.PackageName && 
            packageSetting.LocalPort != llamaCppService.ServicePort && 
            OpenWebUIService.CheckLocalDBFileExists())
        {
            using OpenWebUiDB openWebUiDB = new(OpenWebUIService.GetLocalDBFilePath());
            string originUrl = $"http://127.0.0.1:{llamaCppService.ServicePort}";
            List<ConfigEntity> configs = await openWebUiDB.Configs.Where(x => x.DataJson.Contains(originUrl)).ToListAsync();
            string newUrl = $"http://127.0.0.1:{packageSetting.LocalPort}";
            bool needSave = false;
            foreach (ConfigEntity config in configs)
            {
                config.DataJson = config.DataJson.Replace(originUrl, newUrl);
                openWebUiDB.Configs.Update(config);
                needSave = true;
            }
            if (needSave) await openWebUiDB.SaveChangesAsync();
        }

        PackageSetting result = await databaseManager.SavePackageSetting(packageSetting);
        return result;
    }

    internal async Task Disposing()
    {
        if (openWebUIService.PackageRunning)
        {
            try
            {
                await openWebUIService.PackageStop();
            }
            catch { }
        }
        if (comfyUIService.PackageRunning)
        {
            try
            {
                await comfyUIService.PackageStop();
            }
            catch { }
        }
        if (llamaCppService.PackageRunning)
        {
            try
            {
                await llamaCppService.PackageStop();
            }
            catch { }
        }
        if (hostedHttpService.PackageRunning)
        {
            try
            {
                await hostedHttpService.PackageStop();
            }
            catch { }
        }
    }

}
