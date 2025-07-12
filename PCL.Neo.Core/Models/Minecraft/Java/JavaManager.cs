using PCL.Neo.Core.Models.Minecraft.Game.Data.Arguments.Manifes;
using PCL.Neo.Core.Utils;

namespace PCL.Neo.Core.Models.Minecraft.Java;

using DefaultJavaRuntimeCombine = (JavaRuntime? Java8, JavaRuntime? Java17, JavaRuntime? Java21);

/// <summary>
/// Java管理器
/// </summary>
public sealed partial class JavaManager : IJavaManager
{
    public const int JavaListCacheVersion = 0; // [INFO] Java 缓存版本号，大版本更新后应该增加
    public bool IsInitialized { get; private set; }
    private bool _isBusy;

    public List<JavaRuntime> JavaList { get; private set; } = [];

    /// <summary>
    /// Java验证结果字典
    /// </summary>
    private readonly Dictionary<string, JavaVerifier.JavaVerifyResult> _javaVerifyResults = new();

    private DefaultJavaRuntimeCombine? _defaultJavaRuntimes;

    public DefaultJavaRuntimeCombine DefaultJavaRuntimes
    {
        get
        {
            if (_defaultJavaRuntimes != null)
                return (_defaultJavaRuntimes?.Java8, _defaultJavaRuntimes?.Java17, _defaultJavaRuntimes?.Java21);
            DefaultJavaRuntimeCombine runtimes = new();
            var (minDiff8, minDiff17, minDiff21) = (int.MaxValue, int.MaxValue, int.MaxValue);
            int diff;
            JavaList.ForEach(runtime =>
            {
                switch (runtime.SlugVersion)
                {
                    case >= 8 and < 17:
                        diff = runtime.SlugVersion - 8;
                        if (diff < minDiff8) (minDiff8, runtimes.Java8) = (diff, runtime);
                        break;
                    case >= 17 and < 21:
                        diff = runtime.SlugVersion - 17;
                        if (diff < minDiff17) (minDiff17, runtimes.Java17) = (diff, runtime);
                        break;
                    case >= 21:
                        diff = runtime.SlugVersion - 21;
                        if (diff < minDiff21) (minDiff21, runtimes.Java21) = (diff, runtime);
                        break;
                }
            });
            _defaultJavaRuntimes = runtimes;
            return (_defaultJavaRuntimes?.Java8, _defaultJavaRuntimes?.Java17, _defaultJavaRuntimes?.Java21);
        }
    }

    /// <summary>
    /// 获取适合游戏版本的Java
    /// </summary>
    /// <param name="version">游戏信息清单</param>
    /// <returns>排序后的Java兼容性列表</returns>
    public List<JavaSelector.JavaCompatibilityScore> GetCompatibleJavas(VersionManifest version)
    {
        if (!IsInitialized || JavaList.Count == 0)
        {
            return [];
        }

        return JavaSelector.SelectJavaForGame(version, JavaList);
    }

    /// <summary>
    /// 获取最适合游戏版本的Java
    /// </summary>
    /// <param name="version">游戏信息清单</param>
    /// <returns>最佳的Java或null</returns>
    public JavaRuntime? GetBestJavaForGame(VersionManifest version)
    {
        if (!IsInitialized || JavaList.Count == 0)
        {
            return null;
        }

        var compatibleJavas = JavaSelector.SelectJavaForGame(version, JavaList);
        return compatibleJavas.FirstOrDefault()?.Runtime;
    }

    /// <summary>
    /// 获取Java的验证结果
    /// </summary>
    /// <param name="java">要验证的Java</param>
    /// <returns>验证结果</returns>
    public async Task<JavaVerifier.JavaVerifyResult> GetJavaVerificationAsync(JavaRuntime java)
    {
        if (_javaVerifyResults.TryGetValue(java.DirectoryPath, out var result))
        {
            return result;
        }

        var verifyResult = await JavaVerifier.VerifyJavaAsync(java.JavaExe);
        _javaVerifyResults[java.DirectoryPath] = verifyResult;
        return verifyResult;
    }

    /// <summary>
    /// 清除验证结果缓存
    /// </summary>
    public void ClearVerificationCache()
    {
        _javaVerifyResults.Clear();
    }

    /// <summary>
    /// 初始化 Java 列表，但除非没有 Java，否则不进行检查。
    /// <remarks>更换为 Logger.cs 中的 logger </remarks>
    /// </summary>
    public async Task JavaListInitAsync()
    {
        if (IsInitialized || _isBusy) return;
        _isBusy = true;
        JavaList = [];
        try
        {
            // TODO: 如果本地缓存中已有 Java 列表则读取缓存
            var readJavaListCacheVersion = 0; // TODO: 此数字应该从缓存中读取
            if (readJavaListCacheVersion < JavaListCacheVersion)
            {
                // TODO: 设置本地版本号为 JavaListCacheVersion
                Console.WriteLine("[Java] 要求 Java 列表缓存更新");
            }

            // TODO: 从本地缓存中读取 Java 列表
            if (JavaList.Count == 0)
            {
                Console.WriteLine("[Java] 初始化未找到可用的 Java，将自动触发搜索");
                JavaList = (await SearchJavaAsync()).ToList();
                Console.Write("[Java] 搜索完成 ");

                // 验证找到的Java
                await VerifyAllJavaRuntimes();
            }
            else
            {
                Console.WriteLine($"[Java] 缓存中有{JavaList.Count}个可用的 Java：");
            }

            IsInitialized = true;
            _isBusy = false;
            TestOutput();
        }
        catch (Exception)
        {
            Console.WriteLine("初始化 Java 失败");
            IsInitialized = false;
            _isBusy = false;
            throw;
        }
    }

    public async Task<(JavaRuntime?, bool UpdateCurrent)> ManualAdd(string javaDir)
    {
        if (_isBusy || !IsInitialized) return (null, false);
        if (JavaList.FirstOrDefault(runtime => runtime.DirectoryPath == javaDir) is {} existingRuntime)
        {
            Console.WriteLine("选择的 Java 在列表中已存在，将其标记为手动导入。");
            existingRuntime.IsUserImport = true;
            return (existingRuntime, true);
        }

        var entity = await JavaRuntime.CreateJavaEntityAsync(javaDir, true);
        if (entity is { Compability: not JavaCompability.Error })
        {
            JavaList.Add(entity);
            Console.WriteLine("已成功添加！");
            _defaultJavaRuntimes = null;

            // 验证新添加的Java
            await GetJavaVerificationAsync(entity);

            return (entity, false);
        }

        Console.WriteLine("添加的 Java 文件无法运行！");
        return (null, false);
    }

    public async Task Refresh()
    {
        if (_isBusy || !IsInitialized) return;
        _isBusy = true;
        Console.WriteLine("[Java] 正在刷新 Java");
        List<JavaRuntime> newEntities = [];
        // 对于用户手动导入的 Java，保留并重新检查可用性
        var oldManualEntities = JavaList.FindAll(entity => entity.IsUserImport);
        JavaList.Clear();
        var searchedEntities = (await SearchJavaAsync()).ToList();
        newEntities.AddRange(searchedEntities);
        foreach (var oldRuntime in oldManualEntities.Where(entity =>
                     searchedEntities.All(javaEntity => javaEntity.DirectoryPath != entity.DirectoryPath)))
        {
            if (await oldRuntime.RefreshInfoAsync())
                newEntities.Add(oldRuntime);
            else
                Console.WriteLine($"[Java] 用户导入的 Java 已不可用，已自动剔除：{oldRuntime.DirectoryPath}");
        }

        JavaList = newEntities;
        Console.WriteLine($"[Java] 刷新 Java 完成，现在共有 {JavaList.Count} 个Java");

        // 清除验证结果缓存
        ClearVerificationCache();

        // 重新验证所有Java
        await VerifyAllJavaRuntimes();

        if (JavaList.Count == 0)
        {
            // TODO: 提示用户未找到已安装的 java，是否自动下载合适版本，然后再下载
            var neo2SysDir = Directory.CreateDirectory(
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                    "PCL.Neo", "Java")); // TODO: 此处的路径等配置文件的模块写好了以后应该从配置文件中获取
            var cts = new CancellationTokenSource();
            var progress =
                new Progress<(int, int)>(value =>
                    Console.WriteLine(
                        $"下载进度：已下载{value.Item1}/总文件数{value.Item2}")); // TODO: 后续这个 progress 可以设置成在 UI 上显示
            var fetchedJavaDir = await FetchJavaOnline(SystemUtils.Platform, neo2SysDir.FullName,
                MojangJavaVersion.Α, progress, cts.Token);
            if (fetchedJavaDir != null)
            {
                var runtime = await JavaRuntime.CreateJavaEntityAsync(fetchedJavaDir, true);
                JavaList.Add(runtime!);

                // 验证下载的Java
                await GetJavaVerificationAsync(runtime!);
            }
        }

        _defaultJavaRuntimes = null;
        _isBusy = false;
        TestOutput();
    }

    /// <summary>
    /// 验证所有的Java运行时
    /// </summary>
    private async Task VerifyAllJavaRuntimes()
    {
        if (JavaList.Count == 0) return;

        Console.WriteLine("[Java] 正在验证已发现的Java...");

        // 只验证前5个Java，避免过多验证操作
        var javasToVerify = JavaList.Take(5);
        var tasks = javasToVerify.Select(async java =>
        {
            try
            {
                var result = await JavaVerifier.VerifyJavaAsync(java.JavaExe);
                _javaVerifyResults[java.DirectoryPath] = result;

                if (!result.IsGenuine)
                {
                    Console.WriteLine($"[Java] 警告: 位于 {java.DirectoryPath} 的Java可能不是正版: {result.FailReason}");
                }
                else
                {
                    Console.WriteLine(
                        $"[Java] 验证通过: {java.DirectoryPath} - {JavaVerifier.GetVendorFriendlyName(result.Vendor)}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Java] 验证失败: {java.DirectoryPath} - {ex.Message}");
            }
        });

        await Task.WhenAll(tasks);
        Console.WriteLine("[Java] Java验证完成");
    }

    private static async Task<IEnumerable<JavaRuntime>> SearchJavaAsync()
    {
        return SystemUtils.Os switch
        {
            SystemUtils.RunningOs.Windows => await Windows.SearchJavaAsync(),
            SystemUtils.RunningOs.Linux or SystemUtils.RunningOs.MacOs => await Unix.SearchJavaAsync(SystemUtils.Os),
            _ => throw new PlatformNotSupportedException()
        };
    }

    private void TestOutput()
    {
        if (!IsInitialized) return;
        Console.WriteLine("当前有 " + JavaList.Count + " 个 Java");
        foreach (var javaEntity in JavaList)
        {
            Console.WriteLine("--------------------");
            Console.WriteLine("路径: " + javaEntity.DirectoryPath);
            Console.WriteLine((string?)("是否兼容: " + javaEntity.Compability));
            Console.WriteLine((string?)("架构：" + javaEntity.Architecture));
            Console.WriteLine("发行商：" + javaEntity.Implementor);
            Console.WriteLine("版本：" + javaEntity.Version);
            Console.WriteLine("数字版本：" + javaEntity.SlugVersion);

            // 如果有验证结果，输出验证信息
            if (_javaVerifyResults.TryGetValue(javaEntity.DirectoryPath, out var verifyResult))
            {
                Console.WriteLine($"验证结果：{(verifyResult.IsGenuine ? "正版" : "可疑")}");
                Console.WriteLine($"厂商：{JavaVerifier.GetVendorFriendlyName(verifyResult.Vendor)}");
            }
        }
    }
}
