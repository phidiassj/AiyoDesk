using AiyoDesk.CustomControls;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace AiyoDesk.Pages;

public partial class PagePackages : UserControl
{
    public MainWindow mainWindow = default!;
    public PackagePanel hostedHttpPanel = new();

    public PagePackages()
    {
        InitializeComponent();
    }

    public void InitializePackages()
    {
        hostedHttpPanel.InitializePackage(mainWindow.serviceCenter.hostedHttpService);
        HttpServicePanel.Content = hostedHttpPanel;
        hostedHttpPanel.PackageUninstall.IsEnabled = false;
    }

}