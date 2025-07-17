using PCL.Neo.Services;

namespace PCL.Neo.ViewModels.Online;

[SubViewModel(typeof(OnlineViewModel))]
public class OnlineSubViewModel : ViewModelBase
{
    private readonly INavigationService _navigationService;
}
