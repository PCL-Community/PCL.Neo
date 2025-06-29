using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PCL.Neo.Models.User;
using PCL.Neo.Services;
using PCL.Neo.ViewModels.Home;
using System;
using System.Threading.Tasks;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using CommunityToolkit.Mvvm.Messaging;
using PCL.Neo.Messages;
using SkiaSharp;
using System.IO;

namespace PCL.Neo.ViewModels;

[MainViewModel(typeof(HomeSubViewModel))]
public partial class HomeViewModel : ViewModelBase
{
    private readonly INavigationService _navigationService;
    private readonly UserService _userService;

    #region 用户信息
    [ObservableProperty] private string _currentUserName = "Player";
    [ObservableProperty] private string _currentUserType = "离线账户";
    [ObservableProperty] private string _currentUserInitial = "P";
    [ObservableProperty] private string _currentUserAvatar = string.Empty;
    [ObservableProperty] private bool _isOnline = false;
    [ObservableProperty] private string _selectedGameVersion = "1.20.2-Fabric 0.15.7-OptiFine_I7_pre1";
    [ObservableProperty] private int _memoryAllocation = 4;
    #endregion

    #region 主页布局选择
    [ObservableProperty] private bool _isDefaultLayoutSelected = true;
    [ObservableProperty] private bool _isNewsLayoutSelected = false;
    [ObservableProperty] private bool _isInfoLayoutSelected = false;
    [ObservableProperty] private bool _isSimpleLayoutSelected = false;
    #endregion

    [ObservableProperty]
    private ViewModelBase? _currentSubViewModel;

    [ObservableProperty] private Bitmap? _showImageBitmap;

    public HomeViewModel(INavigationService navigationService, UserService userService)
    {
        _navigationService = navigationService;
        _userService = userService;

        // 订阅用户更改事件
        _userService.CurrentUserChanged += OnCurrentUserChanged;

        // 初始化当前用户信息
        if (_userService.CurrentUser != null)
        {
            UpdateCurrentUserInfo(_userService.CurrentUser);
        }

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

    private void OnCurrentUserChanged(UserInfo? user)
    {
        if (user != null)
        {
            UpdateCurrentUserInfo(user);
        }
    }

    private void UpdateCurrentUserInfo(UserInfo user)
    {
        CurrentUserName    = user.Account.UserName;
        CurrentUserType    = user.GetUserTypeText();
        CurrentUserInitial = user.GetInitial();
    }

    partial void OnIsDefaultLayoutSelectedChanged(bool value)
    {
        if (value && CurrentSubViewModel is HomeSubViewModel subViewModel)
        {
            subViewModel.CurrentLayout = HomeLayoutType.Default;
        }
    }

    partial void OnIsNewsLayoutSelectedChanged(bool value)
    {
        if (value && CurrentSubViewModel is HomeSubViewModel subViewModel)
        {
            subViewModel.CurrentLayout = HomeLayoutType.News;
        }
    }

    partial void OnIsInfoLayoutSelectedChanged(bool value)
    {
        if (value && CurrentSubViewModel is HomeSubViewModel subViewModel)
        {
            subViewModel.CurrentLayout = HomeLayoutType.Info;
        }
    }

    partial void OnIsSimpleLayoutSelectedChanged(bool value)
    {
        if (value && CurrentSubViewModel is HomeSubViewModel subViewModel)
        {
            subViewModel.CurrentLayout = HomeLayoutType.Simple;
        }
    }

    private void UpdateSubViewModel()
    {
        // 当系统导航服务的CurrentSubViewModel更改时，更新本地的CurrentSubViewModel属性
        CurrentSubViewModel = _navigationService.CurrentSubViewModel;
    }

    [RelayCommand]
    private async Task SwitchUser()
    {
        // 实现账户切换逻辑
    }

    [RelayCommand]
    private async Task AddOfflineUser()
    {
        // 实现添加离线账户逻辑
    }

    [RelayCommand]
    private async Task AddMicrosoftUser()
    {
        // 实现添加微软账户逻辑
    }

    [RelayCommand]
    private async Task ManageUsers()
    {
        // 实现管理账户逻辑
    }

    [RelayCommand]
    private async Task ManageVersions()
    {
        // 实现版本管理逻辑
        await _navigationService.GoToAsync<VersionManagerViewModel>();
    }

    [RelayCommand]
    private async Task AddGameDirectory()
    {
        // 实现添加游戏目录逻辑
    }

    [RelayCommand]
    private async Task ScanVersions()
    {
        // 实现扫描版本逻辑
    }

    [RelayCommand]
    private async Task NavigateToDownload()
    {
        // 导航到下载页面
        await _navigationService.GoToAsync<DownloadViewModel>();
    }

    [RelayCommand]
    private async Task LaunchGame()
    {
        // 实现启动游戏逻辑
    }

    [RelayCommand]
    private async Task GameSettings()
    {
        // 实现游戏设置逻辑
    }

    [RelayCommand]
    private async Task LoadUserHeadImg()
    {
        try
        {
            const string    filePath    = @"\res\test_skin.png"; // TODO: replace with actual path
            await using var inputStream = File.OpenRead(filePath);
            using var       skiaStream  = new SKManagedStream(inputStream);
            using var       bitMap      = SKBitmap.Decode(skiaStream);

            var       cropRect = new SKRectI(8, 8, 16, 16);
            using var cropped  = new SKBitmap(cropRect.Width, cropRect.Height);
            bitMap.ExtractSubset(cropped, cropRect);

            using var image = SKImage.FromBitmap(cropped);
            using var data  = image.Encode(SKEncodedImageFormat.Png, 100);
            using var ms    = new MemoryStream(data.ToArray());
            ShowImageBitmap = new Bitmap(ms);
        }
        catch (Exception e)
        {
            // TODO: log this error and tell developter
            Console.WriteLine(e);
            throw;
        }
    }
}