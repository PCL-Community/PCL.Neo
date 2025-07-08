using CommunityToolkit.Mvvm.Input;
using PCL.Neo.Services;
using System.Threading.Tasks;

namespace PCL.Neo.ViewModels.Home;

// TODO)) 暂未使用
public partial class HomeViewModelBackup : ViewModelBase
{
    private readonly INavigationService _navigationService;

    public HomeViewModelBackup(INavigationService navigationService)
    {
        _navigationService = navigationService;
    }

    [RelayCommand]
    private async Task ManageUsers()
    {
        // 导航到用户管理页面
        await _navigationService.GoToAsync<HomeSubViewModel>();
    }

    [RelayCommand]
    private async Task ManageVersions()
    {
        // 导航到版本管理页面
        await _navigationService.GoToAsync<VersionManagerViewModel>();
    }
}
