using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace PCL.Neo.Views.Home;

public partial class SettingsView : UserControl
{
    public SettingsView()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}
