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

    public void ClearAllCheck()
    {
        Dispatcher.UIThread.Invoke(() => {
            chkInstallLlamaCpp.IsChecked = false;
            chkInstallConda.IsChecked = false;
            chkInstallGemma.IsChecked = false;
            chkInstallOpenWebUI.IsChecked = false;
        });
    }

    private void OfficialLink_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (sender == null) return;
        if (sender.Equals(btnLlamaCpp))
        {
            CommandLineExecutor.StartProcess(@"https://github.com/ggml-org/llama.cpp");
        }
        else if (sender.Equals(btnConda))
        {
            CommandLineExecutor.StartProcess(@"https://conda-forge.org");
        }
        else if (sender.Equals(btnGemma))
        {
            CommandLineExecutor.StartProcess(@"https://deepmind.google/models/gemma");
        }
        else if (sender.Equals(btnOpenWebUI))
        {
            CommandLineExecutor.StartProcess(@"https://docs.openwebui.com/");
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
        bool insGemma = (chkInstallGemma.IsChecked!.Value);
        bool insOpenWebUI = (chkInstallOpenWebUI.IsChecked!.Value);

        string confirmMsg = string.Empty;
        if (insConda) confirmMsg += $"安裝 {ServiceCenter.condaService.PackageName}\n";
        if (insLlama) confirmMsg += $"安裝 {ServiceCenter.llamaCppService.PackageName}\n";
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
                await ServiceCenter.llamaCppService.PackageInstall();
            }
            catch
            {
                resultMsg += $"安裝 {ServiceCenter.llamaCppService.PackageName} 發生錯誤\n";
            }
        }
        if (insOpenWebUI)
        {
            try
            {
                _ = ServiceCenter.openWebUIService.PackageInstall();
            }
            catch
            {
                resultMsg += $"安裝 {ServiceCenter.openWebUIService.PackageName} 發生錯誤\n";
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
        var ret = await MessageDialogHandler.ShowMessageAsync("測試訊息內容", "測試訊息標題");
        await Task.Delay(1);
    }
}