using PCL.Neo.Core.Models.Minecraft.Game;
using PCL.Neo.Core.Models.Minecraft.Game.Data;
using PCL.Neo.Core.Models.Minecraft.Game.Data.Arguments;
using PCL.Neo.Core.Models.Minecraft.Game.Data.Arguments.Manifes;
using PCL.Neo.Core.Utils;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Text;

namespace PCL.Neo.Core.Service.Game;

public class GameLauncherService : IGameLauncherService
{
    /// <inheritdoc />
    public VersionManifes? Manifes { get; private set; }

    /// <inheritdoc />
    public GameInfo Information { get; init; }

    /// <inheritdoc />
    public LaunchOptions Options { get; init; }


    private ArgumentsAdapter? _adapter;

    private readonly Lazy<Task<VersionManifes>> _manifestLazy;

    public GameLauncherService(GameInfo information, LaunchOptions options)
    {
        Information = information;
        Options = options;

        // 惰性加载版本清单
        _manifestLazy = new Lazy<Task<VersionManifes>>(LoadVersionManifestAsync);
    }

    /// <summary>
    /// 构建游戏启动命令
    /// </summary>
    public async Task<Collection<string>> BuildLaunchCommandAsync()
    {
        Manifes = await _manifestLazy.Value;

        var nativesDir = PrepareNativesDirectory();
        var libPath = Path.Combine(Information.RootDirectory, "libraries");

        ArgumentNullException.ThrowIfNull(Manifes.Libraries); // enure libraries is not null

        var libCommand = await BuildLibrariesCommandAsync(Manifes.Libraries, Information.RootDirectory);
        var classPath = BuildClassPath(libCommand);

        _adapter = CreateArgumentsAdapter(nativesDir, libPath, classPath);

        var args = new Collection<string>();

        // 添加基础JVM参数
        AddBasicJvmArguments(args);

        // 添加JVM参数
        var jvmArgs = GenerateJvmArguments(Information, Options, Manifes, _adapter);
        args.AddRange(jvmArgs);

        // 添加主类
        args.Add(Manifes.MainClass);

        // 添加游戏参数
        var gameArgs = GenerateGameArguments(Information, Options, Manifes, _adapter);
        args.AddRange(gameArgs);

        return args;
    }

    private async Task<VersionManifes> LoadVersionManifestAsync()
    {
        ValidateDirctories();

        var versionManifest =
            await Versions.GetVersionByIdAsync(Information.RootDirectory, Information.Name);

        if (versionManifest == null)
        {
            throw new InvalidOperationException($"Version manifest not found {Information.Name}");
        }

        if (!string.IsNullOrEmpty(versionManifest.InheritsFrom))
        {
            var parentManifest =
                await Versions.GetVersionByIdAsync(Information.RootDirectory, versionManifest.InheritsFrom);

            if (parentManifest == null)
            {
                throw new InvalidOperationException($"Version manifest not found {versionManifest.InheritsFrom}");
            }

            versionManifest = MergeVersionInfo(versionManifest, parentManifest);
        }

        Manifes = versionManifest;
        return versionManifest;
    }

    private void ValidateDirctories()
    {
        if (!Directory.Exists(Information.RootDirectory))
        {
            throw new DirectoryNotFoundException(
                $"Minecraft root directory not found: {Information.RootDirectory}");
        }

        if (!Directory.Exists(Information.GameDirectory))
        {
            throw new DirectoryNotFoundException(
                $"Minecraft game directory not found: {Information.GameDirectory}");
        }
    }

    /// <summary>
    /// 合并版本信息（处理继承关系）
    /// </summary>
    private static VersionManifes MergeVersionInfo(VersionManifes child, VersionManifes parent) => new()
    {
        Id = child.Id,
        Name = child.Name,
        Type = child.Type,
        ReleaseTime = child.ReleaseTime,
        Time = child.Time,
        JsonOriginData = child.JsonOriginData,
        MinecraftArguments = child.MinecraftArguments ?? parent.MinecraftArguments,
        Arguments = child.Arguments ?? parent.Arguments,
        MainClass = child.MainClass,
        AssetIndex = child.AssetIndex ?? parent.AssetIndex,
        Assets = child.Assets ?? parent.Assets,
        JavaVersion = child.JavaVersion ?? parent.JavaVersion,
        Downloads = child.Downloads ?? parent.Downloads,
        // 合并库文件
        Libraries = MergeLibraries(child.Libraries, parent.Libraries)
    };

    /// <summary>
    /// 合并库文件列表
    /// </summary>
    private static List<Library> MergeLibraries(List<Library>? childLibraries, List<Library>? parentLibraries)
    {
        var libraries = new List<Library>();

        if (parentLibraries != null)
            libraries.AddRange(parentLibraries);

        if (childLibraries != null)
        {
            var existingLibNames = new HashSet<string>(libraries.Select(lib => lib.Name));

            foreach (var lib in childLibraries)
            {
                if (!existingLibNames.Contains(lib.Name))
                    libraries.Add(lib);
            }
        }

        return libraries;
    }

    /// <summary>
    /// 启动游戏
    /// </summary>
    /// <exception cref="DirectoryNotFoundException">Throw if game directory not found.</exception>
    public async Task<Process> LaunchAsync(Collection<string> arguments)
    {
        var gameDir = Information.GameDirectory;
        var javaRuntime = Options.RunnerJava;

        await WriteDebugArgumentsAsync(gameDir, arguments);

        var process = CreateGameProcess(javaRuntime.JavaExe, gameDir, arguments);

        SetEnvironmentVariables(process);

        process.Start();
        Information.IsRunning = true;

        return process;
    }

    private static async Task WriteDebugArgumentsAsync(string gameDir, Collection<string> arguments)
    {
#if DEBUG
        var debugFile = Path.Combine(gameDir, "launch_args.txt");
        await File.WriteAllTextAsync(debugFile, string.Join('\n', arguments));
#endif
    }

    /// <summary>
    /// 创建游戏进程
    /// </summary>
    private static Process CreateGameProcess(string javaExe, string gameDir, Collection<string> arguments)
    {
        var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = javaExe,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true,
                WorkingDirectory = gameDir,
                StandardErrorEncoding = Encoding.UTF8,
                StandardOutputEncoding = Encoding.UTF8
            }
        };

        foreach (var arg in arguments)
        {
            process.StartInfo.ArgumentList.Add(arg);
        }

        return process;
    }

    /// <summary>
    /// 设置环境变量
    /// </summary>
    private void SetEnvironmentVariables(Process process)
    {
        foreach (var env in Options.EnvironmentVariables)
        {
            process.StartInfo.EnvironmentVariables[env.Key] = env.Value;
        }
    }


    private static bool ShouldAddLibraryBasedOnRules(List<Rule>? rulesList, string currentOsName)
    {
        // os属性可能不存在，这个时候默认直接添加
        if (rulesList is null || rulesList.Count == 0)
            return true;

        var permitAdd = true; // Default to allow, unless a rule explicitly disallows

        foreach (var rule in rulesList)
        {
            if (string.IsNullOrEmpty(rule.Action)) // Basic validation
                continue; // Skip malformed rules

            var action = rule.Action; // Ensure action comparison is case-insensitive
            string? ruleOsName = null;

            if (rule.Os != null && !string.IsNullOrEmpty(rule.Os.Name))
                ruleOsName = rule.Os.Name; // Ensure OS name comparison is case-insensitive

            if (string.Equals(action, "allow", StringComparison.OrdinalIgnoreCase))
            {
                if (ruleOsName == null)
                {
                    // os属性可能不存在，这个时候默认直接添加
                }
                else
                {
                    // 如果action为allow，那就说明除os.name指定的系统外其余的全都不添加
                    if (string.Equals(currentOsName, ruleOsName, StringComparison.OrdinalIgnoreCase))
                    {
                        permitAdd = false;
                        break;
                    }
                }
            }
            else if (string.Equals(action, "disallow", StringComparison.OrdinalIgnoreCase))
            {
                if (ruleOsName == null)
                {
                    // This means disallow for all systems.
                    permitAdd = false;
                    break; // This rule explicitly states not to add.
                }
                else
                {
                    // 如果action为disallow，那就说明除os.name指定的系统外其余的全都添加
                    if (string.Equals(currentOsName, ruleOsName, StringComparison.OrdinalIgnoreCase))
                    {
                        permitAdd = false;
                        break; // This rule explicitly states not to add for the current OS.
                    }
                }
            }
        }

        return permitAdd;
    }

    /// <summary>
    /// 获取库文件路径
    /// </summary>
    private static string GetLibraryPath(Library library, string rootDir)
    {
        if (library.Downloads?.Artifact?.Path != null)
            return Path.Combine(rootDir, "libraries", library.Downloads.Artifact.Path);

        var nameSpilited = library.Name.Split(':');

        if (nameSpilited.Length != 3)
            throw new ArgumentException($"Invalid library name format: {library.Name}");

        var prePath = nameSpilited[0].Replace('.', Path.DirectorySeparatorChar);
        var finalPath = Path.Combine(prePath, nameSpilited[1], nameSpilited[2]);
        var fileName = $"{nameSpilited[1]}-{nameSpilited[2]}.jar";

        return Path.Combine(rootDir, "libraries", finalPath, fileName);
    }

    /// <summary>
    /// 构建库文件命令
    /// </summary>
    private static Task<Collection<string>> BuildLibrariesCommandAsync(
        IEnumerable<Library> libraries,
        string minecraftDir) =>
        Task.Run(() =>
        {
            var commands = new Collection<string>();
            var currentOs = Const.Os.ToString().ToLowerInvariant();
            var classifiersNatives = "natives" + currentOs;

            foreach (var library in libraries)
            {
                if (!ShouldAddLibraryBasedOnRules(library.Rules, currentOs))
                    continue;

                var realLibPath = GetLibraryPath(library, minecraftDir);
                commands.Add(realLibPath);

                // 处理原生库
                if (library.Downloads?.Classifiers != null)
                {
                    var classifier = library.Downloads.Classifiers
                        .FirstOrDefault(c => c.Key.Equals(classifiersNatives, StringComparison.OrdinalIgnoreCase));

                    if (classifier.Value is not null)
                    {
                        var realNaPath = Path.Combine(minecraftDir, "libraries", classifier.Value.Path);
                        commands.Add(realNaPath);
                    }
                }
            }

            return commands;
        });

    /// <summary>
    /// 准备natives目录
    /// </summary>
    private string PrepareNativesDirectory()
    {
        var nativesDir = Path.Combine(
            Information.RootDirectory,
            "versions",
            Information.Name,
            "natives");

        if (!Directory.Exists(nativesDir))
            Directory.CreateDirectory(nativesDir);

        return nativesDir;
    }

    /// <summary>
    /// 构建类路径
    /// </summary>
    private string BuildClassPath(Collection<string> libCommand)
    {
        libCommand.Add(Path.Combine(Information.GameDirectory, $"{Information.Name}.jar"));

        var separator = SystemUtils.Os == SystemUtils.RunningOs.Windows ? ';' : ':';
        return string.Join(separator, libCommand.Where(path => !string.IsNullOrEmpty(path)));
    }

    /// <summary>
    /// 创建参数适配器
    /// </summary>
    private ArgumentsAdapter CreateArgumentsAdapter(string nativesDir, string libPath, string classPath)
    {
        var extraArgs = new Dictionary<string, string>
        {
            { "${natives_directory}", DirectoryUtil.ForceQuotePath(nativesDir) },
            { "${launcher_name}", "PCL.Neo" },
            { "${launcher_version}", "1.0.0" },
            { "${library_directory}", libPath },
            { "${classpath}", classPath }
        };

        ArgumentNullException.ThrowIfNull(Manifes);

        return new ArgumentsAdapter(Information, Options, extraArgs, Manifes);
    }

    /// <summary>
    /// 添加基础JVM参数
    /// </summary>
    private void AddBasicJvmArguments(Collection<string> args)
    {
        args.Add($"-Xmx{Options.MaxMemoryMB}M");
        args.Add($"-Xms{Options.MinMemoryMB}M");
        args.Add("-XX:+UseG1GC");
        args.Add("-XX:-UseAdaptiveSizePolicy");
    }

    /// <summary>
    /// 生成JVM参数
    /// </summary>
    private static Collection<string> GenerateJvmArguments(
        GameInfo information,
        LaunchOptions options,
        VersionManifes versionManifest,
        ArgumentsAdapter adapter)
    {
        var args = new Collection<string>();

        // Log4j配置
        AddLoggingConfiguration(args, information, options, versionManifest);

        // 额外的JVM参数
        if (options.ExtraJvmArgs?.Count > 0)
            args.AddRange(options.ExtraJvmArgs);

        // 版本特定的JVM参数
        if (versionManifest.Arguments?.Jvm is not null)
        {
            var jvmArgs = ArgumentProcessor.GetEffectiveArguments(versionManifest.Arguments.Jvm, adapter);
            args.AddRange(jvmArgs);
        }

        return args;
    }

    /// <summary>
    /// 添加日志配置
    /// </summary>
    private static void AddLoggingConfiguration(
        Collection<string> args,
        GameInfo information,
        LaunchOptions options,
        VersionManifes versionManifest)
    {
        if (versionManifest.Logging?.Client is null)
        {
            return;
        }

        var loggingInfo = versionManifest.Logging.Client;
        var logPath = Path.Combine(information.GameDirectory, loggingInfo.File.Id);

        if (File.Exists(logPath))
            args.Add($"-Dlog4j.configurationFile={DirectoryUtil.ForceQuotePath(logPath)}");
    }

    /// <summary>
    /// 生成游戏参数
    /// </summary>
    private static Collection<string> GenerateGameArguments(
        GameInfo information,
        LaunchOptions options,
        VersionManifes versionManifest,
        ArgumentsAdapter adapter)
    {
        var args = new Collection<string>();

        if (versionManifest.Arguments?.Game is not null)
        {
            // 新版本格式
            var gameArgs = ArgumentProcessor.GetEffectiveArguments(versionManifest.Arguments.Game, adapter);
            args.AddRange(gameArgs);
        }
        else if (!string.IsNullOrEmpty(versionManifest.MinecraftArguments))
        {
            // 旧版本格式
            var arguments = versionManifest.MinecraftArguments.Split(' ');
            var processedArgs = ArgumentProcessor.GetEffectiveArguments(arguments, adapter);
            args.AddRange(processedArgs);
        }
        else
        {
            // 默认参数
            AddDefaultGameArguments(args, information, options, versionManifest);
        }

        // 全屏模式
        if (options.FullScreen)
            args.Add("--fullscreen");

        // 额外的游戏参数
        if (options.ExtraGameArgs?.Count > 0)
            args.AddRange(options.ExtraGameArgs);

        return args;
    }

    /// <summary>
    /// 添加默认游戏参数
    /// </summary>
    private static void AddDefaultGameArguments(
        Collection<string> args,
        GameInfo information,
        LaunchOptions options,
        VersionManifes versionManifest)
    {
        var clientType = options.IsOfflineMode ? "legacy" : "mojang";

        args.AddRange([
            "--username", options.Username,
            "--version", information.Name,
            "--gameDir", DirectoryUtil.QuotePath(information.GameDirectory),
            "--assetsDir", DirectoryUtil.QuotePath(Path.Combine(information.RootDirectory, "assets")),
            "--assetIndex", versionManifest.AssetIndex?.Id ?? "legacy",
            "--uuid", options.UUID,
            "--accessToken", options.AccessToken,
            "--userType", clientType,
            "--versionType", versionManifest.Type
        ]);
    }
}
