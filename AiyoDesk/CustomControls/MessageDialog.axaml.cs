using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using DialogHostAvalonia;
using System.Threading.Tasks;

namespace AiyoDesk.CustomControls;

public partial class MessageDialog : UserControl
{

    public MessageDialog()
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
}