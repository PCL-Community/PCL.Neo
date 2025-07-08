using System.Text.Json.Serialization;

namespace PCL.Neo.Core.Models.Minecraft.Game.Data;

public enum Icons : byte
{
    Auto = 0,
    Custom = 1,
    CubeStone = 2,
    CommandBlock = 3,
    GrassBlock = 4,
    EarthenPath = 5,
    Anvil = 6,
    RedStone = 7,
    RedStoneLightOff = 8,
    RedStoneLightOn = 9,
    Egg = 10,
    Fabric = 11,
    NeoForge = 12
}

// TODO)) 此枚举为UI显示，后续建议移到ViewModel中
public enum VersionCardType : byte
{
    Auto = 0,
    Hide = 1,
    Moddable = 2,
    Normal = 3,
    Unusual = 4,
    FoolsDay = 5,
    Error = 6
}

/// <summary>
/// 常规游戏版本的版本号，后续可能会拓展到模组版本
/// </summary>
public record GameVersionNum(byte Sub, byte? Fix = null) : IComparable<GameVersionNum>
{
    private readonly (byte Major, byte Sub, byte Fix) _version = (1, Sub, Fix ?? 0);

    public byte Major => _version.Major;
    public byte Sub => _version.Sub;
    public byte? Fix => _version.Fix > 0 ? _version.Fix : null;

    public int CompareTo(GameVersionNum? other)
    {
        return other == null ? 1 : (Major, Sub, Fix ?? 0).CompareTo((other.Major, other.Sub, other.Fix ?? 0));
    }

    public override string ToString()
    {
        return Fix.HasValue ? $"{Major}.{Sub}.{Fix}" : $"{Major}.{Sub}";
    }
}

public enum ModLoader : byte
{
    None = 0,
    Forge = 1,
    Fabric = 2,
    NeoForge = 3,
    LiteLoader = 4,
    Rift = 5,
    Quilt = 6
}

// TODO)) 最好把GameEntity里的合并到这里来
public class VersionInfo
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;

    [JsonPropertyName("releaseTime")]
    public string ReleaseTime { get; set; } = string.Empty;

    [JsonPropertyName("time")]
    public string Time { get; set; } = string.Empty;

    [JsonPropertyName("minecraftArguments")]
    public string? MinecraftArguments { get; set; }

    [JsonPropertyName("arguments")]
    public Arguments? Arguments { get; set; }

    [JsonPropertyName("mainClass")]
    public string MainClass { get; set; } = string.Empty;

    [JsonPropertyName("libraries")]
    public List<Library>? Libraries { get; set; }

    [JsonPropertyName("inheritsFrom")]
    public string? InheritsFrom { get; set; }

    [JsonPropertyName("assetIndex")]
    public AssetIndexInfo? AssetIndex { get; set; }

    [JsonPropertyName("assets")]
    public string? Assets { get; set; }

    [JsonPropertyName("downloads")]
    public DownloadsInfo? Downloads { get; set; }

    [JsonPropertyName("javaVersion")]
    public JavaVersionInfo? JavaVersion { get; set; }

    /// <summary>
    /// 存储原始的JSON数据
    /// </summary>
    [JsonIgnore]
    public string? JsonData { get; set; }
}

public class AssetIndexInfo
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("url")]
    public string Url { get; set; } = string.Empty;

    [JsonPropertyName("sha1")]
    public string Sha1 { get; set; } = string.Empty;

    [JsonPropertyName("size")]
    public int Size { get; set; }
}

public class Library
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("url")]
    public string? Url { get; set; }

    [JsonPropertyName("downloads")]
    public LibraryDownloads? Downloads { get; set; }

    [JsonPropertyName("natives")]
    public Dictionary<string, string>? Natives { get; set; }

    [JsonPropertyName("rules")]
    public List<Rule>? Rules { get; set; }
}

public class LibraryDownloads
{
    [JsonPropertyName("artifact")]
    public Artifact? Artifact { get; set; }

    [JsonPropertyName("classifiers")]
    public Dictionary<string, Artifact>? Classifiers { get; set; }
}

public class Artifact
{
    [JsonPropertyName("path")]
    public string? Path { get; set; }

    [JsonPropertyName("url")]
    public string? Url { get; set; }

    [JsonPropertyName("sha1")]
    public string? Sha1 { get; set; }

    [JsonPropertyName("size")]
    public int Size { get; set; }
}

public class DownloadsInfo
{
    [JsonPropertyName("client")]
    public DownloadEntry? Client { get; set; }

    [JsonPropertyName("server")]
    public DownloadEntry? Server { get; set; }
}

public class DownloadEntry
{
    [JsonPropertyName("url")]
    public string Url { get; set; } = string.Empty;

    [JsonPropertyName("sha1")]
    public string Sha1 { get; set; } = string.Empty;

    [JsonPropertyName("size")]
    public int Size { get; set; }
}

public class JavaVersionInfo
{
    [JsonPropertyName("component")]
    public string Component { get; set; } = string.Empty;

    [JsonPropertyName("majorVersion")]
    public int MajorVersion { get; set; }
}
