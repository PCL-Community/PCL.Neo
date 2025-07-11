using PCL.Neo.Core.Download;
using PCL.Neo.Core.Utils;
using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace PCL.Neo.Core.Models.Minecraft.Java;

public sealed partial class JavaManager
{
    // TODO: 应该设置多个下载源，从配置文件中获取
    private static string MetaUrl
    {
        get =>
            "https://piston-meta.mojang.com/v1/products/java-runtime/2ec0cc96c44e5a76b9c8b7c39df7210883d12871/all.json";
    }

    /// <summary>
    /// MOJANG 提供的平台记录
    /// </summary>
    /// <param name="Value">获取表示平台的字符串</param>
    public sealed record MojangJavaVersion(string Value)
    {
        public static readonly MojangJavaVersion Α = new("java-runtime-alpha");
        public static readonly MojangJavaVersion Β = new("java-runtime-beta");
        public static readonly MojangJavaVersion Δ = new("java-runtime-delta");
        public static readonly MojangJavaVersion Γ = new("java-runtime-gamma");
        public static readonly MojangJavaVersion Γs = new("java-runtime-gamma-snapshot");
        public static readonly MojangJavaVersion Legacy = new("jre-legacy");
    }

    /// <summary>
    /// 从 MOJANG 官方下载 JRE
    /// </summary>
    /// <param name="platform">平台</param>
    /// <param name="destinationFolder">目标文件夹</param>
    /// <param name="progress">显示进度</param>
    /// <param name="cancellationToken">用于中断下载</param>
    /// <param name="version">要下载的版本，有Alpha, Beta, Gamma, Delta等</param>
    /// <returns>如果未成功下载为null，成功下载则为java可执行文件所在的目录</returns>
    public static async Task<string?> FetchJavaOnline(string platform, string destinationFolder,
        MojangJavaVersion version,
        IProgress<ValueTuple<int, int>>? progress, CancellationToken cancellationToken = default)
    {
        // TODO: 根据配置文件切换下载源
        Uri metaUrl = new(MetaUrl);
        var allJson = await Shared.HttpClient.GetStringAsync(metaUrl, cancellationToken);
        var manifestJson = string.Empty;
        using (var document = JsonDocument.Parse(allJson))
        {
            var root = document.RootElement;
            if (root.TryGetProperty(platform, out JsonElement platformElement) && // get platform property
                platformElement.TryGetProperty(version.Value, out var gammaArray) && // get target version property
                gammaArray.GetArrayLength() > 0 && // has value
                gammaArray[0].TryGetProperty("manifest", out JsonElement manifestElement) && // get manifest property
                manifestElement.TryGetProperty("url", out var manifestUriElement)) // get url property
            {
                var manifestUri = manifestUriElement.GetString(); // convert to string
                if (!string.IsNullOrEmpty(manifestUri))
                {
                    manifestJson = await Shared.HttpClient.GetStringAsync(manifestUri, cancellationToken);
                }
            }

            if (string.IsNullOrEmpty(manifestJson))
            {
                Console.WriteLine("未找到平台 Java 清单");
                return null;
            }
        }

        var manifest = JsonNode.Parse(manifestJson)?.AsObject();
        if (manifest == null || !manifest.TryGetPropertyValue("files", out var filesNode))
        {
            Console.WriteLine("无效的清单文件");
            return null;
        }

        var files = filesNode!.AsObject();
        var tasks = new List<Task>(files.Count);
        var executableFiles = new List<string>(files.Count);
        foreach (var (filePath, value) in files)
        {
            var fileInfo = value!.AsObject();

            if (!fileInfo.TryGetPropertyValue("type", out var typeNode) // try get tyoe property
                || typeNode!.ToString() != "file") // check if type is file
                continue;

            if (!fileInfo.TryGetPropertyValue("downloads", out var downloadsNode))
                continue;

            var downloads = downloadsNode!.AsObject();

            bool isExecutable =
                fileInfo.TryGetPropertyValue("executable", out var execNode) &&
                execNode!.GetValue<bool>();

            string? urlRaw = null, sha1Raw = null, urlLzma = null, sha1Lzma = null;

            if (downloads.TryGetPropertyValue("raw", out var rawNode))
            {
                var raw = rawNode!.AsObject();
                urlRaw = raw["url"]!.ToString();
                sha1Raw = raw["sha1"]!.ToString();
            }

            Debug.Assert(rawNode != null && !string.IsNullOrEmpty(urlRaw) && !string.IsNullOrEmpty(sha1Raw),
                "rawNode 不存在");

            if (downloads.TryGetPropertyValue("lzma", out var lzmaNode))
            {
                var lzma = lzmaNode!.AsObject();
                urlLzma = lzma["url"]!.ToString();
                sha1Lzma = lzma["sha1"]!.ToString();
            }

            var localFilePath = Path.Combine(destinationFolder,
                filePath.Replace("/", Path.DirectorySeparatorChar.ToString()));

            if (isExecutable)
            {
                executableFiles.Add(localFilePath);
            }

            Directory.CreateDirectory(Path.GetDirectoryName(localFilePath)!);

            // 有的文件有LZMA压缩但是有的 tm 没有，尼玛搞了个解压缩发现文件少了几个
            // 要分类讨论，sb MOJANG
            if (lzmaNode != null && !string.IsNullOrEmpty(urlLzma))
            {
                tasks.Add(Task.Run(async () =>
                {
                    try
                    {
                        await using var lzmaFs = await DownloadReceipt.FastDownloadAsStreamAsync(urlLzma,
                            localFilePath + ".lzma", sha1Lzma, cancellationToken);
                        await using var fs = lzmaFs.DecompressLzma(localFilePath);

                        if (fs is null)
                        {
                            Console.WriteLine("outStream 为空");
                            return;
                        }

                        if (!await new FileIntegrity { Hash = sha1Raw }.VerifyAsync(fs, cancellationToken))
                        {
                            Console.WriteLine("解压后的文件SHA-1与源提供的不匹配");
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e); // TODO: 处理异常
                        throw;
                    }
                    finally
                    {
                        if (File.Exists(localFilePath + ".lzma"))
                            File.Delete(localFilePath + ".lzma");
                    }
                }, cancellationToken));
            }
            else
                tasks.Add(DownloadReceipt.FastDownloadAsync(urlRaw, localFilePath, sha1Raw, cancellationToken));
        }

        if (progress != null)
        {
            var completed = 0;
            var total = tasks.Count;
            while (total - completed > 0)
            {
                var finishedTask = await Task.WhenAny(tasks);
                progress.Report((++completed, total));
                try
                {
                    await finishedTask;
                }
                catch (Exception ex)
                {
                    // TODO: 处理异常
                    Console.WriteLine(ex);
                }
            }
        }

        await Task.WhenAll(tasks);

#pragma warning disable CA1416
        if (SystemUtils.Os is not SystemUtils.RunningOs.Windows)
        {
            Parallel.ForEach(executableFiles, executableFile =>
            {
                if (string.IsNullOrEmpty(executableFile) || !File.Exists(executableFile))
                    throw new FileNotFoundException();
                executableFile.SetFileExecutableUnix();
            });
        }
#pragma warning restore CA1416
        var targetFolder = SystemUtils.Os switch
        {
            SystemUtils.RunningOs.MacOs => Path.Combine(destinationFolder, "jre.bundle/Contents/Home/bin"),
            SystemUtils.RunningOs.Linux => Path.Combine(destinationFolder, "bin"),
            SystemUtils.RunningOs.Windows => Path.Combine(destinationFolder, "bin"),
            _ => throw new ArgumentOutOfRangeException()
        };
        return targetFolder;
    }
}
