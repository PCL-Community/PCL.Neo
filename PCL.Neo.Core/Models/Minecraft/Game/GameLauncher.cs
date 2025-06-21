using PCL.Neo.Core.Models.Minecraft.Game.Data;
using PCL.Neo.Core.Utils;
using System.Diagnostics;

namespace PCL.Neo.Core.Models.Minecraft.Game;

public class GameLauncher : IGameLauncher
{
    /// <summary>
    /// 启动游戏
    /// </summary>
    public async Task<Process> LaunchAsync(GameProfile profile)
    {
        string mcDir = profile.Information.RootDirectory;
        string gameDir = profile.Information.GameDirectory;

        Directory.CreateDirectory(mcDir);
        Directory.CreateDirectory(gameDir);


        var versionInfo = await Versions.GetVersionByIdAsync(mcDir, profile.Options.VersionId)
                          ?? throw new Exception($"找不到版本 {profile.Options.VersionId}");

        if (!string.IsNullOrEmpty(versionInfo.InheritsFrom))
        {
            var parentInfo = await Versions.GetVersionByIdAsync(mcDir, versionInfo.InheritsFrom)
                             ?? throw new Exception($"找不到父版本 {versionInfo.InheritsFrom}");
            versionInfo = MergeVersionInfo(versionInfo, parentInfo);
        }

        var commandArgs = BuildLaunchCommand(profile, versionInfo);

        var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = profile.Options.RunnerJava.JavaExe,
                Arguments = commandArgs,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = false,
                WorkingDirectory = gameDir
            }
        };

        foreach (var env in profile.Options.EnvironmentVariables)
        {
            process.StartInfo.EnvironmentVariables[env.Key] = env.Value;
        }

        process.Start();

        //var gameLogDir = Path.Combine(gameDir, "logs");
        //_gameLogger = new McLogFileLogger(gameLogDir, process);
        //_gameLogger.Start();

        profile.Information.IsRunning = true;

        return process;
    }

    /// <summary>
    /// 合并版本信息（处理继承关系）
    /// </summary>
    private static VersionManifes MergeVersionInfo(VersionManifes child, VersionManifes parent)
    {
        // 创建一个新的合并版本，保留子版本的ID和名称
        var merged = new VersionManifes
        {
            Id = child.Id,
            Name = child.Name,
            Type = child.Type,
            ReleaseTime = child.ReleaseTime,
            Time = child.Time,
            JsonOriginData = child.JsonOriginData,
            // 从父版本继承属性
            MinecraftArguments = child.MinecraftArguments ?? parent.MinecraftArguments,
            Arguments = child.Arguments ?? parent.Arguments,
            MainClass = child.MainClass ?? parent.MainClass,
            AssetIndex = child.AssetIndex ?? parent.AssetIndex,
            Assets = child.Assets ?? parent.Assets,
            JavaVersion = child.JavaVersion ?? parent.JavaVersion,
            // 合并下载信息
            Downloads = child.Downloads ?? parent.Downloads
        };

        // 合并库文件（子版本优先）
        var libraries = new List<Library>();

        if (parent.Libraries != null)
            libraries.AddRange(parent.Libraries);

        if (child.Libraries != null)
        {
            foreach (var lib in child.Libraries)
            {
                // 检查是否已存在
                var exists = libraries.Any(existingLib => existingLib.Name == lib.Name);

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
    private static string BuildLaunchCommand(GameProfile profile, VersionManifes versionManifes)
    {
        List<string> args =
        [
            $"-Xmx{profile.Options.MaxMemoryMB}M",
            $"-Xms{profile.Options.MinMemoryMB}M", // 标准JVM参数
            $"-Xmx{profile.Options.MaxMemoryMB}M",
            $"-Xms{profile.Options.MinMemoryMB}M", // 标准JVM参数
            "-XX:+UseG1GC",
            "-XX:+ParallelRefProcEnabled",
            "-XX:MaxGCPauseMillis=200",
            "-XX:+UnlockExperimentalVMOptions",
            "-XX:+DisableExplicitGC",
            "-XX:+AlwaysPreTouch",
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
        DirectoryUtil.EnsureDirectoryExists(nativesDir);

        args.Add($"-Djava.library.path={DirectoryUtil.QuotePath(nativesDir)}");
        args.Add($"-Dminecraft.launcher.brand=PCL.Neo");
        args.Add($"-Dminecraft.launcher.version=1.0.0"); // TODO: load version from configuration

        // 类路径
        args.Add("-cp");
        List<string> classpaths = [];
        if (versionManifes.Libraries != null)
        {
            foreach (Library library in versionManifes.Libraries)
            {
                if (library.Downloads?.Artifact?.Path != null)
                {
                    classpaths.Add(Path.Combine(profile.Information.RootDirectory, "libraries",
                        library.Downloads!.Artifact!.Path!)); // 不用担心空格问题
                }
            }
        }

        classpaths.Add(Path.Combine(profile.Information.GameDirectory, profile.Options.VersionId));
        args.Add(string.Join(SystemUtils.Os == SystemUtils.RunningOs.Windows ? ';' : ':', classpaths));

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
        if (!string.IsNullOrEmpty(versionManifes.MinecraftArguments))
        {
            // 旧版格式
            string gameArgs = versionManifes.MinecraftArguments
                .Replace("${auth_player_name}", profile.Options.Username)
                .Replace("${version_name}", profile.Options.VersionId)
                .Replace("${game_directory}", DirectoryUtil.QuotePath(profile.Information.GameDirectory))
                .Replace("${assets_root}",
                    DirectoryUtil.QuotePath(Path.Combine(profile.Information.RootDirectory, "assets")))
                .Replace("${assets_index_name}", versionManifes.AssetIndex?.Id ?? "legacy")
                .Replace("${auth_uuid}", profile.Options.UUID)
                .Replace("${auth_access_token}", profile.Options.AccessToken)
                .Replace("${user_type}", clientType)
                .Replace("${version_type}", versionManifes.Type);
            args.AddRange(gameArgs.Split(' '));
        }
        else if (versionManifes.Arguments != null)
        {
            // 新版格式
            // 这里简化处理，实际上应该解析Arguments对象并应用规则
            if (versionManifes.Arguments.Game is not null)
            {
                foreach (var arg in versionManifes.Arguments.Game)
                {
                    if (arg is string strArg)
                    {
                        string processedArg = strArg
                            .Replace("${auth_player_name}", profile.Options.Username)
                            .Replace("${version_name}", profile.Options.VersionId)
                            .Replace("${game_directory}", DirectoryUtil.QuotePath(profile.Information.GameDirectory))
                            .Replace("${assets_root}",
                                DirectoryUtil.QuotePath(Path.Combine(profile.Information.RootDirectory, "assets")))
                            .Replace("${assets_index_name}", versionManifes.AssetIndex?.Id ?? "legacy")
                            .Replace("${auth_uuid}", profile.Options.UUID)
                            .Replace("${auth_access_token}", profile.Options.AccessToken)
                            .Replace("${user_type}", clientType)
                            .Replace("${version_type}", versionManifes.Type);

                        args.Add(processedArg);
                    }
                }
            }
        }
        else
        {
            // 如果没有参数格式，则使用默认参数
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
        if (!profile.Options.FullScreen)
        {
            args.Add("--width");
            args.Add(profile.Options.WindowWidth.ToString());
            args.Add("--height");
            args.Add(profile.Options.WindowHeight.ToString());
        }
        else
        {
            args.Add("--fullscreen");
        }

        // 添加额外的游戏参数
        if (profile.Options.ExtraGameArgs is { Count: > 0 })
        {
            args.AddRange(profile.Options.ExtraGameArgs);
        }

        // 拼接所有参数
        return string.Join(' ', args);
    }
}