namespace PCL.Neo.Core.Models.Profile;

/// <summary>
/// 游戏档案模型，用于保存不同游戏配置
/// </summary>
public record GameProfile
{
    // 基本信息
    public required string Id { get; init; } = Guid.NewGuid().ToString();
    public required string Name { get; set; }
    public string Description { get; set; } = string.Empty;
    public DateTime CreatedTime { get; init; } = DateTime.Now;
    public DateTime LastUsed { get; set; } = DateTime.Now;

    // 游戏版本信息
    public string GameVersion { get; set; } = string.Empty;
    public string GameType { get; set; } = "Vanilla"; // Vanilla, Forge, Fabric, Quilt等

    // 资源设置
    public string GameDirectory { get; set; } = string.Empty; // 为空时使用默认目录
    public string AssetsDirectory { get; set; } = string.Empty; // 为空时使用默认目录

    // Java设置
    public string JavaPath { get; set; } = string.Empty; // 为空时使用默认Java
    public int JavaMemoryMb { get; set; } = 2048; // 默认2GB内存
    public string JavaArguments { get; set; } = string.Empty; // 额外JVM参数

    // 模组和资源包
    public List<ModInfo> Mods { get; set; } = [];
    public List<ResourcePackInfo> ResourcePacks { get; set; } = [];
    public List<string> ActiveResourcePacks { get; set; } = []; // 按顺序激活的资源包ID

    // 启动器相关设置
    public string GameResolution { get; set; } = "854x480"; // 游戏分辨率
    public bool FullScreen { get; set; } = false; // 是否全屏

    // 自定义启动参数
    public string CustomLaunchArguments { get; set; } = string.Empty;

    // 快速配置预设
    public bool IsPreset { get; set; } = false;

    public GameProfile CloneProfile(string newName)
    {
        return this with
        {
            Id = Guid.NewGuid().ToString(),
            Name = newName,
            CreatedTime = DateTime.Now,
            LastUsed = DateTime.Now
        };
    }
}
