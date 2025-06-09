using PCL.Neo.Core.Download;
using PCL.Neo.Core.Models.Minecraft.Game.Data;
using PCL.Neo.Core.Models.Minecraft.Java;
using PCL.Neo.Core.Utils;

namespace PCL.Neo.Core.Models.Minecraft.Game;

using DefaultJavaRuntimeCombine = (JavaRuntime? Java8, JavaRuntime? Java17, JavaRuntime? Java21);

public class GameService
{
    private IJavaManager JavaManager { get; }

    public static string DefaultGameDirectory =>
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".minecraft");

    public DefaultJavaRuntimeCombine DefaultJavaRuntimes => JavaManager.DefaultJavaRuntimes;

    public GameService(IJavaManager javaManager)
    {
        JavaManager = javaManager;
    }

    /// <summary>
    /// 获取游戏版本列表
    /// </summary>
    public static async Task<List<VersionInfo>> GetVersionsAsync(string? minecraftDirectory = null,
        bool forceRefresh = false)
    {
        string directory = minecraftDirectory ?? DefaultGameDirectory;
        // 获取本地版本
        var localVersions = await Versions.GetLocalVersionsAsync(directory);
        // 如果需要强制刷新或者本地版本为空，则获取远程版本
        if (forceRefresh || localVersions.Count == 0)
        {
            try
            {
                var remoteVersions = await Versions.GetRemoteVersionsAsync();

                // 合并版本列表，保留本地版本的信息
                Dictionary<string, VersionInfo> versionDict = new();

                foreach (var version in localVersions)
                {
                    versionDict[version.Id] = version;
                }

                foreach (var version in remoteVersions)
                {
                    versionDict.TryAdd(version.Id, version);
                }

                return [..versionDict.Values];
            }
            catch
            {
                // 如果获取远程版本失败，则返回本地版本
                return localVersions;
            }
        }

        return localVersions;
    }

    /// <summary>
    /// 下载指定版本的游戏
    /// </summary>
    public async Task<bool> DownloadVersionAsync(string versionId, IProgress<int>? progressCallback = null)
    {
        // 获取版本信息
        var versionInfo = await Versions.GetRemoteVersionInfoAsync(versionId);
        if (versionInfo == null)
        {
            return false;
        }

        // 保存版本信息到本地
        var versionDir = Path.Combine(DefaultGameDirectory, "versions", versionId);
        Directory.CreateDirectory(versionDir);
        var versionJsonPath = Path.Combine(versionDir, $"{versionId}.json");
        await File.WriteAllTextAsync(versionJsonPath, versionInfo.JsonData);

        // 下载资源文件
        await DownloadAssetsAsync(versionInfo, progressCallback);

        // 下载库文件
        await DownloadLibrariesAsync(versionInfo, progressCallback);

        // 下载Minecraft主JAR文件
        var jarUrl = versionInfo.Downloads.Client.Url;
        var jarPath = Path.Combine(versionDir, $"{versionId}.jar");
        await DownloadReceipt.FastDownloadAsync(jarUrl, jarPath);

        return true;
    }

    /// <summary>
    /// 下载游戏资源文件
    /// </summary>
    private async Task DownloadAssetsAsync(VersionInfo versionInfo, IProgress<int>? progressCallback = null)
    {
        // 下载assets索引文件
        var assetsDir = Path.Combine(DefaultGameDirectory, "assets");
        var indexesDir = Path.Combine(assetsDir, "indexes");
        var objectsDir = Path.Combine(assetsDir, "objects");

        Directory.CreateDirectory(indexesDir);
        Directory.CreateDirectory(objectsDir);

        var assetsIndexUrl = versionInfo.AssetIndex.Url;
        var assetsIndexPath = Path.Combine(indexesDir, $"{versionInfo.AssetIndex.Id}.json");

        await DownloadReceipt.FastDownloadAsync(assetsIndexUrl, assetsIndexPath);

        // 解析assets索引文件
        var assetsIndexJson = await File.ReadAllTextAsync(assetsIndexPath);
        var assetsIndex = System.Text.Json.JsonSerializer.Deserialize<AssetIndex>(assetsIndexJson);

        if (assetsIndex?.Objects == null)
        {
            return;
        }

        // 下载assets文件
        int totalAssets = assetsIndex.Objects.Count;
        int downloadedAssets = 0;

        foreach (var asset in assetsIndex.Objects)
        {
            var hash = asset.Value.Hash;
            var prefix = hash.Substring(0, 2);
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
    private async Task DownloadLibrariesAsync(VersionInfo versionInfo, IProgress<int>? progressCallback = null)
    {
        var librariesDir = Path.Combine(DefaultGameDirectory, "libraries");
        Directory.CreateDirectory(librariesDir);

        var libraries = versionInfo.Libraries;
        int totalLibraries = libraries.Count;
        int downloadedLibraries = 0;

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
                        Directory.CreateDirectory(Path.GetDirectoryName(nativePath)!);
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
    private bool EvaluateRules(List<Rule> rules)
    {
        bool allow = true;

        foreach (var rule in rules)
        {
            bool matches = true;

            if (rule.Os != null)
            {
                if (rule.Os.Name != null && rule.Os.Name != SystemUtils.Os.ToMajangApiName()) matches = false;
                if (rule.Os.Arch != null && rule.Os.Arch != SystemUtils.Architecture.ToMajangApiName()) matches = false;
            }

            if (matches)
                allow = rule.Allow;
        }

        return allow;
    }

    /// <summary>
    /// 检查版本是否已安装
    /// </summary>
    public bool IsVersionInstalled(string versionId, string? minecraftDirectory = null)
    {
        string directory = minecraftDirectory ?? DefaultGameDirectory;
        string versionJsonPath = Path.Combine(directory, "versions", versionId, $"{versionId}.json");
        string versionJarPath = Path.Combine(directory, "versions", versionId, $"{versionId}.jar");

        return File.Exists(versionJsonPath) && File.Exists(versionJarPath);
    }

    /// <summary>
    /// 删除版本
    /// </summary>
    public async Task DeleteVersionAsync(string versionId, string? minecraftDirectory = null)
    {
        string directory = minecraftDirectory ?? DefaultGameDirectory;
        string versionDir = Path.Combine(directory, "versions", versionId);

        if (Directory.Exists(versionDir))
        {
            try
            {
                Directory.Delete(versionDir, true);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"删除版本 {versionId} 失败: {ex.Message}");
                throw;
            }
        }
    }

    /// <summary>
    /// 检查Java版本是否兼容指定的Minecraft版本
    /// </summary>
    public bool IsJavaCompatibleWithGame(JavaRuntime javaRuntime, string minecraftVersion)
    {
        // 先获取Java版本
        string javaVersionString = javaRuntime.Version;

        // 如果无法获取版本信息，直接返回不兼容
        if (string.IsNullOrEmpty(javaVersionString) || javaVersionString == "未知" || javaVersionString == "无法获取")
        {
            return false;
        }

        // 尝试解析Java版本号
        int javaMajorVersion;
        if (javaVersionString.StartsWith("1."))
        {
            // 旧版Java格式：1.8.0_xxx
            javaMajorVersion = 8; // 假设为Java 8
        }
        else
        {
            // 新版Java格式：11.0.x, 17.0.x等
            int dotIndex = javaVersionString.IndexOf('.');
            string majorString = dotIndex > 0 ? javaVersionString.Substring(0, dotIndex) : javaVersionString;

            if (!int.TryParse(majorString, out javaMajorVersion))
            {
                return false; // 解析失败
            }
        }

        // 获取Minecraft版本对应的需求Java版本
        int requiredJavaVersion = GetRequiredJavaVersion(minecraftVersion);

        // 比较版本
        return javaMajorVersion >= requiredJavaVersion;
    }

    /// <summary>
    /// 获取Minecraft版本需要的Java版本
    /// </summary>
    private int GetRequiredJavaVersion(string minecraftVersion)
    {
        // 解析Minecraft版本号
        string[] parts = minecraftVersion.Split('.');
        if (parts.Length < 2 || !int.TryParse(parts[0], out int majorVersion) ||
            !int.TryParse(parts[1], out int minorVersion))
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
        else
        {
            return 8;
        }
    }
}