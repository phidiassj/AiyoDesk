using AiyoDesk.AIModels;
using AiyoDesk.CommanandTools;
using AiyoDesk.LocalHost;
using AiyoDesk.Models;
using Avalonia.Controls;
using System;

namespace AiyoDesk.CustomControls;

public partial class ModelPanel : UserControl
{
    public RecommandModelItem SourceModel { get; set; } = default!;

    public ModelPanel()
    {
        InitializeComponent();
        ServiceCenter.modelManager.InstalledStateChanged += toggleInstallButton;
    }

    private void toggleInstallButton(object? s, EventArgs e)
    {
        toggleInstallButton();
    }

    private void toggleInstallButton()
    {
        ModelInstall.IsVisible = !SourceModel.IsModelInstalled();
        ModelUninstall.IsVisible = SourceModel.IsModelInstalled();
    }

    private async void ModelInstall_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (SourceModel.IsModelInstalled())
        {
            _ = await MessageDialogHandler.ShowMessageAsync($"{Name} 已經安裝");
        }
        else
        {
            var confirm = await MessageDialogHandler.ShowConfirmAsync($"即將開始安裝 {SourceModel.Name}，確定執行嗎?", "下載模型確認");
            if (confirm == null || !confirm.Equals(true)) return;
            var result = await MessageDialogHandler.ShowLicenseAsync(SourceModel);
            if (result == null || !result.Equals(true)) return;
            SourceModel.ModelInstall();
            ServiceCenter.modelManager.LoadInstalledModels();
        }
    }

    private async void ModelUninstall_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (!SourceModel.IsModelInstalled())
        {
            _ = await MessageDialogHandler.ShowMessageAsync($"{Name} 尚未安裝");
        }
        else
        {
            var confirm = await MessageDialogHandler.ShowConfirmAsync($"即將開始卸載 {SourceModel.Name}，確定執行嗎?", "下載模型確認");
            if (confirm == null || !confirm.Equals(true)) return;
            SourceModel.ModelUninstall();
            ServiceCenter.modelManager.LoadInstalledModels();
        }
    }

    private void ModelSource_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(SourceModel.OfficialUrl)) return;
        CommandLineExecutor.StartProcess(SourceModel.OfficialUrl);
    }
}