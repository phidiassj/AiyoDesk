using AiyoDesk.CommanandTools;
using Avalonia.Controls;
using DialogHostAvalonia;
using System.Threading.Tasks;

namespace AiyoDesk.CustomControls;

public partial class LicenseDialog : UserControl
{
    public string LicenseUrl { get; set; } = null!;

    public LicenseDialog()
    {
        InitializeComponent();
    }

    private void btnConfirm_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        DialogHost.GetDialogSession("MainDialogHost")?.Close(true);
    }

    private void btnCancel_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        DialogHost.GetDialogSession("MainDialogHost")?.Close(false);
    }

    private void OfficialLink_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        CommandLineExecutor.StartProcess(LicenseUrl);
        Task.Delay(1000);
        chkLicenseRead.IsEnabled = true;
    }

    private void chkLicenseRead_IsCheckedChanged(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (chkLicenseRead.IsChecked!.Value) btnConfirm.IsEnabled = true;
    }
}