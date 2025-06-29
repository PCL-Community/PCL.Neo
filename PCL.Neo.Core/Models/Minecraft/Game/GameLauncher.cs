using PCL.Neo.Core.Models.Minecraft.Game.Data;
using PCL.Neo.Core.Models.Minecraft.Game.Data.Arguments;
using PCL.Neo.Core.Models.Minecraft.Game.Data.Arguments.Manifes;
using PCL.Neo.Core.Utils;
using PCL.Neo.Core.Utils.Logger;
using System.Diagnostics;

namespace PCL.Neo.Core.Models.Minecraft.Game;

public class LaunchOptions
{
    /// <summary>
    /// Minecraft版本ID
    /// </summary>
    public string VersionId { get; set; } = string.Empty;

    /// <summary>
    /// Minecraft根目录
    /// </summary>
    public string MinecraftRootDirectory { get; set; } = string.Empty;

    /// <summary>
    /// 游戏数据目录
    /// </summary>
    public string GameDirectory { get; set; } = string.Empty;

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
    /// 玩家用户名
    /// </summary>
    public string Username { get; set; } = "Player";

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
    public List<string> ExtraJvmArgs { get; set; } = new();

    /// <summary>
    /// 额外的游戏参数
    /// </summary>
    public List<string> ExtraGameArgs { get; set; } = new();

    /// <summary>
    /// 环境变量
    /// </summary>
    public Dictionary<string, string> EnvironmentVariables { get; set; } = new();

    /// <summary>
    /// 启动后关闭启动器
    /// </summary>
    public bool CloseAfterLaunch { get; set; } = false;

    /// <summary>
    /// 是否使用离线模式
    /// </summary>
    public bool IsOfflineMode { get; set; } = true;
}

public class GameLauncher
{
    private readonly GameService _gameService;
    private McLogFIleLogger _gameLogger;

    public GameLauncher(GameService gameService)
    {
        _gameService = gameService;
    }

    /// <summary>
    /// 启动游戏
    /// </summary>
    /// <exception cref="DirectoryNotFoundException">Throw if game directory not found.</exception>
    public async Task<Process> LaunchAsync(GameProfile profile)
    {
        string mcDir = profile.Information.RootDirectory;
        string gameDir = profile.Information.GameDirectory;

        // ensure directories exist
        if (Directory.Exists(mcDir) == false)
        {
            throw new DirectoryNotFoundException($"Minecraft root directory not found. {mcDir}");
        }

        if (Directory.Exists(gameDir) == false)
        {
            throw new DirectoryNotFoundException($"Minecraft game directory not found. {gameDir}");
        }


        var javaRuntime = profile.Options.RunnerJava;

        var versionInfo = await Versions.GetVersionByIdAsync(mcDir, profile.Options.VersionId)
                          ?? throw new Exception($"找不到版本 {profile.Options.VersionId}");

        if (!string.IsNullOrEmpty(versionInfo.InheritsFrom))
        {
            var parentInfo = await Versions.GetVersionByIdAsync(mcDir, versionInfo.InheritsFrom)
                             ?? throw new Exception($"找不到父版本 {versionInfo.InheritsFrom}");
            versionInfo = MergeVersionInfo(versionInfo, parentInfo);
        }

        var commandArgs = BuildLaunchCommand(profile, versionInfo);

#if DEBUG
        await File.WriteAllTextAsync(Path.Combine(gameDir, "launch_args.txt"), string.Join('\n', commandArgs));
#endif

        var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = javaRuntime.JavaExe,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = false,
                WorkingDirectory = gameDir
            }
        };

        // 使用 Add 方法来添加命令行参数，因为 ArgumentList 是只读的
        foreach (var arg in commandArgs)
        {
            process.StartInfo.ArgumentList.Add(arg);
        }

        foreach (var env in profile.Options.EnvironmentVariables)
        {
            process.StartInfo.EnvironmentVariables[env.Key] = env.Value;
        }

        process.Start();

        var logFilePath = Path.Combine(gameDir, "logs");
        var gameLoader = new McLogFileLogger(logFilePath, process);

        gameLoader.Start(); // Start the logger

        profile.Information.IsRunning = true;

        return process;
    }

    /// <summary>
    /// 合并版本信息（处理继承关系）
    /// </summary>
    private VersionInfo MergeVersionInfo(VersionInfo child, VersionInfo parent)
    {
        // 创建一个新的合并版本，保留子版本的ID和名称
        var merged = new VersionManifes
        {
            Id = child.Id,
            Name = child.Name,
            Type = child.Type,
            ReleaseTime = child.ReleaseTime,
            Time = child.Time,
            JsonData = child.JsonData
        };


        // 从父版本继承属性
        merged.MinecraftArguments = child.MinecraftArguments ?? parent.MinecraftArguments;
        merged.Arguments = child.Arguments ?? parent.Arguments;
        merged.MainClass = child.MainClass ?? parent.MainClass;
        merged.AssetIndex = child.AssetIndex ?? parent.AssetIndex;
        merged.Assets = child.Assets ?? parent.Assets;
        merged.JavaVersion = child.JavaVersion ?? parent.JavaVersion;


        // 合并下载信息
        merged.Downloads = child.Downloads ?? parent.Downloads;


        // 合并库文件（子版本优先）
        var libraries = new List<Library>();

        if (parent.Libraries != null)
                libraries.AddRange(parent.Libraries);

        if (child.Libraries != null)
        {
            foreach (var lib in child.Libraries)
            {
                // 检查是否已存在
                var exists = false;
                foreach (var existingLib in libraries)
                {
                    if (existingLib.Name == lib.Name)
                    {
                        exists = true;
                        break;
                    }
                }


                // 不存在则添加
                if (!exists)
                    libraries.Add(lib);
            }
        }

        merged.Libraries = libraries;


        return merged;
    }

    /// <summary>
    /// 构建游戏启动命令
    /// </summary>
    /// <exception cref="DirectoryNotFoundException">Throw if directory not found.</exception>
    private static ICollection<string> BuildLaunchCommand(
        GameProfile profile,
        VersionManifes versionManifes) // TODO: refactor this method
    {
        Collection<string> args =
        [
            $"-Xmx{profile.Options.MaxMemoryMB}M",
            $"-Xms{profile.Options.MinMemoryMB}M", // 标准JVM参数
            "-XX:+UseG1GC", // give up default arguments
            "-XX:+ParallelRefProcEnabled",
            "-XX:MaxGCPauseMillis=200",
            "-XX:+UnlockExperimentalVMOptions",
            "-XX:+DisableExplicitGC",
            "-XX:+AlwaysPreTouch",
            "-XX:MetaspaceSize=256m",
            "-XX:MaxMetaspaceSize=256m",
            "-XX:G1NewSizePercent=30",
            "-XX:G1MaxNewSizePercent=40",
            "-XX:G1HeapRegionSize=8M",
            "-XX:G1ReservePercent=20",
            "-XX:G1HeapWastePercent=5",
            "-XX:G1MixedGCCountTarget=4",
            "-XX:InitiatingHeapOccupancyPercent=15",
            "-XX:G1MixedGCLiveThresholdPercent=90",
            "-XX:G1RSetUpdatingPauseTimePercent=5",
            "-XX:SurvivorRatio=32",
            "-XX:+PerfDisableSharedMem",
            "-XX:MaxTenuringThreshold=1"
        ];

        // 设置natives路径
        string nativesDir = Path.Combine(profile.Information.RootDirectory, "versions", "natives");
        if (!Directory.Exists(nativesDir))
        {
            throw new DirectoryNotFoundException($"Native directory not found: {nativesDir}");
        }

        args.Add($"-Djava.library.path={DirectoryUtil.QuotePath(nativesDir)}"); // add natives path
        args.Add("-Dminecraft.launcher.brand=PCL.Neo"); // set launcher name
        args.Add($"-Dminecraft.launcher.version=1.0.0"); // TODO: load version from configuration

        // 类路径
        args.Add("-cp");
        ArgumentNullException.ThrowIfNull(versionManifes.Libraries); // ensure libraries is not null
        var libCommand = BuildLibrariesCommand(versionManifes.Libraries, profile.Information.RootDirectory);

        libCommand.Add(Path.Combine(profile.Information.GameDirectory, $"{profile.Options.VersionId}.jar"));
        var classPath = string.Join(SystemUtils.Os == SystemUtils.RunningOs.Windows ? ';' : ':',
            libCommand.Where(it => !string.IsNullOrEmpty(it)));
        profile.Information.ClassPath = classPath;

        // 客户端类型
        string clientType = profile.Options.IsOfflineMode ? "legacy" : "mojang";

        // 添加额外的JVM参数
        if (profile.Options.ExtraJvmArgs is { Count: > 0 })
        {
            args.AddRange(profile.Options.ExtraJvmArgs);
        }

        // 主类
        args.Add(versionManifes.MainClass);

        // 游戏参数
        if (!string.IsNullOrEmpty(versionManifes.MinecraftArguments)) // old version
        {
            // 旧版格式
            var argumetns = versionManifes.MinecraftArguments.Split(' ');
            var bestArgu = ArgumentProcessor.GetEffectiveArguments(argumetns, profile);

            args.AddRange(bestArgu);
        }
        else if (versionManifes.Arguments is not null) // new version
        {
            var gameBestArgu = ArgumentProcessor.GetEffectiveArguments(versionManifes.Arguments.Game, profile);
            var jvmBestArgu = ArgumentProcessor.GetEffectiveArguments(versionManifes.Arguments.Jvm, profile);

            args.AddRange(gameBestArgu);
            args.AddRange(jvmBestArgu);
        }
        else // default arguments
        {
            args.Add("--username");
            args.Add(profile.Options.Username);
            args.Add("--version");
            args.Add(profile.Options.VersionId);
            args.Add("--gameDir");
            args.Add(DirectoryUtil.QuotePath(profile.Information.GameDirectory));
            args.Add("--assetsDir");
            args.Add(DirectoryUtil.QuotePath(Path.Combine(profile.Information.RootDirectory, "assets")));
            args.Add("--assetIndex");
            args.Add(versionManifes.AssetIndex?.Id ?? "legacy");
            args.Add("--uuid");
            args.Add(profile.Options.UUID);
            args.Add("--accessToken");
            args.Add(profile.Options.AccessToken);
            args.Add("--userType");
            args.Add(clientType);
            args.Add("--versionType");
            args.Add(versionManifes.Type);
        }

        // 窗口大小
        if (profile.Options.FullScreen)
        {
            args.Add("--fullscreen");
        }

        // 添加额外的游戏参数
        if (profile.Options.ExtraGameArgs is { Count: > 0 })
        {
            args.AddRange(profile.Options.ExtraGameArgs);
        }

        // 拼接所有参数
        return args;
    }

    private static bool ShouldAddLibraryBasedOnRules(List<Rule>? rulesList, string currentOsName)
    {
        // "os属性可能不存在，这个时候默认直接添加" (Interpreted as: if rulesList is null or empty, default add)
        if (rulesList is null || rulesList.Count == 0)
        {
            return true;
        }

        bool permitAdd = true; // Default to allow, unless a rule explicitly disallows

        foreach (var rule in rulesList)
        {
            if (string.IsNullOrEmpty(rule.Action)) // Basic validation
            {
                continue; // Skip malformed rules
            }

            string action = rule.Action; // Ensure action comparison is case-insensitive
            string? ruleOsName = null;

            if (rule.Os != null && !string.IsNullOrEmpty(rule.Os.Name))
            {
                ruleOsName = rule.Os.Name; // Ensure OS name comparison is case-insensitive
            }

            if (action == "allow")
            {
                if (ruleOsName == null)
                {
                    // Rule: { "action": "allow" } (os attribute missing)
                    // "os属性可能不存在，这个时候默认直接添加" - this rule tends to allow.
                    // It doesn't change permitAdd from true to false.
                }
                else
                {
                    // Rule: { "action": "allow", "os": { "name": "specific_os" } }
                    // "如果action为allow，那就说明除os.name指定的系统外其余的全都不添加"
                    // This means: only allow if current_os == specific_os; otherwise, this rule causes disallow.
                    if (currentOsName != ruleOsName)
                    {
                        permitAdd = false;
                        break; // This rule explicitly states not to add for the current OS.
                    }
                }
            }
            else if (action == "disallow")
            {
                if (ruleOsName == null)
                {
                    // Rule: { "action": "disallow" } (os attribute missing)
                    // This means disallow for all systems.
                    permitAdd = false;
                    break; // This rule explicitly states not to add.
                }
                else
                {
                    // Rule: { "action": "disallow", "os": { "name": "specific_os" } }
                    // "如果action为disallow，那就说明除os.name指定的系统外其余的全都添加"
                    // This means: disallow only if current_os == specific_os; otherwise, this rule allows.
                    if (currentOsName == ruleOsName)
                    {
                        permitAdd = false;
                        break; // This rule explicitly states not to add for the current OS.
                    }
                }
            }
            // else: If action is not "allow" or "disallow", or action is missing, ignore the rule.
            //       You might want to add logging or error handling for unknown actions.
        }

        return permitAdd;
    }

    private static ICollection<string> BuildLibrariesCommand(IEnumerable<Library> lib, string micreaftDir)
    {
        ICollection<string> commands = [];

        var currentOs = Const.Os.ToString().ToLowerInvariant();
        var classifiersNatives = "natives" + currentOs;

        foreach (Library library in lib)
        {
            if (ShouldAddLibraryBasedOnRules(library.Rules, currentOs))
            {
                var realLibPath =
                    Path.Combine(micreaftDir, "libraries",
                        library.Downloads?.Artifact?.Path!); // TODO: handle fabric and other loads specific libraries
                commands.Add(realLibPath);
                if (library.Downloads?.Classifiers is not null)
                {
                    var classifier = library.Downloads.Classifiers
                        .FirstOrDefault(c => c.Key.Equals(classifiersNatives, StringComparison.OrdinalIgnoreCase));
                    if (classifier.Value is not null)
                    {
                        var realNaPath = Path.Combine(micreaftDir, "libraries", classifier.Value.Path);
                        commands.Add(realNaPath);
                    }
                }
            }
        }

        return commands;
    }

    private static ICollection<string> GenerateJvmArguments(GameProfile profile, VersionManifes versionManifes,
        ArgumentsAdapter adapter)
    {
        var args = new Collection<string>();

        // lo4j logger configuration file
        if (versionManifes.Logging is not null)
        {
            var loggingInfo = versionManifes.Logging.Client;
            var logPath = Path.Combine(profile.Information.GameDirectory, loggingInfo.File.Id);

            if (File.Exists(logPath))
            {
                args.Add($"-Dlog4j.configurationFile={DirectoryUtil.ForceQuotePath(logPath)}");
            }
        }

        // 添加额外的JVM参数
        if (profile.Options.ExtraJvmArgs is { Count: > 0 })
        {
            args.AddRange(profile.Options.ExtraJvmArgs);
        }

        if (versionManifes.Arguments is not null)
        {
            var jvmBestArgu =
                ArgumentProcessor.GetEffectiveArguments(versionManifes.Arguments.Jvm, adapter);

            args.AddRange(jvmBestArgu);
        }

        return args;
    }

    private static ICollection<string> GenerateGameArguments(GameProfile profile, VersionManifes versionManifes,
        ArgumentsAdapter adapter)
    {
        var args = new Collection<string>();

        if (!string.IsNullOrEmpty(versionManifes.MinecraftArguments)) // old version
        {
            // 旧版格式
            var argumetns = versionManifes.MinecraftArguments.Split(' ');
            var bestArgu = ArgumentProcessor.GetEffectiveArguments(argumetns, adapter);


            args.AddRange(bestArgu);
        }
        else if (versionManifes.Arguments is not null) // new version
        {
            var gameBestArgu =
                ArgumentProcessor.GetEffectiveArguments(versionManifes.Arguments.Game, adapter);

            args.AddRange(gameBestArgu);
        }
        else // default arguments
        {
            // 客户端类型
            var clientType = profile.Options.IsOfflineMode ? "legacy" : "mojang";

            args.Add("--username");
            args.Add(profile.Options.Username);
            args.Add("--version");
            args.Add(profile.Options.VersionId);
            args.Add("--gameDir");
            args.Add(DirectoryUtil.QuotePath(profile.Information.GameDirectory));
            args.Add("--assetsDir");
            args.Add(DirectoryUtil.QuotePath(Path.Combine(profile.Information.RootDirectory, "assets")));
            args.Add("--assetIndex");
            args.Add(versionManifes.AssetIndex?.Id ?? "legacy");
            args.Add("--uuid");
            args.Add(profile.Options.UUID);
            args.Add("--accessToken");
            args.Add(profile.Options.AccessToken);
            args.Add("--userType");
            args.Add(clientType);
            args.Add("--versionType");
            args.Add(versionManifes.Type);
        }

        // 窗口大小
        if (profile.Options.FullScreen)
        {
            args.Add("--fullscreen");
        }

        // 添加额外的游戏参数
        if (profile.Options.ExtraGameArgs is { Count: > 0 })
        {
            args.AddRange(profile.Options.ExtraGameArgs);
        }

        return args;
    }

    /// <summary>
    /// 构建游戏启动命令
    /// </summary>
    /// <exception cref="DirectoryNotFoundException">Throw if directory not found.</exception>
    private static ICollection<string> BuildLaunchCommand(
        GameProfile profile,
        VersionManifes versionManifes) // TODO: refactor this method
    {
        // set natives path and libPath
        var nativesDir = Path.Combine(
            profile.Information.RootDirectory,
            "versions",
            profile.Options.VersionId,
            "natives");

        var libPath = Path.Combine(profile.Information.RootDirectory, "libraries");

        if (!Directory.Exists(nativesDir)) // ensure natives directory exists
        {
            Directory.CreateDirectory(nativesDir);
        }


        var extraOptions = new Dictionary<string, string>
        {
            { "${natives_directory}", DirectoryUtil.ForceQuotePath(nativesDir) },
            { "${launcher_name}", "PCL.Neo" },
            { "${launcher_version}", "1.0.0" }, // TODO: load version from configuration
            { "${library_directory}", libPath }
        };

        // 类路径
        ArgumentNullException.ThrowIfNull(versionManifes.Libraries); // ensure libraries is not null
        var libCommand = BuildLibrariesCommand(versionManifes.Libraries, profile.Information.RootDirectory);

        // add version jar to classpath
        libCommand.Add(Path.Combine(profile.Information.GameDirectory, $"{profile.Options.VersionId}.jar"));
        var classPath = string.Join(SystemUtils.Os == SystemUtils.RunningOs.Windows ? ';' : ':',
            libCommand.Where(it => !string.IsNullOrEmpty(it)));

        // set class path
        profile.Information.ClassPath = classPath;

        var adapter = new ArgumentsAdapter(profile.Information, profile.Options, extraOptions);

        Collection<string> args =
        [
            $"-Xmx{profile.Options.MaxMemoryMB}M",
            $"-Xms{profile.Options.MinMemoryMB}M", // 标准JVM参数
            "-XX:+UseG1GC",
            "-XX:-UseAdaptiveSizePolicy"
        ];

        // jvm arguments
        var jvmArgs = GenerateJvmArguments(profile, versionManifes, adapter);
        args.AddRange(jvmArgs);

        // java wrapper
        // use temp java wrapper from pcl2
#warning "Replace JavaWrapper path with Neo's"
        var javaWrapperPath = @"C:\Users\WhiteCAT\Desktop\java_launch_wrapper-1.4.3.jar";

        args.Add("-jar");
        args.Add(javaWrapperPath); // TODO: replace with neo's path

        // 主类
        args.Add(versionManifes.MainClass);

        var gameArgs = GenerateGameArguments(profile, versionManifes, adapter);
        args.AddRange(gameArgs);

        return args;
    }
}