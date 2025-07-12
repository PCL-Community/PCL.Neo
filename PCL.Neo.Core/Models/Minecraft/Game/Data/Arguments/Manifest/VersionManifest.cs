using System.Text.Json.Serialization;

namespace PCL.Neo.Core.Models.Minecraft.Game.Data.Arguments.Manifest;

public record VersionManifest
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("type")]
    public string Type { get; init; } = string.Empty;

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

    [JsonPropertyName("logging")]
    public LoggingInfo? Logging { get; set; }

    /// <summary>
    /// 存储原始的JSON数据
    /// </summary>
    [JsonIgnore]
    public string? JsonOriginData { get; set; }
}

public record AssetIndexInfo
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

public record Library
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

public record LibraryDownloads
{
    [JsonPropertyName("artifact")]
    public Artifact? Artifact { get; set; }

    [JsonPropertyName("classifiers")]
    public Dictionary<string, Artifact>? Classifiers { get; set; }
}

public record Artifact
{
    [JsonPropertyName("path")]
    public required string Path { get; set; }

    [JsonPropertyName("url")]
    public required string Url { get; set; }

    [JsonPropertyName("sha1")]
    public string? Sha1 { get; set; }

    [JsonPropertyName("size")]
    public required int Size { get; set; }
}

public record DownloadsInfo
{
    [JsonPropertyName("client")]
    public DownloadEntry? Client { get; set; }

    [JsonPropertyName("server")]
    public DownloadEntry? Server { get; set; }
}

public record DownloadEntry
{
    [JsonPropertyName("url")]
    public string Url { get; set; } = string.Empty;

    [JsonPropertyName("sha1")]
    public string Sha1 { get; set; } = string.Empty;

    [JsonPropertyName("size")]
    public int Size { get; set; }
}

public record JavaVersionInfo
{
    [JsonPropertyName("component")]
    public string Component { get; set; } = string.Empty;

    [JsonPropertyName("majorVersion")]
    public int MajorVersion { get; set; }
}

public class LoggingInfo
{
    public class ClientInfo
    {
        public class FileInfo
        {
            [JsonPropertyName("id")]
            public required string Id { get; set; }

            [JsonPropertyName("sha1")]
            public string Sha1 { get; set; } = string.Empty;

            [JsonPropertyName("size")]
            public int Size { get; set; }

            [JsonPropertyName("url")]
            public string Url { get; set; } = string.Empty;
        }

        [JsonPropertyName("argument")]
        public required string Argument { get; set; }

        [JsonPropertyName("file")]
        public required FileInfo File { get; set; }

        [JsonPropertyName("type")]
        public string Type { get; set; } = string.Empty;
    }

    [JsonPropertyName("client")]
    public required ClientInfo Client { get; set; }
}
