using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;

namespace PCL.Neo.UI.Gallery.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    [ObservableProperty] private Color _accent = NeoTheme.Current?.AccentColor ?? Colors.Transparent;

    partial void OnAccentChanged(Color value)
    {
        NeoTheme.Current?.AccentColor = value;
    }
}