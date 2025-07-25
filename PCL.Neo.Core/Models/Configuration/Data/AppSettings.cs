using Avalonia.Styling;
using Newtonsoft.Json;
using PCL.Neo.Core.Models.Minecraft.Java;
using System.Diagnostics;

namespace PCL.Neo.Core.Models.Configuration.Data;

/// <summary>
/// 应用程序全局设置
/// </summary>
[ConfigurationInfo("appSettings.json")]
public record AppSettingsData
{
    /// <summary>
    /// 应用程序主题
    /// </summary>
    public ThemeVariant Theme { get; set; } = ThemeVariant.Light;

    /// <summary>
    /// 应用程序语言
    /// </summary>
    public string Language { get; set; } = "zh-CN";

    /// <summary>
    /// 全局最大内存
    /// </summary>
    public int MaxMemoryMb { get; set; } = 2048;

    /// <summary>
    /// 全局最小内存
    /// </summary>
    public int MinMemoryMb { get; set; } = 512;

    /// <summary>
    /// 是否启用内存优化
    /// </summary>
    public bool MemoryOptimize { get; set; } = false;

    /// <summary>
    /// 当前游戏档案
    /// </summary>
    public string CurrentGameProfile { get; set; } = "Default";

    /// <summary>
    /// 当前游戏的名称
    /// </summary>
    public string CurrentGame { get; set; } = string.Empty;

    /// <summary>
    /// 版本隔离
    /// </summary>
    public bool VersionIndie { get; set; } = true;

    /// <summary>
    /// 游戏窗口标题
    /// </summary>
    public string GameTitle { get; set; } = string.Empty;

    /// <summary>
    /// 游戏自定义信息（暂未知作用）
    /// </summary>
    public string CustomizedInfo { get; set; } = string.Empty;

    /// <summary>
    /// 启动器可见性
    /// </summary>
    public LauncherVisibleType LauncherVisible { get; set; } = LauncherVisibleType.None;

    /// <summary>
    /// 游戏进程优先级
    /// </summary>
    public ProcessPriorityClass ProcessPriority { get; set; } = ProcessPriorityClass.Normal;

    /// <summary>
    /// 窗口大小
    /// </summary>
    public WindowSizeType WindowSize { get; set; } = WindowSizeType.Default; // TODO: 修改游戏启动代码，适配这个

    /// <summary>
    /// 记住的Java路径
    /// </summary>
    public List<string> JavaPaths { get; set; } = [];

    [JsonIgnore]
    public JavaRuntime? GameJava { get; set; }

    /// <summary>
    /// 默认Java路径
    /// </summary>
    public string DefaultJava { get; set; } = string.Empty;

    /// <summary>
    /// 默认下载源
    /// </summary>
    public DownloadProviderType DownloadProvider { get; set; } = DownloadProviderType.PreferOfficial;

    /// <summary>
    /// 下载线程数
    /// </summary>
    public int DownloadThreads { get; set; } = 4;

    /// <summary>
    /// 下载速度限制，0为无限制
    /// </summary>
    public ulong DownloadSpeedLimit { get; set; } = 0;

    /// <summary>
    /// 标题栏内容（空为无；以Ψ开头的合法文件路径为图片标题；否则为文本）
    /// </summary>
    public string TitleBar { get; set; } = string.Empty;
}
