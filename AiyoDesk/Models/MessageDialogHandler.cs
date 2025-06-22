
using AiyoDesk.AIModels;
using AiyoDesk.AppPackages;
using AiyoDesk.CustomControls;
using DialogHostAvalonia;
using OpenAI.Assistants;
using System.Threading.Tasks;

namespace AiyoDesk.Models;
public class MessageDialogHandler
{

    public static async Task<object?> ShowMessageAsync(string MessageContent, string MessageTitle = "系統訊息")
    {
        var dialog = new MessageDialog();
        dialog.txtMessageTitle.Text = MessageTitle;
        dialog.txtMessageContent.Text = MessageContent;
        dialog.btnConfirm.IsVisible = false;
        dialog.btnCancel.Content = "確定";
        var result = await DialogHost.Show(dialog);
        return result;
    }

    public static async Task<object?> ShowConfirmAsync(
        string MessageContent, string MessageTitle = "系統訊息", 
        string OKButtonText = "確定", string CancelButtonText = "取消")
    {
        var dialog = new MessageDialog();
        dialog.txtMessageTitle.Text = MessageTitle;
        dialog.txtMessageContent.Text = MessageContent;
        dialog.btnConfirm.IsVisible = true;
        dialog.btnConfirm.Content = OKButtonText;
        dialog.btnCancel.Content = CancelButtonText;
        var result = await DialogHost.Show(dialog, "MainDialogHost");
        return result;
    }

    public static async Task<object?> ShowLicenseAsync(IAppPackage appPackage)
    {
        if (string.IsNullOrWhiteSpace(appPackage.PackageLicenseUrl)) return true;
        var dialog = new LicenseDialog();
        dialog.LicenseUrl = appPackage.PackageLicenseUrl;
        dialog.txtMessageTitle.Text = $"{appPackage.PackageName} 的授權協議";
        dialog.btnOfficialLink.Text = appPackage.PackageLicenseUrl;
        dialog.btnConfirm.IsVisible = true;
        var result = await DialogHost.Show(dialog, "MainDialogHost");
        return result;
    }

    public static async Task<object?> ShowLicenseAsync(RecommandModelItem rcModel)
    {
        if (string.IsNullOrWhiteSpace(rcModel.LicenseUrl)) return true;
        var dialog = new LicenseDialog();
        dialog.LicenseUrl = rcModel.LicenseUrl;
        dialog.txtMessageTitle.Text = $"{rcModel.Name} 的授權協議";
        dialog.btnOfficialLink.Text = rcModel.LicenseUrl;
        dialog.btnConfirm.IsVisible = true;
        var result = await DialogHost.Show(dialog, "MainDialogHost");
        return result;
    }

}
