using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace PCL.Neo.Views.Home;

public partial class ModifyView : UserControl
{
    public ModifyView()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}
