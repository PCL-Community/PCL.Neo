using PCL.Neo.Core.Models.Minecraft.Game.Data.Arguments.Manifes;
using System.Text.Json;

namespace PCL.Neo.Core.Models.Minecraft.Game;

public static class Versions
{
    /// Minecraft版本清单API地址
    private const string VersionManifestUrl = "https://launchermeta.mojang.com/mc/game/version_manifest.json";

    /// <summary>
    /// 获取本地已安装的Minecraft版本
    /// </summary>
    public static async Task<List<VersionManifes>> GetLocalVersionsAsync(string minecraftDirectory)
    {
        var result = new List<VersionManifes>();
        var versionsDirectory = Path.Combine(minecraftDirectory, "versions");

        if (!Directory.Exists(versionsDirectory))
        {
            return result;
        }

        foreach (var versionDir in Directory.GetDirectories(versionsDirectory))
        {
            var versionId = Path.GetFileName(versionDir);
            var versionJsonPath = Path.Combine(versionDir, $"{versionId}.json");

            if (!File.Exists(versionJsonPath)) { continue; }

            try
            {
                var jsonContent = await File.ReadAllTextAsync(versionJsonPath);
                var versionInfo = JsonSerializer.Deserialize<VersionManifes>(jsonContent,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (versionInfo is not null)
                {
                    // 如果没有名称，使用ID作为名称
                    if (string.IsNullOrEmpty(versionInfo.Name))
                    {
                        versionInfo.Name = versionInfo.Id;
                    }

                    // 添加JsonData属性
                    versionInfo.JsonOriginData = jsonContent;

                    result.Add(versionInfo);
                }
            }
            catch (Exception)
            {
                // 忽略解析失败的版本文件
            }
        }

        return result;
    }

    /// <summary>
    /// 获取Minecraft远程版本列表
    /// </summary>
    public static async Task<List<VersionManifes>> GetRemoteVersionsAsync()
    {
        try
        {
            // 获取版本清单
            var response = await Shared.HttpClient.GetStringAsync(VersionManifestUrl);
            var manifest = JsonSerializer.Deserialize<VersionManifest>(response);

            if (manifest?.Versions == null)
            {
                return [];
            }

            return manifest.Versions.Select(version => new VersionManifes
                {
                    Id = version.Id,
                    Name = version.Id, // 使用ID作为名称
                    Type = MapVersionType(version.Type),
                    ReleaseTime = version.ReleaseTime,
                    Time = version.Time,
                    // 下载信息
                    Downloads = new DownloadsInfo { Client = new DownloadEntry { Url = version.Url } }
                })
                .ToList();
        }
        catch (Exception)
        {
            // 出现异常时返回空列表
            return [];
        }
    }

    private static string MapVersionType(string type)
    {
        return type.ToLower() switch
        {
            "release" => "release",
            "snapshot" => "snapshot",
            "old_alpha" => "old_alpha",
            "old_beta" => "old_beta",
            _ => "unknown"
        };
    }

    /// <summary>
    /// 通过ID获取特定版本信息
    /// </summary>
    public static async Task<VersionManifes?> GetVersionByIdAsync(string gameDir, string versionId)
    {
        var jsonPath = Path.Combine(gameDir, $"{versionId}.json");

        if (!File.Exists(jsonPath))
        {
            return null;
        }

        try
        {
            var jsonContent = await File.ReadAllTextAsync(jsonPath);
            var versionInfo = JsonSerializer.Deserialize<VersionManifes>(jsonContent);

            if (versionInfo != null)
            {
                if (string.IsNullOrEmpty(versionInfo.Name))
                {
                    versionInfo.Name = versionInfo.Id;
                }

                // 添加JsonData属性
                versionInfo.JsonOriginData = jsonContent;

                return versionInfo;
            }
        }
        catch (Exception)
        {
            // 忽略解析失败的版本文件
        }

        return null;
    }

    /// <summary>
    /// 从远程获取特定版本信息
    /// </summary>
    public static async Task<VersionManifes?> GetRemoteVersionInfoAsync(string versionId)
    {
        try
        {
            // 先获取版本清单
            var response = await Shared.HttpClient.GetStringAsync(VersionManifestUrl);
            var manifest = JsonSerializer.Deserialize<VersionManifest>(response);

            // 查找指定ID的版本
            var version = manifest?.Versions?.FirstOrDefault(v => v.Id == versionId);
            if (version == null)
            {
                return null;
            }

            // 获取详细版本信息
            var versionJsonResponse = await Shared.HttpClient.GetStringAsync(version.Url);
            var versionInfo = JsonSerializer.Deserialize<VersionManifes>(versionJsonResponse,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (versionInfo != null)
            {
                // 保存原始JSON数据
                versionInfo.JsonOriginData = versionJsonResponse;

                // 如果没有名称，使用ID作为名称
                if (string.IsNullOrEmpty(versionInfo.Name))
                {
                    versionInfo.Name = versionInfo.Id;
                }

                return versionInfo;
            }
        }
        catch (Exception)
        {
            // 出现异常时返回null
        }

        return null;
    }
}

/// 用于解析版本清单的内部类
internal class VersionManifest
{
    public LatestVersions? Latest { get; set; }
    public List<ManifestVersion>? Versions { get; set; }
}

internal class LatestVersions
{
    public string? Release { get; set; }
    public string? Snapshot { get; set; }
}

internal class ManifestVersion
{
    public string Id { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public string Time { get; set; } = string.Empty;
    public string ReleaseTime { get; set; } = string.Empty;
}
