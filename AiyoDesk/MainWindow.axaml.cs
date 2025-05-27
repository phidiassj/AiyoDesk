using AiyoDesk.LocalHost;
using Avalonia.Controls;

namespace AiyoDesk;

public partial class MainWindow : Window
{
    private ServiceCenter serviceCenter = new();
    public MainWindow()
    {
        InitializeComponent();
    }
}

