using PCL.Neo.Core.Models.Minecraft.Game.Data;
using PCL.Neo.Core.Models.Minecraft.Game.Data.Arguments;
using PCL.Neo.Core.Models.Minecraft.Game.Data.Arguments.Manifes;
using PCL.Neo.Core.Utils;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Text;
using System.Text;

namespace PCL.Neo.Core.Models.Minecraft.Game;

public class GameLauncher : IGameLauncher
{
    /// <inheritdoc />
    public VersionManifes? Manifes { get; private set; }

    /// <inheritdoc />
    public GameProfile Profile { get; init; }

    private ArgumentsAdapter? _adapter;

    private readonly Lazy<Task<VersionManifes>> _manifestLazy;

    public GameLauncher(GameProfile profile)
    {
        Profile = profile ?? throw new ArgumentNullException(nameof(profile));

        // 惰性加载版本清单
        _manifestLazy = new Lazy<Task<VersionManifes>>(LoadVersionManifestAsync);
    }

    /// <inheritdoc />
    public VersionManifes? Manifes { get; private set; }

    /// <inheritdoc />
    public GameProfile Profile { get; init; }

    private ArgumentsAdapter? _adapter;

    private readonly Lazy<Task<VersionManifes>> _manifestLazy;

    public GameLauncher(GameProfile profile)
    {
        Profile = profile ?? throw new ArgumentNullException(nameof(profile));

        // 惰性加载版本清单
        _manifestLazy = new Lazy<Task<VersionManifes>>(LoadVersionManifestAsync);
    }

    /// <summary>
    /// 构建游戏启动命令
    /// 构建游戏启动命令
    /// </summary>
    public async Task<Collection<string>> BuildLaunchCommandAsync()
    {
        var manifest = await _manifestLazy.Value;

        var nativesDir = PrepareNativesDirectory();
        var libPath = Path.Combine(Profile.Information.RootDirectory, "libraries");

        ArgumentNullException.ThrowIfNull(manifest.Libraries); // enure libraries is not null

        var libCommand = await BuildLibrariesCommandAsync(manifest.Libraries, Profile.Information.RootDirectory);
        var classPath = BuildClassPath(libCommand);

        _adapter = CreateArgumentsAdapter(nativesDir, libPath, classPath);

        var args = new Collection<string>();

        // 添加基础JVM参数
        AddBasicJvmArguments(args);

        // 添加JVM参数
        var jvmArgs = GenerateJvmArguments(Profile, manifest, _adapter);
        args.AddRange(jvmArgs);

        // 添加Java包装器
        AddJavaWrapper(args);

        // 添加主类
        args.Add(manifest.MainClass);

        // 添加游戏参数
        var gameArgs = GenerateGameArguments(Profile, manifest, _adapter);
        args.AddRange(gameArgs);

        return args;
    }

    private async Task<VersionManifes> LoadVersionManifestAsync()
    {
        ValidateDirctories();

        var versionManifest =
            await Versions.GetVersionByIdAsync(Profile.Information.RootDirectory, Profile.Options.VersionId);

        if (versionManifest == null)
        {
            throw new InvalidOperationException($"Version manifest not found {Profile.Options.VersionId}");
        }

        if (!string.IsNullOrEmpty(versionManifest.InheritsFrom))
        {
            var parentManifest =
                await Versions.GetVersionByIdAsync(Profile.Information.RootDirectory, versionManifest.InheritsFrom);

            if (parentManifest == null)
            {
                throw new InvalidOperationException($"Version manifest not found {versionManifest.InheritsFrom}");
            }

            versionManifest = MergeVersionInfo(versionManifest, parentManifest);
        }
    }

    private void ValidateDirctories()
    {
        if (!Directory.Exists(Profile.Information.RootDirectory))
        {
            throw new DirectoryNotFoundException(
                $"Minecraft root directory not found: {Profile.Information.RootDirectory}");
            throw new DirectoryNotFoundException(
                $"Minecraft root directory not found: {Profile.Information.RootDirectory}");
        }

        if (!Directory.Exists(Profile.Information.GameDirectory))
        if (!Directory.Exists(Profile.Information.GameDirectory))
        {
            throw new DirectoryNotFoundException(
                $"Minecraft game directory not found: {Profile.Information.GameDirectory}");
            throw new DirectoryNotFoundException(
                $"Minecraft game directory not found: {Profile.Information.GameDirectory}");
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
        {
            libraries.AddRange(parentLibraries);
        }

        if (childLibraries != null)
        if (parentLibraries != null)
        {
            libraries.AddRange(parentLibraries);
        }

        if (childLibraries != null)
        {
            var existingLibNames = new HashSet<string>(libraries.Select(lib => lib.Name));

            foreach (var lib in childLibraries)
            {
                if (!existingLibNames.Contains(lib.Name))
                {
                    libraries.Add(lib);
                }
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
        var gameDir = Profile.Information.GameDirectory;
        var javaRuntime = Profile.Options.RunnerJava;

        await WriteDebugArgumentsAsync(gameDir, arguments);

        var process = CreateGameProcess(javaRuntime.JavaExe, gameDir, arguments);

        SetEnvironmentVariables(process);

        process.Start();
        Profile.Information.IsRunning = true;

        return process;
    }

    private static async Task WriteDebugArgumentsAsync(string gameDir, Collection<string> arguments)
    {
            var existingLibNames = new HashSet<string>(libraries.Select(lib => lib.Name));

            foreach (var lib in childLibraries)
            {
                if (!existingLibNames.Contains(lib.Name))
                {
                    libraries.Add(lib);
                }
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
        var gameDir = Profile.Information.GameDirectory;
        var javaRuntime = Profile.Options.RunnerJava;

        await WriteDebugArgumentsAsync(gameDir, arguments);

        var process = CreateGameProcess(javaRuntime.JavaExe, gameDir, arguments);

        SetEnvironmentVariables(process);

        process.Start();
        Profile.Information.IsRunning = true;

        return process;
    }

    private static async Task WriteDebugArgumentsAsync(string gameDir, Collection<string> arguments)
    {
#if DEBUG
        var debugFile = Path.Combine(gameDir, "launch_args.txt");
        await File.WriteAllTextAsync(debugFile, string.Join('\n', arguments));
#endif
    }
    }

    /// <summary>
    /// 创建游戏进程
    /// </summary>
    private static Process CreateGameProcess(string javaExe, string gameDir, Collection<string> arguments)
    {
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
                FileName = javaExe,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true,
                WorkingDirectory = gameDir,
                StandardErrorEncoding = Encoding.UTF8,
                StandardOutputEncoding = Encoding.UTF8
                CreateNoWindow = true,
                WorkingDirectory = gameDir,
                StandardErrorEncoding = Encoding.UTF8,
                StandardOutputEncoding = Encoding.UTF8
            }
        };

        foreach (var arg in arguments)
        foreach (var arg in arguments)
        {
            process.StartInfo.ArgumentList.Add(arg);
        }

        return process;
    }

    /// <summary>
    /// 设置环境变量
    /// 设置环境变量
    /// </summary>
    private void SetEnvironmentVariables(Process process)
    private void SetEnvironmentVariables(Process process)
    {
        foreach (var env in Profile.Options.EnvironmentVariables)
        {
            process.StartInfo.EnvironmentVariables[env.Key] = env.Value;
        }
    }

        foreach (var env in Profile.Options.EnvironmentVariables)
        {
            process.StartInfo.EnvironmentVariables[env.Key] = env.Value;
        }
    }


    private static bool ShouldAddLibraryBasedOnRules(List<Rule>? rulesList, string currentOsName)
    {
        // os属性可能不存在，这个时候默认直接添加
        // os属性可能不存在，这个时候默认直接添加
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
                    // os属性可能不存在，这个时候默认直接添加
                }
                else
                {
                    // 如果action为allow，那就说明除os.name指定的系统外其余的全都不添加
                    if (currentOsName != ruleOsName)
                    {
                        permitAdd = false;
                        break;
                        break;
                    }
                }
            }
            else if (action == "disallow")
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
                    if (currentOsName == ruleOsName)
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
        {
            return Path.Combine(rootDir, "libraries", library.Downloads.Artifact.Path);
        }

        var nameSpilited = library.Name.Split(':');

        if (nameSpilited.Length != 3)
        {
            throw new ArgumentException($"Invalid library name format: {library.Name}");
        }

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
    /// <summary>
    /// 获取库文件路径
    /// </summary>
    private static string GetLibraryPath(Library library, string rootDir)
    {
        if (library.Downloads?.Artifact?.Path != null)
        {
            return Path.Combine(rootDir, "libraries", library.Downloads.Artifact.Path);
        }

        var nameSpilited = library.Name.Split(':');

        if (nameSpilited.Length != 3)
        {
            throw new ArgumentException($"Invalid library name format: {library.Name}");
        }

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
                {
                    continue;
                }

                var realLibPath = GetLibraryPath(library, minecraftDir);
            foreach (var library in libraries)
            {
                if (!ShouldAddLibraryBasedOnRules(library.Rules, currentOs))
                {
                    continue;
                }

                var realLibPath = GetLibraryPath(library, minecraftDir);
                commands.Add(realLibPath);

                // 处理原生库
                if (library.Downloads?.Classifiers != null)

                // 处理原生库
                if (library.Downloads?.Classifiers != null)
                {
                    var classifier = library.Downloads.Classifiers
                        .FirstOrDefault(c => c.Key.Equals(classifiersNatives, StringComparison.OrdinalIgnoreCase));

                    if (classifier.Value != null)

                    if (classifier.Value != null)
                    {
                        var realNaPath = Path.Combine(minecraftDir, "libraries", classifier.Value.Path);
                        commands.Add(realNaPath);
                    }
                }
            }

            return commands;
        });

            return commands;
        });

    /// <summary>
    /// 准备natives目录
    /// </summary>
    private string PrepareNativesDirectory()
    {
        var nativesDir = Path.Combine(
            Profile.Information.RootDirectory,
            "versions",
            Profile.Options.VersionId,
            "natives");

        if (!Directory.Exists(nativesDir))
        {
            Directory.CreateDirectory(nativesDir);
        }

        return nativesDir;
    }

    /// <summary>
    /// 构建类路径
    /// </summary>
    private string BuildClassPath(Collection<string> libCommand)
    {
        libCommand.Add(Path.Combine(Profile.Information.GameDirectory, $"{Profile.Options.VersionId}.jar"));

        var separator = SystemUtils.Os == SystemUtils.RunningOs.Windows ? ';' : ':';
        return string.Join(separator, libCommand.Where(path => !string.IsNullOrEmpty(path)));
    /// <summary>
    /// 准备natives目录
    /// </summary>
    private string PrepareNativesDirectory()
    {
        var nativesDir = Path.Combine(
            Profile.Information.RootDirectory,
            "versions",
            Profile.Options.VersionId,
            "natives");

        if (!Directory.Exists(nativesDir))
        {
            Directory.CreateDirectory(nativesDir);
        }

        return nativesDir;
    }

    /// <summary>
    /// 构建类路径
    /// </summary>
    private string BuildClassPath(Collection<string> libCommand)
    {
        libCommand.Add(Path.Combine(Profile.Information.GameDirectory, $"{Profile.Options.VersionId}.jar"));

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

        return new ArgumentsAdapter(Profile.Information, Profile.Options, extraArgs);
    }

    /// <summary>
    /// 添加基础JVM参数
    /// </summary>
    private void AddBasicJvmArguments(Collection<string> args)
    {
        args.Add($"-Xmx{Profile.Options.MaxMemoryMB}M");
        args.Add($"-Xms{Profile.Options.MinMemoryMB}M");
        args.Add("-XX:+UseG1GC");
        args.Add("-XX:-UseAdaptiveSizePolicy");
    }

    /// <summary>
    /// 添加Java包装器
    /// </summary>
    private static void AddJavaWrapper(Collection<string> args)
    {
#warning "Replace JavaWrapper path with Neo's"
        const string javaWrapperPath = @"C:\Users\WhiteCAT\Desktop\java_launch_wrapper-1.4.3.jar";

        args.Add("-jar");
        args.Add(javaWrapperPath);
    }

    /// <summary>
    /// 生成JVM参数
    /// </summary>
    private static Collection<string> GenerateJvmArguments(
        GameProfile profile,
        VersionManifes versionManifest,
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

        return new ArgumentsAdapter(Profile.Information, Profile.Options, extraArgs);
    }

    /// <summary>
    /// 添加基础JVM参数
    /// </summary>
    private void AddBasicJvmArguments(Collection<string> args)
    {
        args.Add($"-Xmx{Profile.Options.MaxMemoryMB}M");
        args.Add($"-Xms{Profile.Options.MinMemoryMB}M");
        args.Add("-XX:+UseG1GC");
        args.Add("-XX:-UseAdaptiveSizePolicy");
    }

    /// <summary>
    /// 添加Java包装器
    /// </summary>
    private static void AddJavaWrapper(Collection<string> args)
    {
#warning "Replace JavaWrapper path with Neo's"
        const string javaWrapperPath = @"C:\Users\WhiteCAT\Desktop\java_launch_wrapper-1.4.3.jar";

        args.Add("-jar");
        args.Add(javaWrapperPath);
    }

    /// <summary>
    /// 生成JVM参数
    /// </summary>
    private static Collection<string> GenerateJvmArguments(
        GameProfile profile,
        VersionManifes versionManifest,
        ArgumentsAdapter adapter)
    {
        var args = new Collection<string>();

        // Log4j配置
        AddLoggingConfiguration(args, profile, versionManifest);

        // 额外的JVM参数
        if (profile.Options.ExtraJvmArgs?.Count > 0)
        {
            args.AddRange(profile.Options.ExtraJvmArgs);
        }

        // 版本特定的JVM参数
        if (versionManifest.Arguments?.Jvm != null)
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
        GameProfile profile,
        VersionManifes versionManifest)
    {
        if (versionManifest.Logging?.Client != null)
        // Log4j配置
        AddLoggingConfiguration(args, profile, versionManifest);

        // 额外的JVM参数
        if (profile.Options.ExtraJvmArgs?.Count > 0)
        {
            args.AddRange(profile.Options.ExtraJvmArgs);
        }

        // 版本特定的JVM参数
        if (versionManifest.Arguments?.Jvm != null)
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
        GameProfile profile,
        VersionManifes versionManifest)
    {
        if (versionManifest.Logging?.Client != null)
        {
            var loggingInfo = versionManifest.Logging.Client;
            var loggingInfo = versionManifest.Logging.Client;
            var logPath = Path.Combine(profile.Information.GameDirectory, loggingInfo.File.Id);

            if (File.Exists(logPath))
            {
                args.Add($"-Dlog4j.configurationFile={DirectoryUtil.ForceQuotePath(logPath)}");
            }
        }
    }

    /// <summary>
    /// 生成游戏参数
    /// </summary>
    private static Collection<string> GenerateGameArguments(
        GameProfile profile,
        VersionManifes versionManifest,
    }

    /// <summary>
    /// 生成游戏参数
    /// </summary>
    private static Collection<string> GenerateGameArguments(
        GameProfile profile,
        VersionManifes versionManifest,
        ArgumentsAdapter adapter)
    {
        var args = new Collection<string>();

        if (versionManifest.Arguments?.Game != null)
        if (versionManifest.Arguments?.Game != null)
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
            AddDefaultGameArguments(args, profile, versionManifest);
            // 旧版本格式
            var arguments = versionManifest.MinecraftArguments.Split(' ');
            var processedArgs = ArgumentProcessor.GetEffectiveArguments(arguments, adapter);
            args.AddRange(processedArgs);
        }
        else
        {
            // 默认参数
            AddDefaultGameArguments(args, profile, versionManifest);
        }

        // 全屏模式
        // 全屏模式
        if (profile.Options.FullScreen)
        {
            args.Add("--fullscreen");
        }

        // 额外的游戏参数
        if (profile.Options.ExtraGameArgs?.Count > 0)
        {
            args.AddRange(profile.Options.ExtraGameArgs);
        }

        return args;
    }

    /// <summary>
    /// 添加默认游戏参数
    /// </summary>
    private static void AddDefaultGameArguments(
        Collection<string> args,
        GameProfile profile,
        VersionManifes versionManifest)
    {
        var clientType = profile.Options.IsOfflineMode ? "legacy" : "mojang";

        args.AddRange([
            "--username", profile.Options.Username,
            "--version", profile.Options.VersionId,
            "--gameDir", DirectoryUtil.QuotePath(profile.Information.GameDirectory),
            "--assetsDir", DirectoryUtil.QuotePath(Path.Combine(profile.Information.RootDirectory, "assets")),
            "--assetIndex", versionManifest.AssetIndex?.Id ?? "legacy",
            "--uuid", profile.Options.UUID,
            "--accessToken", profile.Options.AccessToken,
            "--userType", clientType,
            "--versionType", versionManifest.Type
        ]);
    }
}