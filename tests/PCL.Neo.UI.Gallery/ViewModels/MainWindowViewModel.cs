using System;
using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PCL.Neo.UI.Controls.Primitives;

namespace PCL.Neo.UI.Gallery.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    [ObservableProperty] private Color _accent = NeoTheme.Current?.AccentColor ?? Colors.Transparent;
    [ObservableProperty] private NeoLoadingState _loadingState = NeoLoadingState.Loading;

    partial void OnAccentChanged(Color value)
    {
        NeoTheme.Current?.AccentColor = value;
    }
    
    [RelayCommand]
    private void ChangeState(object? parameter)
    {
        if (parameter is not string state)
            return;

        LoadingState = state switch
        {
            "Idle" => NeoLoadingState.Idle,
            "Loading" => NeoLoadingState.Loading,
            "Failed" => NeoLoadingState.Failed,
            "Success" => NeoLoadingState.Success,
            _ => throw new ArgumentOutOfRangeException()
        };
    }
}