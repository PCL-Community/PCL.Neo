using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
<<<<<<< HEAD
=======
using CommunityToolkit.Mvvm.Messaging;
using PCL.Neo.Controls.MyMsg;
using PCL.Neo.Core.Models.Minecraft.Game;
using PCL.Neo.Core.Models.Minecraft.Game.Data;
using PCL.Neo.Core.Models.Minecraft.Java;
using PCL.Neo.Core.Utils;
using PCL.Neo.Services;
>>>>>>> 17b9b08 (refactor(game service): refactor game service. big commit!)
using PCL.Neo.Helpers;
using PCL.Neo.Messages;
using PCL.Neo.Services;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PCL.Neo.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    private Window? _window;
    public INavigationService NavigationService { get; }

    [ObservableProperty]
    private bool _isNavBtn1Checked = true;

    [ObservableProperty]
    private bool _isNavBtn2Checked;

    [ObservableProperty]
    private bool _isNavBtn3Checked;

    [ObservableProperty]
    private bool _isNavBtn4Checked;

    [ObservableProperty]
    private bool _isNavBtn5Checked;

    /// <summary>
    /// 设置按钮的选中状态
    /// </summary>
    private int CheckedBtn
    {
        set
        {
<<<<<<< HEAD
            if (value is < 1 or > 5) return;
            IsNavBtn1Checked = value == 1;
            IsNavBtn2Checked = value == 2;
            IsNavBtn3Checked = value == 3;
            IsNavBtn4Checked = value == 4;
            IsNavBtn5Checked = value == 5;
=======
            set
            {
                if (value is < 1 or > 5) return;
                IsNavBtn1Checked = value == 1;
                IsNavBtn2Checked = value == 2;
                IsNavBtn3Checked = value == 3;
                IsNavBtn4Checked = value == 4;
                IsNavBtn5Checked = value == 5;
            }
        }


        [ObservableProperty] [NotifyPropertyChangedFor(nameof(CheckedBtn))]
        private ViewModelBase? _currentViewModel;

        [ObservableProperty] private ViewModelBase? _currentSubViewModel;

        [ObservableProperty] private bool _canGoBack;

        // 添加新的属性和命令用于PCL II风格主界面
        [ObservableProperty] private string _selectedGameVersion = "1.20.2-Fabric 0.15.7-OptiFine_I7_pre1";

        [ObservableProperty] private bool _isPremiumAccount = false;

        // 添加CurrentUserName属性以解决绑定错误
        [ObservableProperty] private string _currentUserName = "Player";

        // 为了设计时的 DataContext
        public MainWindowViewModel()
        {
            throw new NotImplementedException();
        }

        public MainWindowViewModel(Window window)
        {
            this._window = window;

            // 启用所有 Messenger 注册事件
            IsActive = true;
        }

        public MainWindowViewModel(INavigationService navigationService)
        {
            NavigationService = navigationService;

            // 启用所有 Messenger 注册事件
            IsActive = true;
        }

        protected override void OnActivated()
        {
            base.OnActivated();

            Messenger.Register<MainWindowViewModel, NavigationMessage, Guid>(
                this, NavigationMessage.Channels.Navigated,
                (_, m) => OnNavigated(m));
        }

        public void OnNavigated(NavigationMessage message)
        {
            if (message.IsMainViewModelChanged)
                CurrentViewModel    = message.NewMainViewModel;
            if (message.IsSubViewModelChanged)
                CurrentSubViewModel = message.NewSubViewModel;
            // 更新返回按钮状态
            CanGoBack = NavigationService.CanGoBack;
            // 由外部的页面跳转反向触发设置按钮状态
            UpdateNavBtnState();
        }

        [RelayCommand]
        private void Close()
        {
            _window?.Close();
        }

        [RelayCommand]
        private void Minimize()
        {
            // 确保window不为空
            if (_window != null)
            {
                _window.WindowState = WindowState.Minimized;
            }
        }

        [RelayCommand]
        private void Maximize()
        {
            // 确保window不为空
            if (_window != null)
            {
                _window.WindowState = _window.WindowState == WindowState.Maximized
                    ? WindowState.Normal
                    : WindowState.Maximized;
            }
        }

        [RelayCommand]
        public async Task Navigate(int tag)
        {
            CheckedBtn = tag;
            switch (tag)
            {
                case 1:
                    break;
                case 2:
                    break;
                case 3:
                    // NavigationService.Goto<LinkViewModel>();
                    break;
                case 4:
                    break;
                case 5:
                    // NavigationService.GoBack<OtherViewModel>();
                    break;
                default:
                    Console.WriteLine("Unknown tag");
                    break;
            }
        }

        [RelayCommand]
        public async Task GoBack()
        {
            NavigationService.GoBackAsync();
            // 更新导航按钮状态
            UpdateNavBtnState();
        }

        private void UpdateNavBtnState()
        {
            CheckedBtn = CurrentViewModel switch
            {
                // LinkViewModel => 3,
                //SetupViewModel => 4,
                // MoreViewModel => 4,
                // _ => throw new ArgumentOutOfRangeException() // 有可能切换到子界面，如下载进度界面
                _ => 1
            };
        }

        public void ShowMessageBox((MessageBoxParam, TaskCompletionSource<MessageBoxResult>) messageBox)
        {
        }

        /// <summary>
        /// 强制关闭正在窗口上展示的 MessageBox。
        /// </summary>
        public void CloseMessageBox()
        {
        }

        [RelayCommand]
        private void SwitchToPremium()
        {
            IsPremiumAccount = true;
            // 可以在这里执行切换账户类型的逻辑
        }

        [RelayCommand]
        private void SwitchToOffline()
        {
            IsPremiumAccount = false;
            // 可以在这里执行切换账户类型的逻辑
        }

        [RelayCommand]
        private async Task GameSettings()
        {
            // 打开游戏设置页面
            // 假设HomeViewModel中已经有GameSettings命令的实现
            //if (CurrentViewModel is HomeViewModel homeViewModel)
            //{
            //    await homeViewModel.GameSettingsCommand.ExecuteAsync(null);
            //}
        }

        [RelayCommand]
        private async Task ManageVersions()
        {
            // 打开版本管理页面
            // 假设HomeViewModel中已经有ManageVersions命令的实现
            //if (CurrentViewModel is HomeViewModel homeViewModel)
            //{
            //    await homeViewModel.ManageVersionsCommand.ExecuteAsync(null);
            //}
        }

        [RelayCommand]
        private async Task LaunchGame()
        {
            // 启动游戏
            // 假设HomeViewModel中已经有LaunchGame命令的实现
            //if (CurrentViewModel is HomeViewModel homeViewModel)
            //{
            //    await homeViewModel.LaunchGameCommand.ExecuteAsync(null);
            //}
        }

        // 打开设置页面
        [RelayCommand]
        private async Task OpenSettings()
        {
            // 打开设置页面
            // 实现打开设置页面的逻辑
        }

        // 打开更多选项
        [RelayCommand]
        private async Task OpenMore()
        {
            // 打开更多选项菜单
            // 实现打开更多选项的逻辑
>>>>>>> 17b9b08 (refactor(game service): refactor game service. big commit!)
        }

        [RelayCommand]
        private async Task StartGame()
        {
            var launchOptions = new LaunchOptions
            {
                VersionId = "Create",
                RunnerJava =
                    await JavaRuntime.CreateJavaEntityAsync(
                        @"C:\Users\WhiteCAT\Documents\Java\zulu17.58.21-ca-jdk17.0.15-win_x64\bin"),
                MaxMemoryMB = 4096,
                MinMemoryMB = 512, // 最小内存设为最大内存的1/4，但不低于512MB
                Username = "Steve",
                UUID = Uuid.GenerateUuid("Steve", Uuid.UuidGenerateType.Standard),
                AccessToken = Guid.NewGuid().ToString(),
                WindowWidth = 854,
                WindowHeight = 480,
                FullScreen = false,
                IsOfflineMode = true,

                // 添加额外的JVM参数
                ExtraJvmArgs =
                [
                    "-Dfile.encoding=COMPAT",
                    "-Dstderr.encoding=UTF-8",
                    "-Dstdout.encoding=UTF-8",
                    "-Djdk.lang.Process.allowAmbiguousCommands=true",
                    "-Dfml.ignoreInvalidMinecraftCertificates=True",
                    "-Dfml.ignorePatchDiscrepancies=True",
                    "-Doolloo.jlw.tmpdir=\"C:\\Users\\WhiteCAT\\Desktop\\Games\\PCL2\\PCL\""
                ],

                // 添加额外的游戏参数
                ExtraGameArgs = [],

                // 环境变量
                EnvironmentVariables = new Dictionary<string, string>
                {
                    { "JAVA_TOOL_OPTIONS", "-Dfile.encoding=UTF-8" }
                },

                // 是否启动后关闭启动器
                CloseAfterLaunch = false
            };

            var gameEntity = new GameEntity()
            {
                Profile = new GameProfile
                {
                    Options = launchOptions,
                    Information = new GameInfo()
                    {
                        GameDirectory =
                            @"C:\Users\WhiteCAT\Desktop\Games\PCL2\.minecraft\versions\Create",
                        RootDirectory = @"C:\Users\WhiteCAT\Desktop\Games\PCL2\.minecraft",
                    }
                }
            };


            var result = await gameEntity.StartGame();
        }

        [RelayCommand]
        private async Task StartGame()
        {
            var launchOptions = new LaunchOptions
            {
                VersionId = "1.20.4-Fabric 0.15.11-[轻量通用]",
                RunnerJava =
                    await JavaRuntime.CreateJavaEntityAsync(
                        @"C:\Users\WhiteCAT\Documents\Java\jdk-17\bin"),
                MaxMemoryMB = 4096,
                MinMemoryMB = 512, // 最小内存设为最大内存的1/4，但不低于512MB
                Username = "Steve",
                UUID = Uuid.GenerateUuid("Steve", Uuid.UuidGenerateType.Standard),
                AccessToken = Guid.NewGuid().ToString(),
                WindowWidth = 854,
                WindowHeight = 480,
                FullScreen = false,
                IsOfflineMode = true,

                // 添加额外的JVM参数
                ExtraJvmArgs =
                [
                    "-Dfile.encoding=UTF-8",
                    "-Dstderr.encoding=UTF-8",
                    "-Dstdout.encoding=UTF-8",
                    "-Djdk.lang.Process.allowAmbiguousCommands=true",
                    "-Dfml.ignoreInvalidMinecraftCertificates=True",
                    "-Dfml.ignorePatchDiscrepancies=True",
                    "-Dlog4j2.formatMsgNoLookups=true",
                    "-XX:-OmitStackTraceInFastThrow",
                    "-Doolloo.jlw.tmpdir=\"C:\\Users\\WhiteCAT\\Desktop\\Games\\PCL2\\PCL\""
                ],

                // 添加额外的游戏参数
                ExtraGameArgs = [],

                // 环境变量
                EnvironmentVariables = new Dictionary<string, string>
                {
                    { "JAVA_TOOL_OPTIONS", "-Dfile.encoding=UTF-8" }
                },

                // 是否启动后关闭启动器
                CloseAfterLaunch = false
            };

            var gameEntity = new GameEntity()
            {
                Profile = new GameProfile
                {
                    Options = launchOptions,
                    Information = new GameInfo()
                    {
                        RootDirectory = @"C:\Users\WhiteCAT\Desktop\Games\PCL2\.minecraft",
                        GameDirectory =
                            @"C:\Users\WhiteCAT\Desktop\Games\PCL2\.minecraft\versions\1.20.4-Fabric 0.15.11-[轻量通用]",
                    }
                }
            };


            var result = await gameEntity.StartGame();
        }
    }


    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CheckedBtn))]
    private ViewModelBase? _currentViewModel;

    [ObservableProperty]
    private ViewModelBase? _currentSubViewModel;

    [ObservableProperty]
    private bool _canGoBack;

    // 添加新的属性和命令用于PCL II风格主界面
    [ObservableProperty]
    private string _selectedGameVersion = "1.20.2-Fabric 0.15.7-OptiFine_I7_pre1";

    [ObservableProperty]
    private bool _isPremiumAccount;

    // 添加CurrentUserName属性以解决绑定错误
    [ObservableProperty]
    private string _currentUserName = "Player";

    // 为了设计时的 DataContext
    public MainWindowViewModel()
    {
        throw new NotImplementedException();
    }

    public MainWindowViewModel(Window window)
    {
        _window = window;

        // 启用所有 Messenger 注册事件
        IsActive = true;
    }

    public MainWindowViewModel(INavigationService navigationService)
    {
        NavigationService = navigationService;

        // 启用所有 Messenger 注册事件
        IsActive = true;
    }

    protected override void OnActivated()
    {
        base.OnActivated();

        Messenger.Register<MainWindowViewModel, NavigationMessage, Guid>(
            this, NavigationMessage.Channels.Navigated,
            (_, m) => OnNavigated(m));
    }

    public void OnNavigated(NavigationMessage message)
    {
        if (message.IsMainViewModelChanged)
            CurrentViewModel = message.NewMainViewModel;
        if (message.IsSubViewModelChanged)
            CurrentSubViewModel = message.NewSubViewModel;
        // 更新返回按钮状态
        CanGoBack = NavigationService.CanGoBack;
        // 由外部的页面跳转反向触发设置按钮状态
        UpdateNavBtnState();
    }

    [RelayCommand]
    private void Close()
    {
        _window?.Close();
    }

    [RelayCommand]
    private void Minimize()
    {
        // 确保window不为空
        if (_window != null)
        {
            _window.WindowState = WindowState.Minimized;
        }
    }

    [RelayCommand]
    private void Maximize()
    {
        // 确保window不为空
        if (_window != null)
        {
            _window.WindowState = _window.WindowState == WindowState.Maximized
                ? WindowState.Normal
                : WindowState.Maximized;
        }
    }

    [RelayCommand]
    public async Task Navigate(int tag)
    {
        CheckedBtn = tag;
        switch (tag)
        {
            case 1:
                await NavigationService.GoToAsync<HomeViewModel>();
                break;
            case 2:
                await NavigationService.GoToAsync<DownloadViewModel>();
                break;
            case 3:
                // NavigationService.Goto<LinkViewModel>();
                break;
            case 4:
                await NavigationService.GoToAsync<SetupViewModel>();
                break;
            case 5:
                // NavigationService.GoBack<OtherViewModel>();
                break;
            default:
                Console.WriteLine("Unknown tag");
                break;
        }
    }

    [RelayCommand]
    public async Task GoBack()
    {
        NavigationService.GoBackAsync();
        // 更新导航按钮状态
        UpdateNavBtnState();
    }

    private void UpdateNavBtnState()
    {
        CheckedBtn = CurrentViewModel switch
        {
            HomeViewModel => 1,
            DownloadViewModel => 2,
            // LinkViewModel => 3,
            SetupViewModel => 4,
            // MoreViewModel => 4,
            // _ => throw new ArgumentOutOfRangeException() // 有可能切换到子界面，如下载进度界面
            _ => 1
        };
    }

    public void ShowMessageBox((MessageBoxParam, TaskCompletionSource<MessageBoxResult>) messageBox)
    {
    }

    /// <summary>
    /// 强制关闭正在窗口上展示的 MessageBox。
    /// </summary>
    public void CloseMessageBox()
    {
    }

    [RelayCommand]
    private void SwitchToPremium()
    {
        IsPremiumAccount = true;
        // 可以在这里执行切换账户类型的逻辑
    }

    [RelayCommand]
    private void SwitchToOffline()
    {
        IsPremiumAccount = false;
        // 可以在这里执行切换账户类型的逻辑
    }

    [RelayCommand]
    private async Task GameSettings()
    {
        // 打开游戏设置页面
        // 假设HomeViewModel中已经有GameSettings命令的实现
        if (CurrentViewModel is HomeViewModel homeViewModel)
        {
            await homeViewModel.GameSettingsCommand.ExecuteAsync(null);
        }
    }

    [RelayCommand]
    private async Task ManageVersions()
    {
        // 打开版本管理页面
        // 假设HomeViewModel中已经有ManageVersions命令的实现
        if (CurrentViewModel is HomeViewModel homeViewModel)
        {
            await homeViewModel.ManageVersionsCommand.ExecuteAsync(null);
        }
    }

    [RelayCommand]
    private async Task LaunchGame()
    {
        // 启动游戏
        // 假设HomeViewModel中已经有LaunchGame命令的实现
        if (CurrentViewModel is HomeViewModel homeViewModel)
        {
            await homeViewModel.LaunchGameCommand.ExecuteAsync(null);
        }
    }

    // 打开设置页面
    [RelayCommand]
    private async Task OpenSettings()
    {
        // 打开设置页面
        // 实现打开设置页面的逻辑
    }

    // 打开更多选项
    [RelayCommand]
    private async Task OpenMore()
    {
        // 打开更多选项菜单
        // 实现打开更多选项的逻辑
    }
}
