using AiyoDesk.CustomControls;
using AiyoDesk.LocalHost;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace AiyoDesk.Pages;

public partial class PagePackages : UserControl
{
    public MainWindow mainWindow = default!;
    public PackagePanel hostedHttpPanel = new();
    public PackagePanel condaServicePanel = new();
    public PackagePanel llamacppServicePanel = new();
    public PackagePanel openWebUIServicePanel = new();

    public PagePackages()
    {
        InitializeComponent();
    }

    public void InitializePackages()
    {
        hostedHttpPanel.InitializePackage(ServiceCenter.hostedHttpService);
        hostedHttpPanel.PackageUninstall.IsEnabled = false;
        HttpServicePanel.Content = hostedHttpPanel;
        condaServicePanel.InitializePackage(ServiceCenter.condaService);
        CondaServicePanel.Content = condaServicePanel;
        llamacppServicePanel.InitializePackage(ServiceCenter.llamaCppService);
        LlamaCppServicePanel.Content = llamacppServicePanel;
        openWebUIServicePanel.InitializePackage(ServiceCenter.openWebUIService);
        OpenWebUIServicePanel.Content = openWebUIServicePanel;
    }

}