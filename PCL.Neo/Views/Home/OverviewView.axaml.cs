using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace PCL.Neo.Views.Home;

public partial class OverviewView : UserControl
{
    public OverviewView()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}
