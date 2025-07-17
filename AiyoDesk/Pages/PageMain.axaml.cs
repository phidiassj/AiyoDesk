using AiyoDesk.CommanandTools;
using AiyoDesk.LocalHost;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Styling;
using Avalonia.Threading;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

namespace AiyoDesk.Pages;

public partial class PageMain : UserControl
{
    bool inStartupProcessMode = false;

    public PageMain()
    {
        InitializeComponent();
    }

    public async Task SystemStartupProcess()
    {
        inStartupProcessMode = true;
        await foreach(string line in ServiceCenter.serviceCenter.ActivateAutoStartServices())
        {
            tbStartupInfo.Text += $"{line}\n";
        }
        await Task.Delay(1000);
        panelStartupInfo.IsVisible = false;
        panelSystemInfo.IsVisible = true;
        inStartupProcessMode = false;
        RefreshSystemInfo();
    }

    public void RefreshSystemInfo()
    {
        if (inStartupProcessMode) return;

        Dispatcher.UIThread.Invoke(() =>
        {
            removeOldLinkButton(wpIPv4Entry);
            if (ServiceCenter.hostedHttpService.PackageRunning)
            {
                List<IPAddress> ipv4s = LocalIpAddressHelper.GetAllLocalIPv4Addresses();
                if (ipv4s.Count > 0)
                {
                    tbIPv4Prepare.IsVisible = false;
                    foreach (IPAddress ip in ipv4s)
                    {
                        HyperlinkButton btn = new HyperlinkButton();
                        if (ServiceCenter.ServiceIP == null) ServiceCenter.ServiceIP = ip;
                        btn.Content = $"http://{ip}:{ServiceCenter.hostedHttpService.ServicePort}";
                        btn.Click += onLinkClick;
                        wpIPv4Entry.Children.Add(btn);
                    }
                }
                else
                {
                    tbIPv4Prepare.IsVisible = true;
                    tbIPv4Prepare.Text = "無 IPv4 位址";
                }
            }
            else
            {
                tbIPv4Prepare.IsVisible = true;
                tbIPv4Prepare.Text = $"{ServiceCenter.hostedHttpService.PackageName} 未啟動";
            }
            string portList = $"{ServiceCenter.hostedHttpService.ServicePort}、{ServiceCenter.llamaCppService.ServicePort}、{ServiceCenter.openWebUIService.ServicePort}";
            tbUsingPort.Text = portList;
        });
    }

    private void removeOldLinkButton(StackPanel parentPanel)
    {
        try
        {
            foreach(var control in parentPanel.Children)
            {
                if (control is HyperlinkButton tgButton)
                    parentPanel.Children.Remove(control);
            }
        } catch { }
    }

    private void onLinkClick(object? s, RoutedEventArgs e)
    {
        if (s is HyperlinkButton button)
        {
            if (button.Content != null)
            {
                var url = button.Content.ToString();
                if (!string.IsNullOrWhiteSpace(url)) CommandLineExecutor.StartProcess(url);
            }
        }
    }
}