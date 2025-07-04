using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace PCL.Neo.Views.Home;

public partial class ExportView : UserControl
{
    public ExportView()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}
