namespace PCL.Neo.Models.Minecraft.Game;

/// <summary>
/// 游戏启动选项
/// </summary>
public class LaunchOptions
{
    /// <summary>
    /// 游戏ID，如"1.20.1"、"1.16.5-forge-36.2.39"等
    /// </summary>
    public string VersionId { get; set; } = string.Empty;

    /// <summary>
    /// Minecraft根目录路径，通常为".minecraft"
    /// </summary>
    public string MinecraftDirectory { get; set; } = string.Empty;

    /// <summary>
    /// 游戏运行目录，如果为空则使用MinecraftDirectory
    /// </summary>
    public string GameDirectory { get; set; } = string.Empty;

    /// <summary>
    /// 游戏资源目录，如果为空则使用MinecraftDirectory/assets
    /// </summary>
    public string AssetsDirectory { get; set; } = string.Empty;

    /// <summary>
    /// 玩家用户名
    /// </summary>
    public string Username { get; set; } = string.Empty;

    /// <summary>
    /// 玩家UUID
    /// </summary>
    public string UUID { get; set; } = string.Empty;

    /// <summary>
    /// 访问令牌
    /// </summary>
    public string AccessToken { get; set; } = string.Empty;

    /// <summary>
    /// 用户类型
    /// </summary>
    public string UserType { get; set; } = "mojang";

    /// <summary>
    /// Java可执行文件路径
    /// </summary>
    public string JavaPath { get; set; } = string.Empty;

    /// <summary>
    /// 最大内存分配(MB)
    /// </summary>
    public int MaxMemoryMB { get; set; } = 2048;

    /// <summary>
    /// 最小内存分配(MB)
    /// </summary>
    public int MinMemoryMB { get; set; } = 512;

    /// <summary>
    /// JVM额外参数
    /// </summary>
    public List<string> JvmArguments { get; set; } = new();

    /// <summary>
    /// 游戏额外参数
    /// </summary>
    public List<string> GameArguments { get; set; } = new();

    /// <summary>
    /// 游戏窗口宽度
    /// </summary>
    public int Width { get; set; } = 854;

    /// <summary>
    /// 游戏窗口高度
    /// </summary>
    public int Height { get; set; } = 480;

    /// <summary>
    /// 是否全屏
    /// </summary>
    public bool Fullscreen { get; set; } = false;

    /// <summary>
    /// 启动后关闭启动器
    /// </summary>
    public bool CloseAfterLaunch { get; set; } = false;

    /// <summary>
    /// 服务器地址，如果不为空则自动加入服务器
    /// </summary>
    public string ServerAddress { get; set; } = string.Empty;

    /// <summary>
    /// 服务器端口
    /// </summary>
    public int ServerPort { get; set; } = 25565;

    /// <summary>
    /// 是否启用演示模式
    /// </summary>
    public bool Demo { get; set; } = false;

    /// <summary>
    /// 是否启用非正版验证
    /// </summary>
    public bool OfflineMode { get; set; } = false;

    /// <summary>
    /// 代理地址，如"http://127.0.0.1:8080"
    /// </summary>
    public string Proxy { get; set; } = string.Empty;

    /// <summary>
    /// 环境变量
    /// </summary>
    public Dictionary<string, string> EnvironmentVariables { get; set; } = new();

    /// <summary>
    /// 额外的游戏参数
    /// </summary>
    public Dictionary<string, string> ExtraGameArgs { get; set; } = new();

    /// <summary>
    /// 是否使用旧版启动方式
    /// </summary>
    public bool UseLegacyLauncher { get; set; } = false;

    /// <summary>
    /// 是否记录游戏日志
    /// </summary>
    public bool SaveGameLog { get; set; } = true;

    /// <summary>
    /// 游戏日志文件名
    /// </summary>
    public string LogFileName { get; set; } = "latest.log";

    /// <summary>
    /// 自定义启动参数字符串
    /// </summary>
    public string CustomLaunchArgs { get; set; } = string.Empty;
}
