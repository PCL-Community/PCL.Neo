using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Input;
using PCL.Neo.Models.Minecraft.Game;
using PCL.Neo.Models.Minecraft.Game.Data;
using PCL.Neo.Models.User;
using PCL.Neo.Services;
using PCL.Neo.ViewModels;
using PCL.Neo.ViewModels.Download;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Runtime.InteropServices;
using PCL.Neo.Core.Models.Minecraft.Game;
using PCL.Neo.Core.Models.Minecraft.Java;
using PCL.Neo.Core.Service.Accounts.Storage;
using PCL.Neo.Jobs;
using System.Diagnostics;

namespace PCL.Neo.ViewModels.Home;

// 定义主页布局类型
public enum HomeLayoutType
{
    Default,
    News,
    Info,
    Simple
}

[SubViewModel(typeof(HomeViewModel))]
public partial class HomeSubViewModel : ViewModelBase
{
    private readonly INavigationService _navigationService;
    private readonly GameLauncher _gameLauncher;
    private readonly UserService _userService;
    private readonly StorageService _storageService;
    private readonly GameService _gameService;
    private readonly GameSettingsViewModel _gameSettingsViewModel;

    [ObservableProperty]
    private ObservableCollection<GameVersion> _gameVersions = new();

    [ObservableProperty]
    private GameVersion? _selectedGameVersion;

    [ObservableProperty]
    private ObservableCollection<UserInfo> _users = new();

    [ObservableProperty]
    private UserInfo? _selectedUser;

    [ObservableProperty]
    private bool _isLaunching;

    [ObservableProperty]
    private string _statusMessage = "等待启动";

    [ObservableProperty]
    private HomeLayoutType _currentLayout = HomeLayoutType.Default;

    [ObservableProperty]
    private string _newsContent = string.Empty;

    [ObservableProperty]
    private List<GameNewsItem> _newsItems = new();

    [ObservableProperty]
    private List<GameInfoItem> _infoItems = new();

    // 首页布局可见性
    [ObservableProperty]
    private bool _isDefaultLayoutVisible = true;

    [ObservableProperty]
    private bool _isNewsLayoutVisible = false;

    [ObservableProperty]
    private bool _isInfoLayoutVisible = false;

    [ObservableProperty]
    private bool _isSimpleLayoutVisible = false;

    public HomeSubViewModel(
        INavigationService navigationService,
        UserService userService,
        StorageService storageService,
        GameService gameService,
        GameLauncher gameLauncher,
        GameSettingsViewModel gameSettingsViewModel)
    {
        _navigationService = navigationService;
        _userService = userService;
        _storageService = storageService;
        _gameService = gameService;
        _gameLauncher = gameLauncher;
        _gameSettingsViewModel = gameSettingsViewModel;

        // 加载游戏版本
        LoadGameVersions();

        // 加载用户列表
        InitializeUserList();
    }

    private async void LoadGameVersions()
    {
        try
        {
            StatusMessage = "正在加载版本列表...";
            var versions = await Versions.GetLocalVersionsAsync(GameService.DefaultGameDirectory);

            GameVersions.Clear();
            foreach (var version in versions)
            {
                var gameVersion = new GameVersion
                {
                    Id = version.Id,
                    Name = version.Name,
                    Type = version.Type,
                    ReleaseTime = version.ReleaseTime
                };
                GameVersions.Add(gameVersion);
            }

            // 按发布时间排序，最新的在前面
            var sortedVersions = GameVersions.OrderByDescending(v => v.ReleaseTime).ToList();
            GameVersions.Clear();
            foreach (var version in sortedVersions)
            {
                GameVersions.Add(version);
            }

            if (GameVersions.Count > 0)
            {
                SelectedGameVersion = GameVersions[0];
            }

            StatusMessage = $"已加载 {GameVersions.Count} 个版本";
        }
        catch (Exception ex)
        {
            StatusMessage = $"加载版本列表失败: {ex.Message}";
        }
    }

    private void InitializeUserList()
    {
        // 监听用户列表变化
        Users = new ObservableCollection<UserInfo>(_userService.Users);

        // 设置当前选中用户
        SelectedUser = _userService.CurrentUser;

        // 监听用户切换
        this.PropertyChanged += (sender, args) =>
        {
            if (args.PropertyName == nameof(SelectedUser) && SelectedUser != null)
            {
                _userService.SwitchUser(SelectedUser);
            }
        };
    }

    [RelayCommand]
    private async Task NavigateToDownloadMod()
    {
        await _navigationService.GoToAsync<DownloadModViewModel>();
    }

    [RelayCommand]
    private async Task NavigateToJob()
    {
        // TODO: remove this testing command
        var js = Ioc.Default.GetRequiredService<JobService>();
        js.Clear();
        _ = js.Submit(new TestJob1()).RunInNewTask();
        _ = js.Submit(new TestJob1 {IntervalFactor = 2.0}).RunInNewTask();
        await this._navigationService.GoToAsync<JobViewModel>();
    }

    [RelayCommand]
    private void ManageVersions()
    {
        // TODO: 实现版本管理功能
        StatusMessage = "版本管理功能开发中...";
    }

    [RelayCommand]
    private async Task OpenGameSettings()
    {
        // 导航到游戏设置视图
        _navigationService.NavigateTo(Ioc.Default.GetRequiredService<HomeViewModelBackup>(), _gameSettingsViewModel);

        // 如果版本ID不为空，初始化为当前选中的版本
        if (SelectedGameVersion != null)
        {
            await _gameSettingsViewModel.Initialize(SelectedGameVersion.Id);
        }
    }

    [RelayCommand]
    private async Task AddUser()
    {
        try
        {
            // TODO: 实现添加用户的UI
            var username = "NewPlayer";  // 这里应该从UI获取输入

            var newUser = await _userService.AddUserAsync(username);

            // 更新用户列表
            Users.Clear();
            foreach (var user in _userService.Users)
            {
                Users.Add(user);
            }

            // 选择新添加的用户
            SelectedUser = newUser;
        }
        catch (Exception ex)
        {
            StatusMessage = $"添加用户失败: {ex.Message}";
        }
    }

    [RelayCommand]
    private async Task LaunchGame()
    {
        if (SelectedGameVersion == null)
        {
            StatusMessage = "请先选择游戏版本";
            return;
        }

        if (SelectedUser == null)
        {
            StatusMessage = "请先选择用户";
            return;
        }

        // 检查Java路径是否存在
        if (string.IsNullOrEmpty(_gameSettingsViewModel.JavaPath) || !File.Exists(_gameSettingsViewModel.JavaPath))
        {
            StatusMessage = "无效的Java路径，请在设置中选择正确的Java可执行文件";
            return;
        }
        // TODO)) 默认Java版本应该设置3个（参考Modrinth启动器）
        //  此处应该判断的应该是当前要启动的那一个游戏实例设置的Java
        // if (!_gameService.IsJavaCompatibleWithGame(_gameSettingsViewModel.JavaPath, SelectedGameVersion.Id))
        // {
        //     // 提示用户但不阻止启动
        //     StatusMessage = "警告：当前Java版本可能与所选Minecraft版本不兼容";
        //
        //     // 可以在这里添加对话框提示，让用户确认是否继续
        //     // 这里简化处理，直接等待3秒后继续
        //     await Task.Delay(3000);
        // }

        try
        {
            IsLaunching = true;
            StatusMessage = "正在启动游戏...";

            var accessToken = SelectedUser.Account switch
            {
                MsaAccount msa => msa.McAccessToken,
                YggdrasilAccount yggdrasil => yggdrasil.McAccessToken,
                OfflineAccount => Guid.NewGuid().ToString()
            };

            // 创建完善的启动选项
            var launchOptions = new PCL.Neo.Core.Models.Minecraft.Game.LaunchOptions
            {
                VersionId = SelectedGameVersion.Id,
                JavaPath = _gameSettingsViewModel.JavaPath,
                MinecraftRootDirectory = _gameSettingsViewModel.GameDirectory,
                GameDirectory = _gameSettingsViewModel.GameDirectory,
                MaxMemoryMB = _gameSettingsViewModel.MemoryAllocation,
                MinMemoryMB = Math.Max(512, _gameSettingsViewModel.MemoryAllocation / 4), // 最小内存设为最大内存的1/4，但不低于512MB
                Username = SelectedUser.Username,
                UUID = SelectedUser.Uuid,
                AccessToken = string.IsNullOrEmpty(accessToken) ? Guid.NewGuid().ToString() : accessToken,
                WindowWidth = _gameSettingsViewModel.GameWidth,
                WindowHeight = _gameSettingsViewModel.GameHeight,
                FullScreen = _gameSettingsViewModel.IsFullScreen,
                IsOfflineMode = SelectedUser.StorageType ==
                                PCL.Neo.Core.Service.Accounts.Storage.UserTypeConstants.Offline,

                // 添加额外的JVM参数
                ExtraJvmArgs = string.IsNullOrEmpty(_gameSettingsViewModel.JvmArguments)
                    ? new List<string>()
                    : _gameSettingsViewModel.JvmArguments.Split(' ').ToList(),

                // 添加额外的游戏参数
                ExtraGameArgs = string.IsNullOrEmpty(_gameSettingsViewModel.GameArguments)
                    ? new List<string>()
                    : _gameSettingsViewModel.GameArguments.Split(' ').ToList(),

                // 环境变量
                EnvironmentVariables = new Dictionary<string, string>
                {
                    { "JAVA_TOOL_OPTIONS", "-Dfile.encoding=UTF-8" }
                },

                // 是否启动后关闭启动器
                CloseAfterLaunch = _gameSettingsViewModel.CloseAfterLaunch
            };

            // 跨平台支持的调整
            if (OperatingSystem.IsLinux())
            {
                // Linux平台添加额外的JVM参数
                launchOptions.ExtraJvmArgs.Add("-Djava.awt.headless=false");
                launchOptions.ExtraJvmArgs.Add("-Dorg.lwjgl.opengl.Display.allowSoftwareOpenGL=true");
            }
            else if (OperatingSystem.IsMacOS())
            {
                // macOS平台添加额外的JVM参数
                launchOptions.ExtraJvmArgs.Add("-XstartOnFirstThread");
                launchOptions.ExtraJvmArgs.Add("-Djava.awt.headless=false");
            }

            // 启动游戏
            Process process = await _gameLauncher.LaunchAsync(launchOptions);
            StatusMessage = "游戏已启动";

            // 如果设置了启动后关闭启动器
            if (launchOptions.CloseAfterLaunch)
            {
                Environment.Exit(0);
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"启动游戏失败: {ex.Message}";
        }
        finally
        {
            IsLaunching = false;
        }
    }

    [RelayCommand]
    private void RefreshVersionList()
    {
        LoadGameVersions();
    }

    [RelayCommand]
    private async Task ViewGameLogs()
    {
        // 导航到日志查看界面
        await _navigationService.GoToAsync<LogViewModel>();
    }

    [RelayCommand]
    private async Task ExportGameLogs()
    {
        try
        {
            var filePath = await _storageService.SaveFile("导出游戏日志", $"PCL.Neo游戏日志_{DateTime.Now:yyyyMMdd_HHmmss}", ".log");

            if (!string.IsNullOrEmpty(filePath))
            {
                _gameLauncher.ExportGameLogsAsync(filePath);
                StatusMessage = "日志导出成功";
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"导出日志失败: {ex.Message}";
        }
    }

    partial void OnCurrentLayoutChanged(HomeLayoutType value)
    {
        IsDefaultLayoutVisible = value == HomeLayoutType.Default;
        IsNewsLayoutVisible = value == HomeLayoutType.News;
        IsInfoLayoutVisible = value == HomeLayoutType.Info;
        IsSimpleLayoutVisible = value == HomeLayoutType.Simple;
    }
}

public class GameVersion
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string ReleaseTime { get; set; } = string.Empty;
}

// 新闻项的数据模型
public class GameNewsItem
{
    public string Title { get; set; } = string.Empty;
    public string Summary { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public string ImageUrl { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
}

// 信息项的数据模型
public class GameInfoItem
{
    public string Icon { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
}