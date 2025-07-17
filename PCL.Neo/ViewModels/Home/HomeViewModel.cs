
using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using PCL.Neo.Core.Models.Skin;
using PCL.Neo.Core.Models.Skin.Extensions;
using PCL.Neo.Messages;
using PCL.Neo.Models.User;
using PCL.Neo.Services;
using SkiaSharp;
using System;

namespace PCL.Neo.ViewModels.Home;

[MainViewModel(typeof(HomeSubViewModel))]
public partial class HomeViewModel : ViewModelBase
{
    private readonly INavigationService _navigationService;
    private readonly UserService _userService;

    [ObservableProperty]
    private ViewModelBase? _currentSubViewModel;

    public IImage SkinHeadSource { get; set; }

    public HomeViewModel(INavigationService navigationService, UserService userService)
    {
        _navigationService = navigationService;
        _userService = userService;

        // SkinHeadSource = SkinHelper.Capture(SKBitmap.Decode("G:\\MinecraftYJQ_.png")).ToBitmap();
        // 上一行是添加皮肤预览的，可自行通过 Service 更改

        // 启用所有 Messenger 注册事件
        IsActive = true;
    }
    protected override void OnActivated()
    {
        base.OnActivated();

        Messenger.Register<HomeViewModel, NavigationMessage, Guid>(
            this,
            NavigationMessage.Channels.Navigated,
            (_, m) => OnNavigated(m));
    }

    private void OnNavigated(NavigationMessage message)
    {
        // 订阅子视图模型变化
        CurrentSubViewModel = message.NewSubViewModel;
    }
}
