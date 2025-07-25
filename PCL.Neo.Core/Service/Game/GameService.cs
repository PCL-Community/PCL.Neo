using PCL.Neo.Core.Download;
using PCL.Neo.Core.Models.Minecraft.Game;
using PCL.Neo.Core.Models.Minecraft.Game.Data.Arguments.Manifest;
using PCL.Neo.Core.Models.Minecraft.Java;
using PCL.Neo.Core.Utils;
using PCL.Neo.Core.Utils.Logger;
using System.Text.Json;
using VersionManifest = PCL.Neo.Core.Models.Minecraft.Game.Data.Arguments.Manifest.VersionManifest;

namespace PCL.Neo.Core.Service.Game;

/// <summary>
/// 提供了 Minecraft 各版本的本地/远程获取、下载、校验、删除等一站式服务，并结合 Java 运行环境的管理，确保游戏运行环境的完整性和兼容性。
/// </summary>
public class GameService(IJavaManager javaManager) : IGameService
{
    public static string DefaultGameDirectory =>
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".minecraft");


    /// <inheritdoc/>
    public async Task<bool> DownloadVersionAsync(string gameName, IProgress<int>? progressCallback = null)
    {
        // 获取版本信息
        var versionInfo = await Versions.GetRemoteVersionInfoAsync(gameName);
        if (versionInfo == null)
        {
            return false;
        }

        // 保存版本信息到本地
        var versionDir = Path.Combine(DefaultGameDirectory, "versions", gameName);
        Directory.CreateDirectory(versionDir);
        var versionJsonPath = Path.Combine(versionDir, $"{gameName}.json");
        await File.WriteAllTextAsync(versionJsonPath, versionInfo.JsonOriginData);

        // 下载资源文件
        await DownloadAssetsAsync(versionInfo, progressCallback);

        // 下载库文件
        await DownloadLibrariesAsync(versionInfo, progressCallback);

        // 下载Minecraft主JAR文件
        ArgumentNullException.ThrowIfNull(versionInfo.Downloads);
        ArgumentNullException.ThrowIfNull(versionInfo.Downloads.Client);

        var jarUrl = versionInfo.Downloads.Client.Url;
        var jarPath = Path.Combine(versionDir, $"{gameName}.jar");
        await DownloadReceipt.FastDownloadAsync(jarUrl, jarPath);

        return true;
    }

    /// <summary>
    /// 下载游戏资源文件
    /// </summary>
    /// <exception cref="ArgumentNullException">Throw if AssIndex is null.</exception>
    private static async Task DownloadAssetsAsync(VersionManifest versionManifest,
        IProgress<int>? progressCallback = null)
    {
        // pre check
        ArgumentNullException.ThrowIfNull(versionManifest.AssetIndex); // not allow null

        // 下载assets索引文件
        var assetsDir = Path.Combine(DefaultGameDirectory, "assets");
        var indexesDir = Path.Combine(assetsDir, "indexes");
        var objectsDir = Path.Combine(assetsDir, "objects");

        Directory.CreateDirectory(indexesDir);
        Directory.CreateDirectory(objectsDir);

        var assetsIndexUrl = versionManifest.AssetIndex.Url;
        var assetsIndexPath = Path.Combine(indexesDir, $"{versionManifest.AssetIndex.Id}.json");

        await DownloadReceipt.FastDownloadAsync(assetsIndexUrl, assetsIndexPath);

        // 解析assets索引文件
        var assetsIndexJson = await File.ReadAllTextAsync(assetsIndexPath);
        var assetsIndex = JsonSerializer.Deserialize<AssetIndex>(assetsIndexJson);

        if (assetsIndex?.Objects == null)
        {
            return;
        }

        // 下载assets文件
        var totalAssets = assetsIndex.Objects.Count;
        var downloadedAssets = 0;

        foreach (var asset in assetsIndex.Objects)
        {
            var hash = asset.Value.Hash;
            var prefix = hash[..2];
            var assetObjectDir = Path.Combine(objectsDir, prefix);
            var assetObjectPath = Path.Combine(assetObjectDir, hash);

            if (!File.Exists(assetObjectPath))
            {
                Directory.CreateDirectory(assetObjectDir);
                var assetUrl = $"https://resources.download.minecraft.net/{prefix}/{hash}";
                await DownloadReceipt.FastDownloadAsync(assetUrl, assetObjectPath);
            }

            downloadedAssets++;
            progressCallback?.Report((int)((float)downloadedAssets / totalAssets * 100));
        }
    }

    /// <summary>
    /// 下载游戏库文件
    /// </summary>
    private static async Task DownloadLibrariesAsync(VersionManifest versionManifest,
        IProgress<int>? progressCallback = null)
    {
        var librariesDir = Path.Combine(DefaultGameDirectory, "libraries");
        Directory.CreateDirectory(librariesDir);

        var libraries = versionManifest.Libraries ??
                        throw new ArgumentNullException(nameof(versionManifest.Libraries),
                            "The libraries property is null.");
        var totalLibraries = libraries.Count;
        var downloadedLibraries = 0;

        foreach (var library in libraries)
        {
            // 检查是否适用于当前系统
            if (library.Rules != null && !EvaluateRules(library.Rules))
            {
                downloadedLibraries++;
                continue;
            }

            // 获取库文件路径
            var paths = library.Name.Split(':');
            var group = paths[0].Replace('.', '/');
            var artifact = paths[1];
            var version = paths[2];

            var relativePath = $"{group}/{artifact}/{version}/{artifact}-{version}.jar";
            var libraryPath = Path.Combine(librariesDir, relativePath);

            // 下载库文件
            if (!File.Exists(libraryPath))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(libraryPath)!);
                var libraryUri = library.Downloads?.Artifact?.Url;

                if (!string.IsNullOrEmpty(libraryUri))
                {
                    await DownloadReceipt.FastDownloadAsync(libraryUri, libraryPath);
                }
            }

            // 下载natives文件
            if (library.Downloads?.Classifiers != null)
            {
                var nativeKey = SystemUtils.GetNativeKey();
                if (nativeKey != null && library.Downloads.Classifiers.TryGetValue(nativeKey, out var nativeDownload))
                {
                    var nativePath = Path.Combine(librariesDir, nativeDownload.Path);

                    if (!File.Exists(nativePath))
                    {
                        var directoryName = Path.GetDirectoryName(nativePath);
                        ArgumentNullException.ThrowIfNull(directoryName); // ensure not null

                        Directory.CreateDirectory(directoryName);

                        await DownloadReceipt.FastDownloadAsync(nativeDownload.Url, nativePath);
                    }
                }
            }

            downloadedLibraries++;
            progressCallback?.Report((int)((float)downloadedLibraries / totalLibraries * 100));
        }
    }

    /// <summary>
    /// 评估规则是否适用于当前系统
    /// </summary>
    private static bool EvaluateRules(List<Rule> rules)
    {
        var allow = true;

        foreach (var rule in rules)
        {
            var matches = true;

            if (rule.Os != null)
            {
                if (rule.Os.Name != null && rule.Os.Name != SystemUtils.Os.ToMajangApiName()) matches = false;
                if (rule.Os.Arch != null && rule.Os.Arch != SystemUtils.Architecture.ToMajangApiName()) matches = false;
            }

            if (matches)
                allow = rule.Action == "allow";
        }

        return allow;
    }

    /// <summary>
    /// 检查版本是否已安装
    /// </summary>
    /// <param name="versionId">版本ID</param>
    /// <param name="minecraftDirectory">Minecraft 目录</param>
    /// <returns>是否已安装</returns>
    [Obsolete]
    public static bool IsVersionInstalled(string versionId, string? minecraftDirectory = null)
    {
        var directory = minecraftDirectory ?? DefaultGameDirectory;
        var versionJsonPath = Path.Combine(directory, "versions", versionId, $"{versionId}.json");
        var versionJarPath = Path.Combine(directory, "versions", versionId, $"{versionId}.jar");

        return File.Exists(versionJsonPath) && File.Exists(versionJarPath);
    }

    /// <inheritdoc/>
    public void DeleteVersionAsync(string gameName, string? minecraftDirectory = null)
    {
        var directory = minecraftDirectory ?? DefaultGameDirectory;
        var versionDir = Path.Combine(directory, "versions", gameName);

        if (Directory.Exists(versionDir))
        {
            try
            {
                Directory.Delete(versionDir, true);
            }
            catch (Exception ex)
            {
                NewLogger.Logger.LogError($"删除版本 {gameName} 失败", ex);
                throw;
            }
        }
    }

    /// <inheritdoc/>
    public bool IsJavaCompatibleWithGame(JavaRuntime javaRuntime, string minecraftVersion)
    {
        // 先获取Java版本
        var javaVersionString = javaRuntime.Version;

        // 如果无法获取版本信息，直接返回不兼容
        if (string.IsNullOrEmpty(javaVersionString) || javaVersionString == "未知" || javaVersionString == "无法获取")
        {
            return false;
        }

        // 尝试解析Java版本号
        int javaMajorVersion;
        var spilted = javaVersionString.Split('.');

        if (spilted.Length < 3)
        {
            throw new ArgumentException("Java version string is invalid.", nameof(javaRuntime));
        }

        if (spilted[0] == "1")
        {
            // 旧版Java格式：1.8.0_xxx
            int.TryParse(spilted[1], out javaMajorVersion);
        }
        else
        {
            // 新版Java格式：11.0.x, 17.0.x等
            var majorString = spilted[0];

            if (!int.TryParse(majorString, out javaMajorVersion))
            {
                throw new ArgumentException("Java version string is invalid.", nameof(javaRuntime));
            }
        }

        // 获取Minecraft版本对应的需求Java版本
        var requiredJavaVersion = GetRequiredJavaVersion(minecraftVersion);

        // 比较版本
        return javaMajorVersion >= requiredJavaVersion;
    }

    /// <summary>
    /// 获取Minecraft版本需要的Java版本
    /// </summary>
    private static int GetRequiredJavaVersion(string minecraftVersion)
    {
        // 解析Minecraft版本号
        var parts = minecraftVersion.Split('.');
        if (parts.Length < 2
            || !int.TryParse(parts[0], out var majorVersion)
            || !int.TryParse(parts[1], out var minorVersion))
        {
            return 8; // 默认要求Java 8
        }

        // 根据Minecraft版本推断所需的Java版本
        // Minecraft 1.17+需要Java 16+
        if (majorVersion == 1 && minorVersion >= 17)
        {
            return 16;
        }
        // Minecraft 1.16及以下需要Java 8

        return 8;
    }
}
