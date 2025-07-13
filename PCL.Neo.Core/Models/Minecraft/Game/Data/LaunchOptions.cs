using PCL.Neo.Core.Models.Minecraft.Java;
using System.Text.Json.Serialization;

namespace PCL.Neo.Core.Models.Minecraft.Game.Data;

public sealed record LaunchOptions
{
    /// <summary>
    /// Java可执行文件路径
    /// </summary>
    [JsonIgnore]
    public required JavaRuntime RunnerJava { get; set; }

    /// <summary>
    /// 最大内存分配(MB)
    /// </summary>
    public int MaxMemoryMB { get; set; } = 2048;

    /// <summary>
    /// 最小内存分配(MB)
    /// </summary>
    public int MinMemoryMB { get; set; } = 512;

    /// <summary>
    /// 玩家用户名
    /// </summary>
    public string Username { get; set; } = "Steve";

    /// <summary>
    /// 玩家UUID
    /// </summary>
    public string UUID { get; set; } = string.Empty;

    /// <summary>
    /// 访问令牌
    /// </summary>
    public string AccessToken { get; set; } = string.Empty;

    /// <summary>
    /// 游戏窗口宽度
    /// </summary>
    public int WindowWidth { get; set; } = 854;

    /// <summary>
    /// 游戏窗口高度
    /// </summary>
    public int WindowHeight { get; set; } = 480;

    /// <summary>
    /// 是否全屏
    /// </summary>
    public bool FullScreen { get; set; } = false;

    /// <summary>
    /// 额外的JVM参数
    /// </summary>
    public List<string> ExtraJvmArgs { get; set; } = [];

    /// <summary>
    /// 额外的游戏参数
    /// </summary>
    public List<string> ExtraGameArgs { get; set; } = [];

    /// <summary>
    /// 环境变量
    /// </summary>
    public Dictionary<string, string> EnvironmentVariables { get; set; } = new();

    /// <summary>
    /// 启动后隐藏启动器
    /// </summary>
    public bool CloseAfterLaunch { get; set; } = false;

    /// <summary>
    /// 是否使用离线模式
    /// </summary>
    public bool IsOfflineMode { get; set; } = true;
}
