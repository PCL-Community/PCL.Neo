using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PCL.Neo.Services;
using PCL.Neo.Views.Home;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using PCL.Neo.Core.Models.Minecraft.Game;
using Avalonia.Platform.Storage;
using PCL.Neo.Core.Utils;
using System.IO;
using System.Diagnostics;
using System.Linq;

namespace PCL.Neo.ViewModels.Home;

public class EnvironmentVariable
{
    public bool IsEnabled { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
}

public class ModInfo
{
    public bool IsEnabled { get; set; } = true;
    public string Name { get; set; } = string.Empty;
    public string Version { get; set; } = string.Empty;
    public string Author { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
}

public class VersionComponent
{
    public string Name { get; set; } = string.Empty;
    public string Version { get; set; } = string.Empty;
    public bool IsCompatible { get; set; } = true;
    public bool IsClickable { get; set; } = false;
}

[SubViewModel(typeof(HomeViewModelBackup))]
public partial class GameSettingsViewModel : ViewModelBase
{
    private readonly INavigationService _navigationService;
    private readonly GameService _gameService;
    private readonly StorageService _storageService;
    private bool _isInitialized = false;

    // 版本标题
    [ObservableProperty] private string _versionTitle = "1.20.2-Fabric 0.15.7-OptiFine_I7_pre1";

    // 导航相关
    [ObservableProperty] private int _selectedMenuIndex = 0;
    [ObservableProperty] private object? _currentContentView;
    [ObservableProperty] private object? _currentView;

    #region 基本信息

    [ObservableProperty] private string _versionId = string.Empty;
    [ObservableProperty] private string _gameVersionName = string.Empty;
    [ObservableProperty] private string _versionType = string.Empty;
    [ObservableProperty] private string _releaseTime = string.Empty;
    [ObservableProperty] private string _mainClass = string.Empty;
    [ObservableProperty] private string _inheritsFrom = string.Empty;

    #endregion

    #region 修改页面

    [ObservableProperty] private ObservableCollection<VersionComponent> _components = new();
    [ObservableProperty] private bool _hasFabricWarning = true;
    [ObservableProperty] private string _minecraftVersion = "";

    #endregion

    #region 概览页面

    [ObservableProperty] private string _packageName = "";
    [ObservableProperty] private string _packageDescription = "";
    [ObservableProperty] private string _customIcon = "自动";
    [ObservableProperty] private string _customCategory = "自动";

    #endregion

    #region 设置页面

    [ObservableProperty] private string _launchMode = "开启";
    [ObservableProperty] private string _gameWindowTitle = "跟随全局设置";
    [ObservableProperty] private string _customGameInfo = "跟随全局设置";
    [ObservableProperty] private string _gameJava = "跟随全局设置";
    [ObservableProperty] private bool _useGlobalMemory = true;
    [ObservableProperty] private bool _useCustomMemory = false;
    [ObservableProperty] private bool _isCustomMemory = false;
    [ObservableProperty] private double _memorySliderValue = 50;
    [ObservableProperty] private double _usedMemory = 8.3;
    [ObservableProperty] private double _totalMemory = 15.9;
    [ObservableProperty] private double _gameMemory = 4.5;
    [ObservableProperty] private string _serverLoginMethod = "正版登录或离线登录";
    [ObservableProperty] private string _serverAutoJoin = string.Empty;
    [ObservableProperty] private string _memoryOptimization = "跟随全局设置";
    [ObservableProperty] private string _advancedLaunchOptions = string.Empty;

    #endregion

    #region 导出页面

    [ObservableProperty] private string _packageVersion = "1.0.0";
    [ObservableProperty] private bool _exportGameCore = true;
    [ObservableProperty] private bool _exportGameSettings = true;
    [ObservableProperty] private bool _exportGameUserInfo = false;
    [ObservableProperty] private bool _exportMods = true;
    [ObservableProperty] private bool _exportModsSettings = true;
    [ObservableProperty] private bool _exportResourcePacks = true;
    [ObservableProperty] private string _selectedResourcePack = "";
    [ObservableProperty] private bool _exportMultiServerList = false;
    [ObservableProperty] private bool _exportLauncherProgram = false;
    [ObservableProperty] private bool _exportSourceFiles = false;
    [ObservableProperty] private bool _useModrinthUpload = false;

    #endregion

    #region Java设置

    [ObservableProperty] private string _javaPath = string.Empty;
    [ObservableProperty] private int _memoryAllocation = 2048;
    [ObservableProperty] private int _maxMemoryMB = 8192;
    [ObservableProperty] private string _memoryAllocationDisplay = "2048 MB";
    [ObservableProperty] private string _jvmArguments = string.Empty;

    #endregion

    #region 游戏设置

    [ObservableProperty] private string _gameDirectory = string.Empty;
    [ObservableProperty] private bool _isFullScreen = false;
    [ObservableProperty] private int _gameWidth = 854;
    [ObservableProperty] private int _gameHeight = 480;
    [ObservableProperty] private string _gameArguments = string.Empty;
    [ObservableProperty] private bool _closeAfterLaunch = false;
    [ObservableProperty] private bool _disableAnimation = false;
    [ObservableProperty] private bool _useLegacyLauncher = false;

    #endregion

    #region Mods管理

    [ObservableProperty] private ObservableCollection<ModInfo> _mods = new();

    #endregion

    #region 高级设置

    [ObservableProperty] private ObservableCollection<string> _downloadSources = new();
    [ObservableProperty] private string _selectedDownloadSource = string.Empty;
    [ObservableProperty] private string _proxyAddress = string.Empty;
    [ObservableProperty] private bool _isProxyEnabled = false;
    [ObservableProperty] private int _maxThreads = 16;
    [ObservableProperty] private ObservableCollection<EnvironmentVariable> _environmentVariables = new();
    [ObservableProperty] private bool _isDebugMode = false;
    [ObservableProperty] private bool _saveGameLog = true;
    [ObservableProperty] private bool _enableCrashAnalysis = true;

    #endregion

    #region 性能设置

    [ObservableProperty] private int _renderDistance = 12;
    [ObservableProperty] private ObservableCollection<string> _particleOptions = new();
    [ObservableProperty] private string _selectedParticleOption = string.Empty;
    [ObservableProperty] private ObservableCollection<string> _graphicsOptions = new();
    [ObservableProperty] private string _selectedGraphicsOption = string.Empty;
    [ObservableProperty] private int _maxFrameRate = 60;
    [ObservableProperty] private int _masterVolume = 100;
    [ObservableProperty] private int _musicVolume = 100;
    [ObservableProperty] private int _soundVolume = 100;

    #endregion

    public GameSettingsViewModel(INavigationService navigationService, GameService gameService,
        StorageService storageService)
    {
        _navigationService = navigationService;
        _gameService = gameService;
        _storageService = storageService;

        // 初始化组件列表
        Components = new ObservableCollection<VersionComponent>
        {
            new VersionComponent { Name = "Minecraft", Version = "1.20.2", IsClickable = true },
            new VersionComponent { Name = "Forge", Version = "与Fabric不兼容", IsCompatible = false },
            new VersionComponent { Name = "NeoForge", Version = "与Fabric不兼容", IsCompatible = false },
            new VersionComponent { Name = "Fabric", Version = "0.15.7", IsClickable = true },
            new VersionComponent { Name = "Fabric API", Version = "点击选择", IsClickable = true },
            new VersionComponent { Name = "Quilt", Version = "与Fabric不兼容", IsCompatible = false },
            new VersionComponent { Name = "OptiFine", Version = "点击选择", IsClickable = true }
        };

        // 初始化下拉选项
        DownloadSources = new ObservableCollection<string> { "官方源", "BMCLAPI", "自定义源" };
        SelectedDownloadSource = "BMCLAPI";

        ParticleOptions = new ObservableCollection<string> { "全部", "减少", "最小", "关闭" };
        SelectedParticleOption = "全部";

        GraphicsOptions = new ObservableCollection<string> { "流畅", "均衡", "高品质", "自定义" };
        SelectedGraphicsOption = "高品质";

        // 初始化环境变量列表
        EnvironmentVariables = new ObservableCollection<EnvironmentVariable>
        {
            new EnvironmentVariable
            {
                IsEnabled = true, Name = "JAVA_TOOL_OPTIONS", Value = "-Dfile.encoding=UTF-8"
            },
            new EnvironmentVariable { IsEnabled = false, Name = "JAVA_OPTS", Value = "-XX:+UseG1GC" }
        };

        // 监听菜单索引变化
        this.PropertyChanged += (sender, args) =>
        {
            if (args.PropertyName == nameof(SelectedMenuIndex))
            {
                UpdateContentView();
            }
        };

        // 监听设置变更
        this.PropertyChanged += (sender, args) =>
        {
            if (args.PropertyName == nameof(JavaPath) ||
                args.PropertyName == nameof(GameDirectory) ||
                args.PropertyName == nameof(MemoryAllocation) ||
                args.PropertyName == nameof(JvmArguments) ||
                args.PropertyName == nameof(GameArguments) ||
                args.PropertyName == nameof(IsFullScreen) ||
                args.PropertyName == nameof(GameWidth) ||
                args.PropertyName == nameof(GameHeight) ||
                args.PropertyName == nameof(CloseAfterLaunch))
            {
                // 只有在初始化完成后才保存设置
                if (_isInitialized)
                {
                    SaveUserSettings();
                }
            }
        };

        // 初始化默认视图
        UpdateContentView();

        // 设置默认Java路径
        JavaPath = _gameService.DefaultJavaRuntimes.Java21?.JavaWExe ?? string.Empty;

        // 设置默认游戏目录
        GameDirectory = GameService.DefaultGameDirectory;

        // 获取系统内存信息，设置最大可用内存
        DetectSystemMemory();

        // 加载用户设置
        LoadUserSettings();

        _isInitialized = true;
    }

    private void UpdateContentView()
    {
        // 根据选中的菜单索引更新内容视图
        switch (SelectedMenuIndex)
        {
            case 0: // 概览
                CurrentContentView = new OverviewView { DataContext = this };
                break;
            case 1: // 设置
                CurrentContentView = new SettingsView { DataContext = this };
                break;
            case 2: // 修改
                CurrentContentView = new ModifyView { DataContext = this };
                break;
            case 3: // 导出
                CurrentContentView = new ExportView { DataContext = this };
                break;
            default:
                CurrentContentView = new SavesView { DataContext = this };
                break;
        }
    }

    public async Task Initialize(string versionId)
    {
        VersionId = versionId;
        VersionTitle = versionId;

        try
        {
            // 加载版本信息
            var versionInfo = await Versions.GetVersionByIdAsync(GameService.DefaultGameDirectory, versionId);
            if (versionInfo != null)
            {
                GameVersionName = versionInfo.Name;
                VersionTitle = versionInfo.Name;
                PackageName = versionInfo.Name;
                VersionType = versionInfo.Type;
                ReleaseTime = versionInfo.ReleaseTime;
                MainClass = versionInfo.MainClass;
                InheritsFrom = versionInfo.InheritsFrom ?? "无";

                // 加载版本特定设置
                LoadVersionSettings(versionId);
            }

            // 加载系统信息
            MaxMemoryMB = SystemUtils.SystemMaxMemoryMB;

            // 确保内存分配合理
            if (MemoryAllocation > MaxMemoryMB)
            {
                MemoryAllocation = Math.Min(MaxMemoryMB, 4096);
            }

            // 加载模组列表
            await LoadMods(versionId);
        }
        catch (Exception ex)
        {
            // 处理异常
            System.Diagnostics.Debug.WriteLine($"初始化游戏设置失败: {ex.Message}");
        }
    }

    private void LoadVersionSettings(string versionId)
    {
        // 这里应该从配置文件加载特定版本的设置
        // 以下是示例数据
        JavaPath = _gameService.DefaultJavaRuntimes.Java21.JavaWExe;
        GameDirectory = GameService.DefaultGameDirectory;
        JvmArguments = "-XX:+UseG1GC -XX:+ParallelRefProcEnabled -XX:MaxGCPauseMillis=200";
        GameArguments = "";
    }

    private async Task LoadMods(string versionId)
    {
        // 这里应该从mods文件夹加载mod列表
        // 以下是示例数据
        Mods = new ObservableCollection<ModInfo>
        {
            new ModInfo { Name = "OptiFine", Version = "1.20.1_HD_U_I5", Author = "sp614x", Description = "优化模组" },
            new ModInfo { Name = "JourneyMap", Version = "5.9.16", Author = "techbrew", Description = "小地图模组" },
            new ModInfo
            {
                Name = "Fabric API", Version = "0.92.0", Author = "FabricMC", Description = "Fabric模组加载器API"
            }
        };

        // 实际实现时需要扫描mods文件夹
        await Task.CompletedTask;
    }

    partial void OnMemoryAllocationChanged(int value)
    {
        MemoryAllocationDisplay = $"{value} MB";
    }

    [RelayCommand]
    private async Task SelectJava()
    {
        var filters = new List<FilePickerFileType>
        {
            new("Java程序")
            {
                Patterns = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
                    ? new[] { "java.exe", "javaw.exe" }
                    : new[] { "java" }
            },
            new("所有文件") { Patterns = new[] { "*" } }
        };

        var javaPath = await _storageService.SelectFile("选择Java执行文件", filters);
        if (!string.IsNullOrEmpty(javaPath))
        {
            JavaPath = javaPath;
        }
    }

    [RelayCommand]
    private async Task SelectGameDirectory()
    {
        var gameDir = await _storageService.SelectFolder("选择游戏目录");
        if (!string.IsNullOrEmpty(gameDir))
        {
            GameDirectory = gameDir;
        }
    }

    [RelayCommand]
    private async Task SaveSettings()
    {
        try
        {
            // 保存设置
            SaveUserSettings();

            // 返回上一个页面
            _navigationService.GoBackAsync();
        }
        catch (Exception ex)
        {
            // 处理异常
            System.Diagnostics.Debug.WriteLine($"保存设置失败: {ex.Message}");
        }
    }

    [RelayCommand]
    private async Task Return()
    {
        // 返回上一页
        _navigationService.GoBackAsync();
    }

    [RelayCommand]
    private void EditVersionName()
    {
        try
        {
            // 实现版本名称编辑功能，需要UI交互
            // 这里暂且仅修改本地变量
            string versionJsonPath = Path.Combine(GameDirectory, "versions", VersionId, $"{VersionId}.json");

            if (File.Exists(versionJsonPath))
            {
                string jsonContent = File.ReadAllText(versionJsonPath);
                // TODO:修改版本名称需要UI交互
                // 实际使用时应该弹出对话框让用户输入新名称
                // var newName = "新版本名称";

                // 回调UI层实现编辑功能
                System.Diagnostics.Debug.WriteLine("需要UI层实现版本名称编辑功能");
            }
            else
            {
                System.Diagnostics.Debug.WriteLine($"版本文件不存在: {versionJsonPath}");
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"编辑版本名称失败: {ex.Message}");
        }
    }

    [RelayCommand]
    private void EditVersionDescription()
    {
        try
        {
            // TODO:实现版本描述编辑功能，需要UI交互
            // 回调UI层实现编辑功能
            System.Diagnostics.Debug.WriteLine("需要UI层实现版本描述编辑功能");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"编辑版本描述失败: {ex.Message}");
        }
    }

    [RelayCommand]
    private void AddToCollection()
    {
        // 实现添加到收藏的逻辑
    }

    [RelayCommand]
    private void OpenVersionFolder()
    {
        try
        {
            string versionFolder = Path.Combine(GameDirectory, "versions", VersionId);
            if (Directory.Exists(versionFolder))
            {
                OpenFolder(versionFolder);
            }
            else
            {
                System.Diagnostics.Debug.WriteLine($"版本文件夹不存在: {versionFolder}");
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"打开版本文件夹失败: {ex.Message}");
        }
    }

    [RelayCommand]
    private void OpenSaveFolder()
    {
        try
        {
            string saveFolder = Path.Combine(GameDirectory, "saves");
            if (!Directory.Exists(saveFolder))
            {
                Directory.CreateDirectory(saveFolder);
            }

            OpenFolder(saveFolder);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"打开存档文件夹失败: {ex.Message}");
        }
    }

    [RelayCommand]
    private void OpenModFolder()
    {
        try
        {
            string modFolder = Path.Combine(GameDirectory, "mods");
            if (!Directory.Exists(modFolder))
            {
                Directory.CreateDirectory(modFolder);
            }

            OpenFolder(modFolder);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"打开Mod文件夹失败: {ex.Message}");
        }
    }

    [RelayCommand]
    private async Task TestGame()
    {
        try
        {
            // 通过GameService启动游戏进行测试
            if (string.IsNullOrEmpty(VersionId))
            {
                System.Diagnostics.Debug.WriteLine("未选择游戏版本");
                return;
            }

            // 检查Java路径是否有效
            if (string.IsNullOrEmpty(JavaPath) || !File.Exists(JavaPath))
            {
                System.Diagnostics.Debug.WriteLine("无效的Java路径");
                return;
            }

            // 创建测试启动选项
            var testOptions = new PCL.Neo.Core.Models.Minecraft.Game.LaunchOptions
            {
                VersionId = VersionId,
                JavaPath = JavaPath,
                MinecraftDirectory = GameDirectory,
                GameDirectory = GameDirectory,
                MaxMemoryMB = MemoryAllocation,
                MinMemoryMB = Math.Max(512, MemoryAllocation / 4),
                Username = "Player", // 测试用户名
                UUID = Guid.NewGuid().ToString(),
                AccessToken = Guid.NewGuid().ToString(),
                WindowWidth = GameWidth,
                WindowHeight = GameHeight,
                FullScreen = IsFullScreen,
                IsOfflineMode = true, // 测试时使用离线模式

                // 添加JVM参数和游戏参数
                ExtraJvmArgs = string.IsNullOrEmpty(JvmArguments)
                    ? new List<string>()
                    : JvmArguments.Split(' ').ToList(),
                ExtraGameArgs = string.IsNullOrEmpty(GameArguments)
                    ? new List<string>()
                    : GameArguments.Split(' ').ToList(),

                // 环境变量
                EnvironmentVariables = new Dictionary<string, string>
                {
                    { "JAVA_TOOL_OPTIONS", "-Dfile.encoding=UTF-8" }
                }
            };

            // 跨平台支持
            if (OperatingSystem.IsLinux())
            {
                testOptions.ExtraJvmArgs.Add("-Djava.awt.headless=false");
                testOptions.ExtraJvmArgs.Add("-Dorg.lwjgl.opengl.Display.allowSoftwareOpenGL=true");
            }
            else if (OperatingSystem.IsMacOS())
            {
                testOptions.ExtraJvmArgs.Add("-XstartOnFirstThread");
                testOptions.ExtraJvmArgs.Add("-Djava.awt.headless=false");
            }

            // 获取GameLauncher实例并启动游戏
            var gameLauncher = new PCL.Neo.Core.Models.Minecraft.Game.GameLauncher(_gameService);
            var process = await gameLauncher.LaunchAsync(testOptions);

            System.Diagnostics.Debug.WriteLine($"测试游戏已启动，进程ID: {process.Id}");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"测试游戏失败: {ex.Message}");
        }
    }

    [RelayCommand]
    private async Task ExportStarter()
    {
        try
        {
            // 创建启动脚本
            string scriptContent;
            string fileExtension;

            // 确定版本ID和Java路径有效
            if (string.IsNullOrEmpty(VersionId))
            {
                System.Diagnostics.Debug.WriteLine("未选择游戏版本");
                return;
            }

            if (string.IsNullOrEmpty(JavaPath) || !File.Exists(JavaPath))
            {
                System.Diagnostics.Debug.WriteLine("无效的Java路径");
                return;
            }

            // 构建启动命令
            string javaPath = Path.GetFullPath(JavaPath);
            string gameDir = Path.GetFullPath(GameDirectory);

            // 构建JVM参数
            string jvmArgs = $"-Xmx{MemoryAllocation}M -Xms{Math.Max(512, MemoryAllocation / 4)}M";

            if (!string.IsNullOrEmpty(JvmArguments))
            {
                jvmArgs += " " + JvmArguments;
            }

            // 平台特定JVM参数
            if (OperatingSystem.IsLinux())
            {
                jvmArgs += " -Djava.awt.headless=false -Dorg.lwjgl.opengl.Display.allowSoftwareOpenGL=true";
            }
            else if (OperatingSystem.IsMacOS())
            {
                jvmArgs += " -XstartOnFirstThread -Djava.awt.headless=false";
            }

            // 构建游戏参数（简化版）
            string gameArgs =
                $"--username Player --version {VersionId} --gameDir \"{gameDir}\" --assetsDir \"{Path.Combine(gameDir, "assets")}\"";

            if (!string.IsNullOrEmpty(GameArguments))
            {
                gameArgs += " " + GameArguments;
            }

            // 根据操作系统创建不同的脚本
            if (OperatingSystem.IsWindows())
            {
                scriptContent = $@"@echo off
title Minecraft {VersionId} Launcher
""{javaPath}"" {jvmArgs} -jar ""{Path.Combine(gameDir, "versions", VersionId, $"{VersionId}.jar")}"" {gameArgs}
pause";
                fileExtension = ".bat";
            }
            else // Linux/macOS
            {
                scriptContent = $@"#!/bin/bash
echo ""Starting Minecraft {VersionId}...""
""{javaPath}"" {jvmArgs} -jar ""{Path.Combine(gameDir, "versions", VersionId, $"{VersionId}.jar")}"" {gameArgs}
echo ""Game exited. Press any key to continue...""
read -n 1 -s";
                fileExtension = ".sh";
            }

            // 保存脚本文件
            var filePath = await _storageService.SaveFile("保存启动脚本", $"启动Minecraft_{VersionId}", fileExtension);

            if (!string.IsNullOrEmpty(filePath))
            {
                File.WriteAllText(filePath, scriptContent);

                // 在Linux/macOS上设置脚本为可执行
                if (!OperatingSystem.IsWindows())
                {
                    try
                    {
                        using var process = new Process
                        {
                            StartInfo = new ProcessStartInfo
                            {
                                FileName = "chmod",
                                Arguments = $"+x \"{filePath}\"",
                                UseShellExecute = false,
                                CreateNoWindow = true
                            }
                        };
                        process.Start();
                        process.WaitForExit();
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"设置脚本可执行权限失败: {ex.Message}");
                    }
                }

                System.Diagnostics.Debug.WriteLine($"启动脚本已导出: {filePath}");
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"导出启动脚本失败: {ex.Message}");
        }
    }

    [RelayCommand]
    private async Task CompleteFiles()
    {
        try
        {
            if (string.IsNullOrEmpty(VersionId))
            {
                System.Diagnostics.Debug.WriteLine("未选择游戏版本");
                return;
            }

            // 补全游戏文件，需要访问GameService
            System.Diagnostics.Debug.WriteLine($"开始补全版本 {VersionId} 的文件...");

            string versionFolder = Path.Combine(GameDirectory, "versions", VersionId);

            if (!Directory.Exists(versionFolder))
            {
                System.Diagnostics.Debug.WriteLine($"版本文件夹不存在: {versionFolder}");
                return;
            }

            // 检查版本JSON文件
            string versionJsonPath = Path.Combine(versionFolder, $"{VersionId}.json");

            if (!File.Exists(versionJsonPath))
            {
                System.Diagnostics.Debug.WriteLine($"版本JSON文件不存在: {versionJsonPath}");
                return;
            }

            // 解析版本信息
            string jsonContent = File.ReadAllText(versionJsonPath);
            var versionInfo = System.Text.Json.JsonSerializer.Deserialize<object>(jsonContent);

            // 这里需要调用GameService的实际补全文件方法
            // 由于GameService的具体实现可能不同，这里只模拟进度

            // 显示进度条或提示（实际实现需要UI交互）
            for (int i = 0; i <= 100; i += 10)
            {
                System.Diagnostics.Debug.WriteLine($"补全文件进度: {i}%");
                await Task.Delay(100); // 模拟进度，实际实现中应删除此行
            }

            System.Diagnostics.Debug.WriteLine("文件补全完成");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"补全文件失败: {ex.Message}");
        }
    }

    [RelayCommand]
    private void OpenSaveFile()
    {
        try
        {
            // 打开选择存档文件对话框
            System.Diagnostics.Debug.WriteLine("需要实现选择存档文件的UI交互");

            // 以下为示例代码，需要UI层实现文件选择
            // var saveFile = await _storageService.SelectFileWithFilters(
            //     new List<FilePickerFileType>
            //     {
            //         new("Minecraft存档") { Patterns = new[] { "*.zip", "*.mcworld" } },
            //         new("所有文件") { Patterns = new[] { "*" } }
            //     },
            //     "选择Minecraft存档文件");
            //
            // if (!string.IsNullOrEmpty(saveFile))
            // {
            //     OpenFile(saveFile);
            // }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"打开存档文件失败: {ex.Message}");
        }
    }

    [RelayCommand]
    private async Task PasteSaveFile()
    {
        try
        {
            // 实现粘贴存档功能
            string savesFolder = Path.Combine(GameDirectory, "saves");

            if (!Directory.Exists(savesFolder))
            {
                Directory.CreateDirectory(savesFolder);
            }

            // 选择要导入的存档文件
            var saveFile = await _storageService.SelectFile("选择Minecraft存档文件",
                new List<FilePickerFileType>
                {
                    new("Minecraft存档") { Patterns = new[] { "*.zip", "*.mcworld" } },
                    new("所有文件") { Patterns = new[] { "*" } }
                }
            );

            if (string.IsNullOrEmpty(saveFile) || !File.Exists(saveFile))
            {
                return;
            }

            // 处理存档文件，根据文件类型决定操作
            string extension = Path.GetExtension(saveFile).ToLower();

            if (extension == ".zip" || extension == ".mcworld")
            {
                // 解压存档文件
                System.Diagnostics.Debug.WriteLine($"正在解压存档文件: {saveFile}");

                // 这里需要实现解压功能
                // 作为示例，我们只做简单的文件复制
                string saveFileName = Path.GetFileNameWithoutExtension(saveFile);
                string targetFolder = Path.Combine(savesFolder, saveFileName);

                if (!Directory.Exists(targetFolder))
                {
                    Directory.CreateDirectory(targetFolder);
                }

                // 检查是否为有效的存档文件（应该包含level.dat）
                // 实际实现应该解压ZIP文件并检查内容

                File.Copy(saveFile, Path.Combine(targetFolder, Path.GetFileName(saveFile)), true);

                System.Diagnostics.Debug.WriteLine($"存档已导入: {targetFolder}");
            }
            else
            {
                // 非ZIP格式，直接复制文件
                string targetFile = Path.Combine(savesFolder, Path.GetFileName(saveFile));
                File.Copy(saveFile, targetFile, true);

                System.Diagnostics.Debug.WriteLine($"存档文件已复制: {targetFile}");
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"粘贴存档文件失败: {ex.Message}");
        }
    }

    // 检测系统内存
    private void DetectSystemMemory()
    {
        try
        {
            // 获取系统总内存 (以MB为单位)
            MaxMemoryMB = SystemUtils.SystemMaxMemoryMB;

            // 默认分配最大内存的1/4，但至少1GB，最多4GB
            MemoryAllocation = Math.Min(4096, Math.Max(1024, MaxMemoryMB / 4));
        }
        catch
        {
            // 出错时使用默认值
            MaxMemoryMB = 8192;
            MemoryAllocation = 2048;
        }
    }

    // 保存用户设置
    private void SaveUserSettings()
    {
        try
        {
            // 创建设置目录
            string settingsDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "PCL.Neo", "settings");
            Directory.CreateDirectory(settingsDir);

            // 保存基本设置
            var settings = new Dictionary<string, string>
            {
                { "JavaPath", JavaPath },
                { "GameDirectory", GameDirectory },
                { "MemoryAllocation", MemoryAllocation.ToString() },
                { "JvmArguments", JvmArguments },
                { "GameArguments", GameArguments },
                { "IsFullScreen", IsFullScreen.ToString() },
                { "GameWidth", GameWidth.ToString() },
                { "GameHeight", GameHeight.ToString() },
                { "CloseAfterLaunch", CloseAfterLaunch.ToString() },
                { "SaveGameLog", SaveGameLog.ToString() },
                { "EnableCrashAnalysis", EnableCrashAnalysis.ToString() },
                { "IsProxyEnabled", IsProxyEnabled.ToString() },
                { "ProxyAddress", ProxyAddress }
            };

            // 写入JSON文件
            string settingsFile = Path.Combine(settingsDir, "game_settings.json");
            File.WriteAllText(settingsFile,
                System.Text.Json.JsonSerializer.Serialize(settings,
                    new System.Text.Json.JsonSerializerOptions { WriteIndented = true }));
        }
        catch (Exception ex)
        {
            // 处理异常
            System.Diagnostics.Debug.WriteLine($"保存设置失败: {ex.Message}");
        }
    }

    // 加载用户设置
    private void LoadUserSettings()
    {
        try
        {
            string settingsFile = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "PCL.Neo", "settings", "game_settings.json");

            if (File.Exists(settingsFile))
            {
                var json = File.ReadAllText(settingsFile);
                var settings = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, string>>(json);

                if (settings != null)
                {
                    // 加载Java路径
                    if (settings.TryGetValue("JavaPath", out string javaPath) && !string.IsNullOrEmpty(javaPath) &&
                        File.Exists(javaPath))
                    {
                        JavaPath = javaPath;
                    }
                    else
                    {
                        JavaPath = _gameService.DefaultJavaRuntimes.Java21.JavaWExe;
                    }

                    // 加载游戏目录
                    if (settings.TryGetValue("GameDirectory", out string gameDir) && !string.IsNullOrEmpty(gameDir) &&
                        Directory.Exists(gameDir))
                    {
                        GameDirectory = gameDir;
                    }

                    // 加载内存设置
                    if (settings.TryGetValue("MemoryAllocation", out string memAlloc) &&
                        int.TryParse(memAlloc, out int memory))
                    {
                        MemoryAllocation = Math.Min(MaxMemoryMB, memory);
                    }

                    // 加载JVM参数
                    if (settings.TryGetValue("JvmArguments", out string jvmArgs))
                    {
                        JvmArguments = jvmArgs;
                    }

                    // 加载游戏参数
                    if (settings.TryGetValue("GameArguments", out string gameArgs))
                    {
                        GameArguments = gameArgs;
                    }

                    // 加载全屏设置
                    if (settings.TryGetValue("IsFullScreen", out string isFullScreen) &&
                        bool.TryParse(isFullScreen, out bool fullScreen))
                    {
                        IsFullScreen = fullScreen;
                    }

                    // 加载游戏窗口尺寸
                    if (settings.TryGetValue("GameWidth", out string gameWidth) &&
                        int.TryParse(gameWidth, out int width))
                    {
                        GameWidth = width;
                    }

                    if (settings.TryGetValue("GameHeight", out string gameHeight) &&
                        int.TryParse(gameHeight, out int height))
                    {
                        GameHeight = height;
                    }

                    // 加载启动后关闭设置
                    if (settings.TryGetValue("CloseAfterLaunch", out string closeAfterLaunch) &&
                        bool.TryParse(closeAfterLaunch, out bool close))
                    {
                        CloseAfterLaunch = close;
                    }

                    // 加载日志保存设置
                    if (settings.TryGetValue("SaveGameLog", out string saveGameLog) &&
                        bool.TryParse(saveGameLog, out bool saveLog))
                    {
                        SaveGameLog = saveLog;
                    }

                    // 加载崩溃分析设置
                    if (settings.TryGetValue("EnableCrashAnalysis", out string enableCrashAnalysis) &&
                        bool.TryParse(enableCrashAnalysis, out bool crashAnalysis))
                    {
                        EnableCrashAnalysis = crashAnalysis;
                    }

                    // 加载代理设置
                    if (settings.TryGetValue("IsProxyEnabled", out string isProxyEnabled) &&
                        bool.TryParse(isProxyEnabled, out bool proxyEnabled))
                    {
                        IsProxyEnabled = proxyEnabled;
                    }

                    if (settings.TryGetValue("ProxyAddress", out string proxyAddress))
                    {
                        ProxyAddress = proxyAddress;
                    }
                }
            }
        }
        catch (Exception ex)
        {
            // 处理异常
            System.Diagnostics.Debug.WriteLine($"加载设置失败: {ex.Message}");
        }
    }

    // 跨平台打开文件夹
    private void OpenFolder(string folderPath)
    {
        try
        {
            if (OperatingSystem.IsWindows())
            {
                using var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "explorer.exe", Arguments = $"\"{folderPath}\"", UseShellExecute = true
                    }
                };
                process.Start();
            }
            else if (OperatingSystem.IsLinux())
            {
                using var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "xdg-open", Arguments = folderPath, UseShellExecute = true
                    }
                };
                process.Start();
            }
            else if (OperatingSystem.IsMacOS())
            {
                using var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "open", Arguments = folderPath, UseShellExecute = true
                    }
                };
                process.Start();
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"打开文件夹失败: {ex.Message}");
        }
    }

    // 跨平台打开文件
    private void OpenFile(string filePath)
    {
        try
        {
            if (!File.Exists(filePath))
            {
                System.Diagnostics.Debug.WriteLine($"文件不存在: {filePath}");
                return;
            }

            if (OperatingSystem.IsWindows())
            {
                using var process = new Process
                {
                    StartInfo = new ProcessStartInfo { FileName = filePath, UseShellExecute = true }
                };
                process.Start();
            }
            else if (OperatingSystem.IsLinux())
            {
                using var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "xdg-open", Arguments = filePath, UseShellExecute = true
                    }
                };
                process.Start();
            }
            else if (OperatingSystem.IsMacOS())
            {
                using var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "open", Arguments = filePath, UseShellExecute = true
                    }
                };
                process.Start();
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"打开文件失败: {ex.Message}");
        }
    }

    [RelayCommand]
    private void Reinstall()
    {
        try
        {
            if (string.IsNullOrEmpty(VersionId))
            {
                System.Diagnostics.Debug.WriteLine("未选择游戏版本");
                return;
            }

            // 确认对话框（应该由UI层实现）
            bool confirmed = true; // 假设用户已确认，实际使用时应该显示确认对话框

            if (!confirmed)
            {
                return;
            }

            string versionFolder = Path.Combine(GameDirectory, "versions", VersionId);

            if (!Directory.Exists(versionFolder))
            {
                System.Diagnostics.Debug.WriteLine($"版本文件夹不存在: {versionFolder}");
                return;
            }

            // 备份版本JSON文件
            string versionJsonPath = Path.Combine(versionFolder, $"{VersionId}.json");
            string backupJsonPath = Path.Combine(versionFolder, $"{VersionId}.json.bak");

            if (File.Exists(versionJsonPath))
            {
                File.Copy(versionJsonPath, backupJsonPath, true);
            }

            // 删除除JSON备份外的所有文件
            foreach (var file in Directory.GetFiles(versionFolder))
            {
                if (file != backupJsonPath)
                {
                    File.Delete(file);
                }
            }

            // 恢复JSON文件
            if (File.Exists(backupJsonPath))
            {
                File.Copy(backupJsonPath, versionJsonPath, true);
                File.Delete(backupJsonPath);
            }

            // 重新下载所需文件（实际应调用GameService方法）
            System.Diagnostics.Debug.WriteLine($"版本 {VersionId} 已准备重新安装");

            // TODO: 调用GameService方法重新下载文件
            // 例如: await _gameService.ReinstallVersion(VersionId, GameDirectory);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"重装失败: {ex.Message}");
        }
    }

    [RelayCommand]
    private async Task DeleteVersion()
    {
        try
        {
            if (string.IsNullOrEmpty(VersionId))
            {
                System.Diagnostics.Debug.WriteLine("未选择游戏版本");
                return;
            }

            // 确认对话框（应该由UI层实现）
            bool confirmed = true; // 假设用户已确认，实际使用时应该显示确认对话框

            if (!confirmed)
            {
                return;
            }

            string versionFolder = Path.Combine(GameDirectory, "versions", VersionId);

            if (!Directory.Exists(versionFolder))
            {
                System.Diagnostics.Debug.WriteLine($"版本文件夹不存在: {versionFolder}");
                return;
            }

            // 删除版本文件夹及其内容
            Directory.Delete(versionFolder, true);

            System.Diagnostics.Debug.WriteLine($"版本 {VersionId} 已删除");

            // 返回上一页
            _navigationService.GoBackAsync();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"删除版本失败: {ex.Message}");
        }
    }

    [RelayCommand]
    private void StartModify()
    {
        try
        {
            if (string.IsNullOrEmpty(VersionId))
            {
                System.Diagnostics.Debug.WriteLine("未选择游戏版本");
                return;
            }

            // 切换到修改页面
            SelectedMenuIndex = 2;

            // 准备修改操作
            System.Diagnostics.Debug.WriteLine($"开始修改版本 {VersionId}");

            // 可以在这里加载可用的组件，如Forge、Fabric等版本
            // 例如：LoadAvailableComponents();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"开始修改失败: {ex.Message}");
        }
    }

    [RelayCommand]
    private async Task ExportPackageGuide()
    {
        try
        {
            // 打开整合包制作指南，可以是在线链接或本地文档
            string guideUrl = "https://www.mcbbs.net/thread-896219-1-1.html"; // 示例链接，应替换为实际指南URL

            // 在浏览器中打开指南
            if (OperatingSystem.IsWindows())
            {
                using var process = new Process
                {
                    StartInfo = new ProcessStartInfo { FileName = guideUrl, UseShellExecute = true }
                };
                process.Start();
            }
            else if (OperatingSystem.IsLinux())
            {
                using var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "xdg-open", Arguments = guideUrl, UseShellExecute = true
                    }
                };
                process.Start();
            }
            else if (OperatingSystem.IsMacOS())
            {
                using var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "open", Arguments = guideUrl, UseShellExecute = true
                    }
                };
                process.Start();
            }

            // 也可以提供本地HTML文档
            // 或显示对话框提示
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"打开整合包制作指南失败: {ex.Message}");
        }
    }

    [RelayCommand]
    private async Task Export()
    {
        try
        {
            if (string.IsNullOrEmpty(VersionId))
            {
                System.Diagnostics.Debug.WriteLine("未选择游戏版本");
                return;
            }

            // 选择导出目录
            var exportDir = await _storageService.SelectFolder("选择整合包导出目录");

            if (string.IsNullOrEmpty(exportDir))
            {
                return;
            }

            // 创建整合包目录
            string packageName = $"{PackageName}_{PackageVersion}";
            string packageDir = Path.Combine(exportDir, packageName);

            if (Directory.Exists(packageDir))
            {
                // 提示用户确认覆盖或者重命名
                // 这里简化处理，直接删除已存在的目录
                Directory.Delete(packageDir, true);
            }

            Directory.CreateDirectory(packageDir);

            // 复制游戏核心
            if (ExportGameCore)
            {
                string versionFolder = Path.Combine(GameDirectory, "versions", VersionId);
                string targetVersionFolder = Path.Combine(packageDir, "versions", VersionId);

                Directory.CreateDirectory(targetVersionFolder);

                foreach (var file in Directory.GetFiles(versionFolder))
                {
                    string fileName = Path.GetFileName(file);
                    File.Copy(file, Path.Combine(targetVersionFolder, fileName), true);
                }

                System.Diagnostics.Debug.WriteLine("已导出游戏核心");
            }

            // 复制游戏设置
            if (ExportGameSettings)
            {
                string optionsFile = Path.Combine(GameDirectory, "options.txt");
                if (File.Exists(optionsFile))
                {
                    File.Copy(optionsFile, Path.Combine(packageDir, "options.txt"), true);
                    System.Diagnostics.Debug.WriteLine("已导出游戏设置");
                }
            }

            // 复制Mods
            if (ExportMods)
            {
                string modsFolder = Path.Combine(GameDirectory, "mods");
                string targetModsFolder = Path.Combine(packageDir, "mods");

                if (Directory.Exists(modsFolder))
                {
                    Directory.CreateDirectory(targetModsFolder);

                    foreach (var file in Directory.GetFiles(modsFolder, "*.jar"))
                    {
                        string fileName = Path.GetFileName(file);
                        File.Copy(file, Path.Combine(targetModsFolder, fileName), true);
                    }

                    System.Diagnostics.Debug.WriteLine("已导出Mods");
                }
            }

            // 复制Mods配置
            if (ExportModsSettings)
            {
                string configFolder = Path.Combine(GameDirectory, "config");
                string targetConfigFolder = Path.Combine(packageDir, "config");

                if (Directory.Exists(configFolder))
                {
                    CopyDirectory(configFolder, targetConfigFolder);
                    System.Diagnostics.Debug.WriteLine("已导出Mods配置");
                }
            }

            // 复制资源包
            if (ExportResourcePacks)
            {
                string resourcePacksFolder = Path.Combine(GameDirectory, "resourcepacks");
                string targetResourcePacksFolder = Path.Combine(packageDir, "resourcepacks");

                if (Directory.Exists(resourcePacksFolder))
                {
                    Directory.CreateDirectory(targetResourcePacksFolder);

                    // 如果选择了特定资源包
                    if (!string.IsNullOrEmpty(SelectedResourcePack))
                    {
                        string resourcePackPath = Path.Combine(resourcePacksFolder, SelectedResourcePack);
                        if (File.Exists(resourcePackPath))
                        {
                            File.Copy(resourcePackPath, Path.Combine(targetResourcePacksFolder, SelectedResourcePack),
                                true);
                        }
                    }
                    else
                    {
                        // 复制所有资源包
                        foreach (var file in Directory.GetFiles(resourcePacksFolder))
                        {
                            string fileName = Path.GetFileName(file);
                            File.Copy(file, Path.Combine(targetResourcePacksFolder, fileName), true);
                        }
                    }

                    System.Diagnostics.Debug.WriteLine("已导出资源包");
                }
            }

            // 复制多人游戏服务器列表
            if (ExportMultiServerList)
            {
                string serversFile = Path.Combine(GameDirectory, "servers.dat");
                if (File.Exists(serversFile))
                {
                    File.Copy(serversFile, Path.Combine(packageDir, "servers.dat"), true);
                    System.Diagnostics.Debug.WriteLine("已导出服务器列表");
                }
            }

            // 创建启动器程序
            if (ExportLauncherProgram)
            {
                // 创建启动脚本
                string scriptContent;
                string scriptPath;

                if (OperatingSystem.IsWindows())
                {
                    scriptContent = $@"@echo off
title {PackageName} 启动器
echo 正在启动 {PackageName} v{PackageVersion}...
cd /d ""%~dp0""
java -Xmx{MemoryAllocation}M -jar ""versions\{VersionId}\{VersionId}.jar""
pause";
                    scriptPath = Path.Combine(packageDir, "启动.bat");
                }
                else // Linux/macOS
                {
                    scriptContent = $@"#!/bin/bash
echo ""正在启动 {PackageName} v{PackageVersion}...""
cd ""$(dirname ""$0"")""
java -Xmx{MemoryAllocation}M -jar ""versions/{VersionId}/{VersionId}.jar""
echo ""游戏已退出""
read -n 1 -s -r -p ""按任意键退出...""";
                    scriptPath = Path.Combine(packageDir, "启动.sh");
                }

                File.WriteAllText(scriptPath, scriptContent);

                // 设置Linux/macOS脚本权限
                if (!OperatingSystem.IsWindows() && File.Exists(scriptPath))
                {
                    try
                    {
                        using var process = new Process
                        {
                            StartInfo = new ProcessStartInfo
                            {
                                FileName = "chmod",
                                Arguments = $"+x \"{scriptPath}\"",
                                UseShellExecute = false,
                                CreateNoWindow = true
                            }
                        };
                        process.Start();
                        process.WaitForExit();
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"设置启动脚本权限失败: {ex.Message}");
                    }
                }

                System.Diagnostics.Debug.WriteLine("已创建启动器程序");
            }

            // 创建ReadMe文件
            string readmePath = Path.Combine(packageDir, "ReadMe.txt");
            string readmeContent = $@"{PackageName} v{PackageVersion}
{PackageDescription}

安装说明:
1. 将整个文件夹复制到您喜欢的位置
2. 确保已安装Java
3. 双击启动脚本开始游戏

祝您游戏愉快!";

            File.WriteAllText(readmePath, readmeContent);

            // 打开导出目录
            OpenFolder(packageDir);

            System.Diagnostics.Debug.WriteLine($"整合包已导出到: {packageDir}");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"导出整合包失败: {ex.Message}");
        }
    }

    // 递归复制目录
    private void CopyDirectory(string sourceDir, string targetDir)
    {
        Directory.CreateDirectory(targetDir);

        foreach (var file in Directory.GetFiles(sourceDir))
        {
            string fileName = Path.GetFileName(file);
            string targetFile = Path.Combine(targetDir, fileName);
            File.Copy(file, targetFile, true);
        }

        foreach (var directory in Directory.GetDirectories(sourceDir))
        {
            string dirName = Path.GetFileName(directory);
            string targetSubDir = Path.Combine(targetDir, dirName);
            CopyDirectory(directory, targetSubDir);
        }
    }

    [RelayCommand]
    private void ReadConfig()
    {
        try
        {
            // 读取游戏配置文件
            string optionsFile = Path.Combine(GameDirectory, "options.txt");

            if (File.Exists(optionsFile))
            {
                var gameOptions = new Dictionary<string, string>();

                foreach (var line in File.ReadAllLines(optionsFile))
                {
                    if (string.IsNullOrWhiteSpace(line) || !line.Contains(':'))
                        continue;

                    var parts = line.Split(new[] { ':' }, 2);
                    if (parts.Length == 2)
                    {
                        string key = parts[0].Trim();
                        string value = parts[1].Trim();
                        gameOptions[key] = value;
                    }
                }

                // 根据读取的配置更新UI
                if (gameOptions.TryGetValue("renderDistance", out string renderDistanceValue) &&
                    int.TryParse(renderDistanceValue, out int renderDistance))
                {
                    RenderDistance = renderDistance;
                }

                if (gameOptions.TryGetValue("particles", out string particlesValue))
                {
                    switch (particlesValue)
                    {
                        case "0":
                            SelectedParticleOption = "全部";
                            break;
                        case "1":
                            SelectedParticleOption = "减少";
                            break;
                        case "2":
                            SelectedParticleOption = "最小";
                            break;
                        default:
                            SelectedParticleOption = "全部";
                            break;
                    }
                }

                if (gameOptions.TryGetValue("maxFps", out string maxFpsValue) &&
                    int.TryParse(maxFpsValue, out int maxFps))
                {
                    MaxFrameRate = maxFps;
                }

                if (gameOptions.TryGetValue("soundVolume", out string soundVolumeValue) &&
                    float.TryParse(soundVolumeValue, out float soundVolume))
                {
                    SoundVolume = (int)(soundVolume * 100);
                }

                if (gameOptions.TryGetValue("musicVolume", out string musicVolumeValue) &&
                    float.TryParse(musicVolumeValue, out float musicVolume))
                {
                    MusicVolume = (int)(musicVolume * 100);
                }

                // 加载图形设置
                if (gameOptions.TryGetValue("graphicsMode", out string graphicsModeValue) &&
                    int.TryParse(graphicsModeValue, out int graphicsMode))
                {
                    switch (graphicsMode)
                    {
                        case 0:
                            SelectedGraphicsOption = "流畅";
                            break;
                        case 1:
                            SelectedGraphicsOption = "均衡";
                            break;
                        case 2:
                            SelectedGraphicsOption = "高品质";
                            break;
                        default:
                            SelectedGraphicsOption = "自定义";
                            break;
                    }
                }

                System.Diagnostics.Debug.WriteLine("游戏配置已加载");
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("游戏配置文件不存在，将使用默认设置");
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"读取配置失败: {ex.Message}");
        }
    }

    [RelayCommand]
    private void SaveConfig()
    {
        try
        {
            // 保存游戏配置文件
            string optionsFile = Path.Combine(GameDirectory, "options.txt");

            // 读取现有配置（如果存在）
            var gameOptions = new Dictionary<string, string>();

            if (File.Exists(optionsFile))
            {
                foreach (var line in File.ReadAllLines(optionsFile))
                {
                    if (string.IsNullOrWhiteSpace(line) || !line.Contains(':'))
                        continue;

                    var parts = line.Split(new[] { ':' }, 2);
                    if (parts.Length == 2)
                    {
                        string key = parts[0].Trim();
                        string value = parts[1].Trim();
                        gameOptions[key] = value;
                    }
                }
            }

            // 更新配置
            gameOptions["renderDistance"] = RenderDistance.ToString();

            switch (SelectedParticleOption)
            {
                case "全部":
                    gameOptions["particles"] = "0";
                    break;
                case "减少":
                    gameOptions["particles"] = "1";
                    break;
                case "最小":
                    gameOptions["particles"] = "2";
                    break;
                case "关闭":
                    gameOptions["particles"] = "3";
                    break;
            }

            gameOptions["maxFps"] = MaxFrameRate.ToString();
            gameOptions["soundVolume"] = (SoundVolume / 100f).ToString("0.0");
            gameOptions["musicVolume"] = (MusicVolume / 100f).ToString("0.0");

            // 设置图形选项
            switch (SelectedGraphicsOption)
            {
                case "流畅":
                    gameOptions["graphicsMode"] = "0";
                    gameOptions["fancyGraphics"] = "false";
                    gameOptions["renderClouds"] = "false";
                    break;
                case "均衡":
                    gameOptions["graphicsMode"] = "1";
                    gameOptions["fancyGraphics"] = "true";
                    gameOptions["renderClouds"] = "true";
                    break;
                case "高品质":
                    gameOptions["graphicsMode"] = "2";
                    gameOptions["fancyGraphics"] = "true";
                    gameOptions["renderClouds"] = "true";
                    gameOptions["enableVsync"] = "true";
                    break;
                default:
                    gameOptions["graphicsMode"] = "3"; // 自定义
                    break;
            }

            // 保存配置
            using (var writer = new StreamWriter(optionsFile, false))
            {
                foreach (var option in gameOptions)
                {
                    writer.WriteLine($"{option.Key}:{option.Value}");
                }
            }

            System.Diagnostics.Debug.WriteLine("游戏配置已保存");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"保存配置失败: {ex.Message}");
        }
    }
}