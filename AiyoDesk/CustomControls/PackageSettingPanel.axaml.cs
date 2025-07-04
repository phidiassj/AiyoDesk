using AiyoDesk.AppPackages;
using AiyoDesk.Data;
using AiyoDesk.LocalHost;
using Avalonia.Controls;
using DialogHostAvalonia;
using System.Threading.Tasks;

namespace AiyoDesk.CustomControls;

public partial class PackageSettingPanel : UserControl
{
    private PackageSetting packageSetting { get; set; } = null!;

    public PackageSettingPanel()
    {
        InitializeComponent();
    }

    public void LoadSetting(PackageSetting setting, bool CanAutoActivate, bool HasServicePort, bool HasStartupParameters)
    {
        packageSetting = setting;
        PackageName.Text = setting.PackageName;
        chkAutoActivate.IsEnabled = CanAutoActivate;
        txtServicePort.IsEnabled = HasServicePort;
        txtActivateParameters.IsEnabled = HasStartupParameters;
        chkAutoActivate.IsChecked = packageSetting.AutoActivate;
        txtServicePort.Text = packageSetting.LocalPort.ToString();
        txtActivateParameters.Text = packageSetting.ActivateCommand ?? string.Empty;
    }

    private async void SaveSettings_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (!int.TryParse(txtServicePort.Text, out int startPort))
        {
            txtServicePort.Focus();
            return;
        }
        packageSetting.AutoActivate = chkAutoActivate.IsChecked!.Value;
        packageSetting.LocalPort = startPort;
        packageSetting.ActivateCommand = txtActivateParameters.Text;
        await ServiceCenter.databaseManager.SavePackageSetting(packageSetting);

        DialogHost.GetDialogSession("MainDialogHost")?.Close(true);
    }

    private void Cancel_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        DialogHost.GetDialogSession("MainDialogHost")?.Close(false);
    }

}