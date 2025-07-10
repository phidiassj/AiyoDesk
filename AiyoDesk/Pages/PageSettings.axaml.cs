using AiyoDesk;
using AiyoDesk.AIModels;
using AiyoDesk.Data;
using AiyoDesk.LocalHost;
using Avalonia.Controls;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace AiyoDesk.Pages;

public partial class PageSettings : UserControl
{
    public ObservableCollection<InstalledModelItem>? InstalledModels { get; set; }
    private SystemSetting systemSetting { get; set; } = null!;
    private bool initSuccess = false;
    private bool needSave = false;

    public PageSettings()
    {
        InitializeComponent();
        loadInstalledModels();
        LoadSystemSettings();
        ServiceCenter.modelManager.InstalledStateChanged += loadInstalledModels;
        initSuccess = true;
    }

    private void LoadSystemSettings()
    {
        systemSetting = ServiceCenter.databaseManager.GetSystemSetting();
        chkAutoRunAtStartup.IsChecked = systemSetting.AutoRunAtStartup;
        chkDisplaySystemTray.IsChecked = systemSetting.MinToSystemTray;
        if (InstalledModels != null && systemSetting.DefaultModelName != null && systemSetting.DefaultModelSubDir != null)
        {
            var selected = InstalledModels.FirstOrDefault(x => 
                x.ModelName == systemSetting.DefaultModelName && 
                x.SubDir == systemSetting.DefaultModelSubDir);
            if (selected != null)
            {
                cbModelList.SelectedItem = selected;
                ServiceCenter.modelManager.UsingLlmModel = selected;
            }
            else if (cbModelList.Items.Count > 0)
            {
                cbModelList.SelectedIndex = 0;
                ServiceCenter.modelManager.UsingLlmModel = ServiceCenter.modelManager.ChatModels.First();
            }
        }
    }
    private async Task saveSystemSettings()
    {
        if (!initSuccess) return;
        await ServiceCenter.databaseManager.SaveSystemSetting(systemSetting);
    }

    private void loadInstalledModels(object? s, EventArgs e)
    {
        loadInstalledModels();
    }
    private void loadInstalledModels()
    {
        InstalledModels = new ObservableCollection<InstalledModelItem>(ServiceCenter.modelManager.ChatModels);
        cbModelList.ItemsSource = InstalledModels;
    }

    private void Settings_IsCheckedChanged(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (!initSuccess) return;
        systemSetting.AutoRunAtStartup = chkAutoRunAtStartup.IsChecked!.Value;
        systemSetting.MinToSystemTray = chkDisplaySystemTray.IsChecked!.Value;
        needSave = true;
    }
    private void cbModelList_SelectionChanged(object? sender, Avalonia.Controls.SelectionChangedEventArgs e)
    {
        if (!initSuccess) return;
        if (cbModelList.SelectedItem != null)
        {
            InstalledModelItem selectedModel = (InstalledModelItem)cbModelList.SelectedItem;
            systemSetting.DefaultModelName = selectedModel.ModelName;
            systemSetting.DefaultModelSubDir = selectedModel.SubDir;
            ServiceCenter.modelManager.UsingLlmModel = selectedModel;
        }
        else
        {
            systemSetting.DefaultModelName = null;
            systemSetting.DefaultModelSubDir = null;
        }
        needSave = true;
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

    private async void UserControl_LostFocus_1(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (needSave) await saveSystemSettings();
    }
}

