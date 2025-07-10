using AiyoDesk.CommanandTools;
using AiyoDesk.CustomControls;
using AiyoDesk.LocalHost;
using AiyoDesk.Models;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using DialogHostAvalonia;
using System.Threading.Tasks;
using static AiyoDesk.AppPackages.LlamaCppService;

namespace AiyoDesk.Pages;

public partial class PageMustInstall : UserControl
{
    public MainWindow mainWindow = default!;
    private BackendType hardwareChoose { get; set; } = BackendType.cpu;

    public PageMustInstall()
    {
        InitializeComponent();
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
        bool insConda = (chkInstallConda.IsChecked!.Value);
        bool insLlama = (chkInstallLlamaCpp.IsChecked!.Value);

        string confirmMsg = string.Empty;
        if (insConda) confirmMsg += $"安裝 {ServiceCenter.condaService.PackageName}\n";
        if (insLlama) confirmMsg += $"安裝 {ServiceCenter.llamaCppService.PackageName}\n";

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

        if (string.IsNullOrWhiteSpace(resultMsg))
        {
            await MessageDialogHandler.ShowMessageAsync("要求的作業已經執行完成");
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