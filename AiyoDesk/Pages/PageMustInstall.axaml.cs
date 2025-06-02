using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace AiyoDesk.Pages;

public partial class PageMustInstall : UserControl
{
    public MainWindow mainWindow = default!;
    private int hardwareChoose { get; set; }
    private bool installGemma { get; set; } = true;
    private bool installOpenWebUI { get; set; } = true;

    public PageMustInstall()
    {
        InitializeComponent();
    }

    private void OfficialLink_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (sender == null) return;
        if (sender.Equals(btnLlamaCpp))
        {
            mainWindow.StartProcess(@"https://github.com/ggml-org/llama.cpp");
        }
        else if (sender.Equals(btnConda))
        {
            mainWindow.StartProcess(@"https://conda-forge.org");
        }
        else if (sender.Equals(btnGemma))
        {
            mainWindow.StartProcess(@"https://deepmind.google/models/gemma");
        }
        else if (sender.Equals(btnOpenWebUI))
        {
            mainWindow.StartProcess(@"https://docs.openwebui.com/");
        }
    }

    private void rdoHardware_Checked(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (sender == null) return;
        if (sender.Equals(rdoHardwareCPU))
        {
            hardwareChoose = 0;
        }
        else if (sender.Equals(rdoHardwareCuda))
        {
            hardwareChoose = 1;
        }
        else if (sender.Equals(rdoHardwareHip))
        {
            hardwareChoose = 2;
        }
        else if (sender.Equals(rdoHardwareIntel))
        {
            hardwareChoose = 3;
        }
        else if (sender.Equals(rdoHardwarePass))
        {
            hardwareChoose = -1;
        }
    }

    private void chkInstallGemma_Checked(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (sender == null || !sender.Equals(chkInstallGemma)) return;
        installGemma = chkInstallGemma.IsChecked!.Value;
    }

    private void chkInstallOpenWebUI_Checked(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (sender == null || !sender.Equals(chkInstallOpenWebUI)) return;
        installOpenWebUI = chkInstallOpenWebUI.IsChecked!.Value;
    }

    private void btnStart_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
    }

    private void btnCancel_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
    }
}