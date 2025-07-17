using PCL.Neo.Services;

namespace PCL.Neo.ViewModels.Home;

[SubViewModel(typeof(HomeViewModel))]
public class HomeSubViewModel : ViewModelBase
{
    private readonly INavigationService _navigationService;
}
