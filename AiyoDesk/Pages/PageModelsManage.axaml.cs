using AiyoDesk.AIModels;
using AiyoDesk.CustomControls;
using AiyoDesk.LocalHost;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace AiyoDesk.Pages;

public partial class PageModelsManage : UserControl
{
    public PageModelsManage()
    {
        InitializeComponent();
        loadRecommandModels();
    }

    private void loadRecommandModels()
    {
        foreach(RecommandModelItem rcModel in ServiceCenter.modelManager.RecommandModels)
        {
            ModelPanel modelPanel = new();
            modelPanel.ModelName.Text = rcModel.Name;
            modelPanel.ModelDescription.Text = rcModel.Description;
            modelPanel.lblHardwareNeeded.Text = "硬體需求: " + (rcModel.HardwareRequired == HardwareRequiredType.high ? "高" : (rcModel.HardwareRequired == HardwareRequiredType.medium ? "中" : "低"));
            modelPanel.bdCanVision.IsVisible = rcModel.Vision;
            modelPanel.bdCanTools.IsVisible = rcModel.FunctionCall;
            RecommandContainer.Children.Add(modelPanel);
        }
    }
    

}