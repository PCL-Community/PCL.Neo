using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Avalonia.Xaml.Interactions.Core;
using Avalonia.Xaml.Interactions.Custom;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PCL.Neo.Controls.MyMsg;
using PCL.Neo.Core.Service.Accounts.MicrosoftAuth;
using PCL.Neo.Helpers;
using PCL.Neo.Models.User;
using PCL.Neo.Services;
using PCL.Neo.ViewModels.Home;
using PCL.Neo.Views;
using SkiaSharp;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace PCL.Neo.ViewModels;

[DefaultSubViewModel(typeof(HomeSubViewModel))]
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

        // 订阅子视图模型变化
        _navigationService.CurrentSubViewModelChanged += vm => CurrentSubViewModel = vm;

        // 初始化当前用户信息
        if (_userService.CurrentUser != null)
        {
            UpdateCurrentUserInfo(_userService.CurrentUser);
        }
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
        CurrentUserName = user.Account.UserName;
        CurrentUserType = user.GetUserTypeText();
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
        MicrosoftAuthService microsoftAuthService = new MicrosoftAuthService();
        var result = microsoftAuthService.StartDeviceCodeFlow();
        result.Subscribe(
    onNext: async state =>
    {
        switch (state)
        {
            case DeviceFlowAwaitUser awaitUser:
                if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desk)
                {
                    var mainWindow = desk.MainWindow;
                    var topLevel = mainWindow;
                    var clipboard = topLevel.Clipboard;
                    var launcher = topLevel.Launcher;
                    await clipboard.SetTextAsync(awaitUser.UserCode);
                    await launcher.LaunchUriAsync(new Uri(awaitUser.VerificationUri));
                }
                break;

            case DeviceFlowPolling:
                Debug.WriteLine("正在轮询服务器，等待用户验证...");
                break;

            case DeviceFlowDeclined:
                Debug.WriteLine("用户拒绝了授权请求。");
                break;

            case DeviceFlowExpired:
                Debug.WriteLine("设备代码已过期，请重新启动验证流程。");
                break;

            case DeviceFlowBadVerificationCode:
                Debug.WriteLine("验证码无效，请检查后重试。");
                break;

            case DeviceFlowGetAccountInfo:
                Debug.WriteLine("正在获取 Minecraft 账户信息...");
                break;

            case DeviceFlowSucceeded success:
                UpdateCurrentUserInfo(new(success.Account));
                break;

            case DeviceFlowInternetError:
                Debug.WriteLine("网络错误，请检查连接后重试。");
                break;

            case DeviceFlowJsonError:
                Debug.WriteLine("服务器返回的数据格式错误，可能是 API 变更。");
                break;

            case DeviceFlowUnkonw:
                Debug.WriteLine("未知错误，请查看日志。");
                break;

            default:
                Debug.WriteLine($"收到未处理的状态: {state.GetType().Name}");
                break;
        }
    },
    onError: error =>
    {
        Debug.WriteLine($"发生未捕获的错误: {error.Message}");
    },
    onCompleted: () =>
    {
        Debug.WriteLine("验证流程已完成（成功或不可恢复的错误）。");
    }
);
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
        await _navigationService.GotoAsync<VersionManagerViewModel>();
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
        await _navigationService.GotoAsync<DownloadViewModel>();
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
            const string filePath = @"\res\test_skin.png"; // TODO: replace with actual path
            await using var inputStream = File.OpenRead(filePath);
            using var skiaStream = new SKManagedStream(inputStream);
            using var bitMap = SKBitmap.Decode(skiaStream);

            var cropRect = new SKRectI(8, 8, 16, 16);
            using var cropped = new SKBitmap(cropRect.Width, cropRect.Height);
            bitMap.ExtractSubset(cropped, cropRect);

            using var image = SKImage.FromBitmap(cropped);
            using var data = image.Encode(SKEncodedImageFormat.Png, 100);
            using var ms = new MemoryStream(data.ToArray());
            ShowImageBitmap = new Bitmap(ms);
        }
        catch (Exception e)
        {
            // TODO: log this error and tell developter
            Debug.WriteLine(e);
            throw;
        }
    }
}