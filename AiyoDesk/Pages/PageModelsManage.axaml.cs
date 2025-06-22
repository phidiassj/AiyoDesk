using AiyoDesk.AIModels;
using AiyoDesk.CommanandTools;
using AiyoDesk.CustomControls;
using AiyoDesk.LocalHost;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Markup.Xaml;
using System;
using System.Collections.ObjectModel;
using System.IO;

namespace AiyoDesk.Pages;

public partial class PageModelsManage : UserControl
{
    public ObservableCollection<InstalledModelItem>? InstalledModels { get; set; }

    public PageModelsManage()
    {
        InitializeComponent();
        loadRecommandModels();
        loadInstalledModels();
        ServiceCenter.modelManager.InstalledStateChanged += loadInstalledModels;
    }

    private void loadRecommandModels()
    {
        foreach(RecommandModelItem rcModel in ServiceCenter.modelManager.RecommandModels)
        {
            ModelPanel modelPanel = new();
            modelPanel.SourceModel = rcModel;
            modelPanel.ModelSource.IsEnabled = (!string.IsNullOrWhiteSpace(rcModel.OfficialUrl));
            modelPanel.ModelName.Text = rcModel.Name;
            modelPanel.ModelDescription.Text = rcModel.Description;
            modelPanel.lblHardwareNeeded.Text = "硬體需求: " + (rcModel.HardwareRequired == HardwareRequiredType.high ? "高" : (rcModel.HardwareRequired == HardwareRequiredType.medium ? "中" : "低"));
            modelPanel.bdCanVision.IsVisible = rcModel.Vision;
            modelPanel.bdCanTools.IsVisible = rcModel.FunctionCall;
            modelPanel.ModelInstall.IsVisible = !(rcModel.IsModelInstalled());
            modelPanel.ModelUninstall.IsVisible = (rcModel.IsModelInstalled());
            RecommandContainer.Children.Add(modelPanel);
        }
    }

    private void loadInstalledModels(object? s, EventArgs e)
    {
        loadInstalledModels();
    }

    private void loadInstalledModels()
    {
        InstalledModels = new ObservableCollection<InstalledModelItem>(ServiceCenter.modelManager.ChatModels);
        InstalledModelContainer.ItemsSource = InstalledModels;
    }
}