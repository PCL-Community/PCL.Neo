using PCL.Neo.Services;

namespace PCL.Neo.ViewModels.Online;

[MainViewModel(typeof(OnlineSubViewModel))]
public class OnlineViewModel : ViewModelBase
{
    private readonly INavigationService _navigationService;
}
