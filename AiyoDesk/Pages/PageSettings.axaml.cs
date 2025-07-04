using AiyoDesk;
using AiyoDesk.LocalHost;
using Avalonia.Controls;

namespace AiyoDesk.Pages;

public partial class PageSettings : UserControl
{

    public PageSettings()
    {
        InitializeComponent();

    }

    private void LogsButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (sender == null) return;
        string msg = "目前沒有相關記錄存在";
        if (sender.Equals(btnSysLogs) && !string.IsNullOrWhiteSpace(ServiceCenter.systemCommander.MessageLogs))
        {
            msg = ServiceCenter.systemCommander.MessageLogs;
        }
        else if (sender.Equals(btnLlamacppLogs) && !string.IsNullOrWhiteSpace(ServiceCenter.llamaCppService.ServiceCli.MessageLogs))
        {
            msg = ServiceCenter.llamaCppService.ServiceCli.MessageLogs;
        }
        else if (sender.Equals(btnOpenWebUILogs) && !string.IsNullOrWhiteSpace(ServiceCenter.openWebUIService.ServiceCli.MessageLogs))
        {
            msg = ServiceCenter.openWebUIService.ServiceCli.MessageLogs;
        }

        txtMessages.Text = msg;
        scrollMessages.ScrollToEnd();
    }

}

