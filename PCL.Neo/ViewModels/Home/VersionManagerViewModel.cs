using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PCL.Neo.Core.Models.Minecraft.Game;
using PCL.Neo.Core.Models.Minecraft.Game.Data;
using PCL.Neo.Services;
using PCL.Neo.ViewModels.Download;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace PCL.Neo.ViewModels.Home;

public class GameDirectory
{
    public string Path { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public bool IsDefault { get; set; }
    public bool IsScanned { get; set; }
    public DateTime LastScanTime { get; set; }
}

public class VersionItem
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Directory { get; set; } = string.Empty;
    public string IconPath { get; set; } = string.Empty;
    public VersionInfo VersionInfo { get; set; } = null!;
}

[SubViewModel(typeof(HomeViewModelBackup))]
public partial class VersionManagerViewModel : ViewModelBase
{
    private readonly INavigationService _navigationService;
    private readonly GameService _gameService;
    private readonly GameLauncher _gameLauncher;
    private readonly StorageService _storageService;

    [ObservableProperty]
    private ObservableCollection<GameDirectory> _directories = new();

    [ObservableProperty]
    private GameDirectory? _selectedDirectory;

    [ObservableProperty]
    private ObservableCollection<VersionItem> _versions = new();

    [ObservableProperty]
    private ObservableCollection<VersionItem> _filteredVersions = new();

    [ObservableProperty]
    private VersionItem? _selectedVersion;

    [ObservableProperty]
    private ObservableCollection<string> _versionFilters = new();

    [ObservableProperty]
    private string _selectedVersionFilter = "全部";

    [ObservableProperty]
    private string _searchText = string.Empty;

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private string _statusMessage = string.Empty;

    public VersionManagerViewModel(
        INavigationService navigationService,
        GameService gameService,
        StorageService storageService,
        GameLauncher gameLauncher)
    {
        _navigationService = navigationService;
        _gameService = gameService;
        _storageService = storageService;
        _gameLauncher = gameLauncher;

        // 初始化版本筛选器
        VersionFilters = new ObservableCollection<string>
        {
            "全部",
            "正式版",
            "快照版",
            "旧版",
            "Forge",
            "Fabric",
            "Quilt"
        };

        // 初始化游戏目录
        InitializeGameDirectories();
    }

    private void InitializeGameDirectories()
    {
        // 添加默认目录
        Directories.Add(new GameDirectory
        {
            Path = GameService.DefaultGameDirectory,
            DisplayName = "默认目录",
            IsDefault = true,
            IsScanned = false
        });

        // 可以从配置文件加载其他目录
        // ...

        if (Directories.Count > 0)
        {
            SelectedDirectory = Directories[0];
        }
    }

    public async Task Initialize()
    {
        // 加载默认目录的版本
        await LoadVersionsAsync(GameService.DefaultGameDirectory);
    }

    partial void OnSelectedDirectoryChanged(GameDirectory? value)
    {
        if (value != null)
        {
            _ = LoadVersionsAsync(value.Path);
        }
    }

    partial void OnSelectedVersionFilterChanged(string value)
    {
        ApplyFilters();
    }

    partial void OnSearchTextChanged(string value)
    {
        ApplyFilters();
    }

    private void ApplyFilters()
    {
        var filtered = Versions.AsEnumerable();

        // 应用版本类型筛选
        if (_selectedVersionFilter != "全部")
        {
            filtered = filtered.Where(v => v.Type.Contains(_selectedVersionFilter));
        }

        // 应用搜索文本
        if (!string.IsNullOrEmpty(_searchText))
        {
            var searchLower = _searchText.ToLower();
            filtered = filtered.Where(v =>
                v.Name.ToLower().Contains(searchLower) ||
                v.Id.ToLower().Contains(searchLower) ||
                v.Type.ToLower().Contains(searchLower));
        }

        FilteredVersions = new ObservableCollection<VersionItem>(filtered);
    }

    [RelayCommand]
    private async Task LoadVersionsAsync(string directory)
    {
        if (string.IsNullOrEmpty(directory))
            return;

        try
        {
            IsLoading = true;
            StatusMessage = "正在加载版本...";

            // 加载版本列表
            var versionInfos = await GameService.GetVersionsAsync(directory);

            // 转换为UI项
            var versionItems = versionInfos.Select(v => new VersionItem
            {
                Id = v.Id,
                Name = v.Name,
                Type = v.Type,
                Directory = directory,
                VersionInfo = v
            }).ToList();

            // 更新列表
            Versions = new ObservableCollection<VersionItem>(versionItems);

            // 应用筛选
            ApplyFilters();

            // 如果是选中的目录，更新扫描状态
            if (SelectedDirectory != null && SelectedDirectory.Path == directory)
            {
                SelectedDirectory.IsScanned = true;
                SelectedDirectory.LastScanTime = DateTime.Now;
            }

            StatusMessage = $"已加载 {versionItems.Count} 个版本";
        }
        catch (Exception ex)
        {
            StatusMessage = $"加载版本失败: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    public async Task ScanSelectedDirectoryCommand()
    {
        if (SelectedDirectory == null)
            return;

        await LoadVersionsAsync(SelectedDirectory.Path);
    }

    [RelayCommand]
    public async Task AddDirectoryCommand()
    {
        try
        {
            var path = await _storageService.SelectFolder("选择游戏目录");
            if (string.IsNullOrEmpty(path))
                return;

            // 检查目录是否已存在
            if (Directories.Any(d => d.Path == path))
            {
                StatusMessage = "该目录已添加";
                return;
            }

            // 添加到目录列表
            var displayName = GetDirectoryDisplayName(path);
            var newDirectory = new GameDirectory
            {
                Path = path,
                DisplayName = displayName,
                IsDefault = false,
                IsScanned = false
            };

            Directories.Add(newDirectory);

            // 选中并加载新添加的目录
            SelectedDirectory = newDirectory;

            StatusMessage = "已添加目录：" + displayName;
        }
        catch (Exception ex)
        {
            StatusMessage = $"添加目录失败: {ex.Message}";
        }
    }

    [RelayCommand]
    public void RemoveDirectoryCommand(GameDirectory directory)
    {
        if (directory == null || directory.IsDefault)
            return;

        Directories.Remove(directory);

        // 移除该目录下的版本
        var versionsToRemove = Versions.Where(v => v.Directory == directory.Path).ToList();
        foreach (var version in versionsToRemove)
        {
            Versions.Remove(version);
        }

        ApplyFilters();

        StatusMessage = "已移除目录：" + directory.DisplayName;
    }

    [RelayCommand]
    public void RefreshCommand()
    {
        if (SelectedDirectory != null)
        {
            _ = LoadVersionsAsync(SelectedDirectory.Path);
        }
    }

    [RelayCommand]
    public async Task DownloadVersionCommand()
    {
        // 导航到下载页面
        await _navigationService.GoToAsync<DownloadGameViewModel>();
    }

    [RelayCommand]
    public async Task SettingsCommand(VersionItem version)
    {
        if (version == null)
            return;

        // 导航到版本设置视图
        // TODO: 实现版本设置视图
        StatusMessage = $"准备设置版本 {version.Name}";
    }

    [RelayCommand]
    public async Task DeleteVersionCommand(VersionItem version)
    {
        if (version == null)
            return;

        try
        {
            IsLoading = true;
            StatusMessage = $"正在删除版本 {version.Name}...";

            // 删除版本
            await GameService.DeleteVersionAsync(version.Id, version.Directory);

            // 从列表中移除
            Versions.Remove(version);
            ApplyFilters();

            StatusMessage = $"已删除版本 {version.Name}";
        }
        catch (Exception ex)
        {
            StatusMessage = $"删除失败: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    public async Task LaunchVersionCommand(VersionItem version)
    {
        if (version == null)
            return;

        // 检查Java路径是否存在
        var javaPath = _gameService.DefaultJavaRuntimes.Java21.JavaWExe;
        if (string.IsNullOrEmpty(javaPath) || !File.Exists(javaPath))
        {
            StatusMessage = "无效的Java路径，请在设置中选择正确的Java可执行文件";
            return;
        }

        // 检查Java版本与Minecraft版本的兼容性
        if (!GameService.IsJavaCompatibleWithGame(_gameService.DefaultJavaRuntimes.Java21, version.Id))
        {
            // 提示用户但不阻止启动
            StatusMessage = "警告：当前Java版本可能与所选Minecraft版本不兼容";

            // 可以在这里添加对话框提示，让用户确认是否继续
            // 这里简化处理，直接等待3秒后继续
            await Task.Delay(3000);
        }

        try
        {
            IsLoading = true;
            StatusMessage = $"正在启动 {version.Name}...";

            // 创建完善的启动选项
            var launchOptions = new LaunchOptions
            {
                VersionId = version.Id,
                MinecraftRootDirectory = version.Directory,
                GameDirectory = version.Directory,
                JavaPath = _gameService.DefaultJavaRuntimes.Java21.JavaWExe,
                MaxMemoryMB = 2048, // 默认2GB内存
                MinMemoryMB = 512, // 最小内存设为512MB
                Username = "Player", // 默认用户
                UUID = Guid.NewGuid().ToString(),
                AccessToken = Guid.NewGuid().ToString(),
                WindowWidth = 854,
                WindowHeight = 480,
                FullScreen = false,
                IsOfflineMode = true,

                // 添加额外的JVM参数以优化性能
                ExtraJvmArgs = new List<string>
                {
                    "-XX:+UseG1GC",
                    "-XX:+ParallelRefProcEnabled",
                    "-XX:MaxGCPauseMillis=200",
                    "-XX:+UnlockExperimentalVMOptions",
                    "-XX:+DisableExplicitGC",
                    "-XX:+AlwaysPreTouch",
                    "-Dfile.encoding=UTF-8", // 确保使用UTF-8编码
                    "-Djava.net.preferIPv4Stack=true" // 优先使用IPv4
                },

                // 环境变量
                EnvironmentVariables = new Dictionary<string, string>
                {
                    { "JAVA_TOOL_OPTIONS", "-Dfile.encoding=UTF-8" }
                },

                // 是否启动后关闭启动器
                CloseAfterLaunch = false
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

            // 使用正式的GameLauncher启动游戏
            var process = await _gameLauncher.LaunchAsync(launchOptions);
            StatusMessage = $"{version.Name} 已启动";

            // 如果设置了启动后关闭启动器
            if (launchOptions.CloseAfterLaunch)
            {
                Environment.Exit(0);
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"启动失败: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }


    private string GetDirectoryDisplayName(string path)
    {
        // 从路径生成显示名称
        var displayName = Path.GetFileName(path);
        if (string.IsNullOrEmpty(displayName))
        {
            displayName = path;
        }

        return displayName;
    }
}
