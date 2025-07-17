using AiyoDesk.AIModels;
using AiyoDesk.CommanandTools;
using AiyoDesk.Data;
using AiyoDesk.LocalHost;
using AiyoDesk.Models;
using Avalonia.Controls;
using Avalonia.Threading;
using System.Linq;
using static AiyoDesk.AppPackages.LlamaCppService;

namespace AiyoDesk.Pages;

public partial class PageMustInstall : UserControl
{
    public MainWindow mainWindow = default!;
    private BackendType hardwareChoose { get; set; } = BackendType.cpu;
    private RecommandModelItem defaultModel { get; set; } = null!;

    public PageMustInstall()
    {
        InitializeComponent();
        defaultModel = ServiceCenter.modelManager.RecommandModels.First(x => x.Name == "gemma-3-4b-it");
    }

    public void CheckInstalledPackage()
    {
        Dispatcher.UIThread.Invoke(() => {
            if (ServiceCenter.llamaCppService.PackageInstalled) chkInstallLlamaCpp.IsChecked = false;
            if (ServiceCenter.condaService.PackageInstalled) chkInstallConda.IsChecked = false;
        });
    }

    public void ClearAllCheck()
    {
        Dispatcher.UIThread.Invoke(() => {
            chkInstallLlamaCpp.IsChecked = false;
            chkInstallConda.IsChecked = false;
        });
    }

    private void OfficialLink_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (sender == null) return;
        if (sender.Equals(btnLlamaCpp))
        {
            CommandLineExecutor.StartProcess(ServiceCenter.llamaCppService.PackageOfficialUrl);
        }
        else if (sender.Equals(btnConda))
        {
            CommandLineExecutor.StartProcess(ServiceCenter.condaService.PackageOfficialUrl);
        }
    }

    private void rdoModel_Checked(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (sender == null) return;
        if (sender.Equals(rdoModelGemma4b))
        {
            defaultModel = ServiceCenter.modelManager.RecommandModels.First(x => x.Name == "gemma-3-4b-it");
        }
        else if (sender.Equals(rdoModelTwinkle3bF1))
        {
            defaultModel = ServiceCenter.modelManager.RecommandModels.First(x => x.Name == "twinkle-ai.Llama-3.2-3B-F1-Instruct");
        }
        else if (sender.Equals(rdoModelGemma12b))
        {
            defaultModel = ServiceCenter.modelManager.RecommandModels.First(x => x.Name == "gemma-3-12b-it");
        }
        else if (sender.Equals(rdoModelGemma1b))
        {
            defaultModel = ServiceCenter.modelManager.RecommandModels.First(x => x.Name == "gemma-3-1b-it");
        }
    }

    private void rdoHardware_Checked(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (sender == null) return;
        if (sender.Equals(rdoHardwareCPU))
        {
            hardwareChoose = BackendType.cpu;
        }
        else if (sender.Equals(rdoHardwareCuda))
        {
            hardwareChoose = BackendType.cuda;
        }
        else if (sender.Equals(rdoHardwareHip))
        {
            hardwareChoose = BackendType.hip;
        }
        else if (sender.Equals(rdoHardwareIntel))
        {
            hardwareChoose = BackendType.sycl;
        }
    }

    private async void btnStart_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        bool insConda = (chkInstallConda.IsChecked!.Value && !ServiceCenter.condaService.PackageInstalled);
        bool insLlama = (chkInstallLlamaCpp.IsChecked!.Value && (!ServiceCenter.llamaCppService.PackageInstalled || !ServiceCenter.condaService.PackageInstalled));
        bool insModel = (chkInstallAiModel.IsChecked!.Value && !defaultModel.IsModelInstalled());
        bool insOpenWebUI = (chkInstallOpenWebUI.IsChecked!.Value && (!ServiceCenter.openWebUIService.PackageInstalled || !ServiceCenter.condaService.PackageInstalled));

        string confirmMsg = string.Empty;
        if (insConda) confirmMsg += $"安裝 {ServiceCenter.condaService.PackageName}\n";
        if (insLlama) confirmMsg += $"安裝 {ServiceCenter.llamaCppService.PackageName}\n";
        if (insModel) confirmMsg += $"安裝模型 {defaultModel.Name}\n";
        if (insOpenWebUI) confirmMsg += $"安裝 {ServiceCenter.openWebUIService.PackageName}\n";

        if (string.IsNullOrWhiteSpace(confirmMsg))
        {
            await MessageDialogHandler.ShowMessageAsync("沒有需要安裝的套件");
            return;
        }
        else
        {
            confirmMsg = $"即將進行以下作業\n\n" + confirmMsg + "\n確定執行嗎?";
        }
        var ret = await MessageDialogHandler.ShowConfirmAsync(confirmMsg, "安裝提示");
        if (ret == null || !ret.Equals(true)) return;

        if (insModel || insOpenWebUI)
            await MessageDialogHandler.ShowMessageAsync("安裝期間視您的網路狀況可能費時較久，\n請不要關閉本軟體，並且避免電腦進入休眠狀態，\n以免安裝失敗。");

        if (insConda)
        {
            var result = await MessageDialogHandler.ShowLicenseAsync(ServiceCenter.condaService);
            if (!result!.Equals(true)) return;
        }
        if (insLlama)
        {
            var result = await MessageDialogHandler.ShowLicenseAsync(ServiceCenter.llamaCppService);
            if (!result!.Equals(true)) return;
        }
        if (insModel)
        {
            var result = await MessageDialogHandler.ShowLicenseAsync(defaultModel);
            if (!result!.Equals(true)) return;
        }
        if (insOpenWebUI)
        {
            var result = await MessageDialogHandler.ShowLicenseAsync(ServiceCenter.openWebUIService);
            if (!result!.Equals(true)) return;
        }

        string resultMsg = string.Empty;
        if (insConda)
        {
            try
            {
                await ServiceCenter.condaService.PackageInstall();
            }
            catch
            {
                resultMsg += $"安裝 {ServiceCenter.condaService.PackageName} 發生錯誤\n";
            }
        }
        if (insLlama)
        {
            try
            {
                ServiceCenter.llamaCppService.UsingBackend = hardwareChoose;
                if (hardwareChoose == BackendType.cpu)
                {
                    await ServiceCenter.databaseManager.SaveBackendUseGPU(false);
                }
                else
                {
                    await ServiceCenter.databaseManager.SaveBackendUseGPU(true);
                }
                await ServiceCenter.llamaCppService.PackageInstall();
            }
            catch
            {
                resultMsg += $"安裝 {ServiceCenter.llamaCppService.PackageName} 發生錯誤\n";
            }
        }
        if (insModel)
        {
            try
            {
                defaultModel.ModelInstall();
                ServiceCenter.modelManager.LoadInstalledModels();
                var insedModel = ServiceCenter.modelManager.ChatModels.FirstOrDefault(x => x.ModelName == defaultModel.Name);
                if (insedModel != null)
                {
                    ServiceCenter.modelManager.UsingLlmModel = insedModel;
                    SystemSetting systemSetting = ServiceCenter.databaseManager.GetSystemSetting();
                    systemSetting.DefaultModelName = insedModel.ModelName;
                    systemSetting.DefaultModelSubDir = insedModel.SubDir;
                    await ServiceCenter.databaseManager.SaveSystemSetting(systemSetting);
                }
            }
            catch
            {
                resultMsg += $"安裝 {defaultModel.Name} 發生錯誤\n";
            }
        }
        if (insOpenWebUI)
        {
            try
            {
                await ServiceCenter.openWebUIService.PackageInstall();
            }
            catch
            {
                resultMsg += $"安裝 {ServiceCenter.condaService.PackageName} 發生錯誤\n";
            }
        }

        if (string.IsNullOrWhiteSpace(resultMsg))
        {
            await MessageDialogHandler.ShowMessageAsync("要求的作業已經執行完成，\n建議您先關閉本軟體後重新執行。");
            mainWindow.SwitchPage(mainWindow.pagePackages);
        }
        else
        {
            await MessageDialogHandler.ShowMessageAsync($"作業已經完成但可能有錯誤發生\n\n{resultMsg}");
        }

    }

    private async void btnCancel_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        var ret = await MessageDialogHandler.ShowConfirmAsync("下次軟體啟動時將不再顯示這個頁面\n\n確定執行嗎?");
        if (ret == null || !ret.Equals(true)) return;
        await ServiceCenter.databaseManager.SavePassPackageCheck();
        mainWindow.SwitchPage(mainWindow.pageMain);
    }
}