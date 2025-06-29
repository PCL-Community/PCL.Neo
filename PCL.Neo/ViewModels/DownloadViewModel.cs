using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PCL.Neo.Services;
using PCL.Neo.ViewModels.Download;
using System.Threading.Tasks;

namespace PCL.Neo.ViewModels;

[MainViewModel(typeof(DownloadGameViewModel))]
public partial class DownloadViewModel : ViewModelBase
{
    private readonly INavigationService _navigationService;

    [ObservableProperty] private string _message = "I am from DownloadViewModel";

    public DownloadViewModel(INavigationService navigationService)
    {
        _navigationService = navigationService;
    }

    [RelayCommand]
    private async Task NavigateToDownloadGame()
    {
        await _navigationService.GoToAsync<DownloadGameViewModel>();
    }

    [RelayCommand]
    private async Task NavigateToDownloadMod()
    {
        await _navigationService.GoToAsync<DownloadModViewModel>();
    }

    [RelayCommand]
    private void Btn_Test1()
    {
        Message = "I am from DownloadViewModel             ੭ ˙ᗜ˙ )੭";
    }

    [RelayCommand]
    private void Btn_Test2()
    {
        Message = "I am from DownloadViewModel (⚭-⚭ ) ੭";
    }
}