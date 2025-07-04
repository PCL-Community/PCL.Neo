using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace PCL.Neo.Views.Home;

public partial class SavesView : UserControl
{
    public SavesView()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}
