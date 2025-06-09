using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using PCL.Neo.Core.Models.Minecraft.Game.Data;
using PCL.Neo.Core.Models.Minecraft.Java;

namespace PCL.Neo.Core.Models.Minecraft.Game
{
    public class LaunchOptions
    {
        /// <summary>
        /// Minecraft版本ID
        /// </summary>
        public string VersionId { get; set; } = string.Empty;

        /// <summary>
        /// Minecraft根目录
        /// </summary>
        public string MinecraftDirectory { get; set; } = string.Empty;

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
        public List<string> ExtraJvmArgs { get; set; } = new List<string>();

        /// <summary>
        /// 额外的游戏参数
        /// </summary>
        public List<string> ExtraGameArgs { get; set; } = new List<string>();

        /// <summary>
        /// 环境变量
        /// </summary>
        public Dictionary<string, string> EnvironmentVariables { get; set; } = new Dictionary<string, string>();

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
        
        public GameLauncher(GameService gameService)
        {
            _gameService = gameService;
        }

        /// <summary>
        /// 启动游戏
        /// </summary>
        public async Task<Process> LaunchAsync(LaunchOptions options)
        {
            // 验证必要参数
            if (string.IsNullOrEmpty(options.VersionId))
                throw new ArgumentException("版本ID不能为空");
            
            if (string.IsNullOrEmpty(options.JavaPath))
                throw new ArgumentException("Java路径不能为空");
            
            // 确保目录存在
            string mcDir = options.MinecraftDirectory;
            if (string.IsNullOrEmpty(mcDir))
                mcDir = GameService.DefaultGameDirectory;
            
            string gameDir = options.GameDirectory;
            if (string.IsNullOrEmpty(gameDir))
                gameDir = mcDir;
                
            // 确保目录存在
            Directory.CreateDirectory(mcDir);
            Directory.CreateDirectory(gameDir);
            
            // 获取版本信息
            var versionInfo = await Versions.GetVersionByIdAsync(mcDir, options.VersionId);
            if (versionInfo == null)
                throw new Exception($"找不到版本 {options.VersionId}");
            
            // 解析继承关系（如果有）
            if (!string.IsNullOrEmpty(versionInfo.InheritsFrom))
            {
                var parentInfo = await Versions.GetVersionByIdAsync(mcDir, versionInfo.InheritsFrom);
                if (parentInfo == null)
                    throw new Exception($"找不到父版本 {versionInfo.InheritsFrom}");
                
                // 合并版本信息
                versionInfo = MergeVersionInfo(versionInfo, parentInfo);
            }
            
            // 构建启动命令
            var commandArgs = BuildLaunchCommand(options, versionInfo, mcDir, gameDir);
            
            // 创建进程
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = options.JavaPath,
                    Arguments = commandArgs,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = false,
                    WorkingDirectory = gameDir
                }
            };
            
            // 设置环境变量
            foreach (var env in options.EnvironmentVariables)
            {
                process.StartInfo.EnvironmentVariables[env.Key] = env.Value;
            }
            
            // 启动进程
            process.Start();
            
            // 记录日志（异步）
            _ = LogProcessOutputAsync(process);
            
            return process;
        }
        
        /// <summary>
        /// 合并版本信息（处理继承关系）
        /// </summary>
        private VersionInfo MergeVersionInfo(VersionInfo child, VersionInfo parent)
        {
            // 创建一个新的合并版本，保留子版本的ID和名称
            var merged = new VersionInfo
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
                    bool exists = false;
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
        private string BuildLaunchCommand(LaunchOptions options, VersionInfo versionInfo, string mcDir, string gameDir)
        {
            var args = new List<string>();
            
            // JVM参数
            args.Add($"-Xmx{options.MaxMemoryMB}M");
            args.Add($"-Xms{options.MinMemoryMB}M");
            
            // 标准JVM参数
            args.Add("-XX:+UseG1GC");
            args.Add("-XX:+ParallelRefProcEnabled");
            args.Add("-XX:MaxGCPauseMillis=200");
            args.Add("-XX:+UnlockExperimentalVMOptions");
            args.Add("-XX:+DisableExplicitGC");
            args.Add("-XX:+AlwaysPreTouch");
            args.Add("-XX:G1NewSizePercent=30");
            args.Add("-XX:G1MaxNewSizePercent=40");
            args.Add("-XX:G1HeapRegionSize=8M");
            args.Add("-XX:G1ReservePercent=20");
            args.Add("-XX:G1HeapWastePercent=5");
            args.Add("-XX:G1MixedGCCountTarget=4");
            args.Add("-XX:InitiatingHeapOccupancyPercent=15");
            args.Add("-XX:G1MixedGCLiveThresholdPercent=90");
            args.Add("-XX:G1RSetUpdatingPauseTimePercent=5");
            args.Add("-XX:SurvivorRatio=32");
            args.Add("-XX:+PerfDisableSharedMem");
            args.Add("-XX:MaxTenuringThreshold=1");
            
            // 设置natives路径
            string nativesDir = Path.Combine(mcDir, "versions", options.VersionId, "natives");
            EnsureDirectoryExists(nativesDir);
            
            args.Add($"-Djava.library.path={QuotePath(nativesDir)}");
            args.Add($"-Dminecraft.launcher.brand=PCL.Neo");
            args.Add($"-Dminecraft.launcher.version=1.0.0");
            
            // 客户端类型
            string clientType = options.IsOfflineMode ? "legacy" : "mojang";
            
            // 添加额外的JVM参数
            if (options.ExtraJvmArgs != null && options.ExtraJvmArgs.Count > 0)
            {
                args.AddRange(options.ExtraJvmArgs);
            }
            
            // 主类
            args.Add(versionInfo.MainClass);
            
            // 游戏参数
            if (!string.IsNullOrEmpty(versionInfo.MinecraftArguments))
            {
                // 旧版格式
                string gameArgs = versionInfo.MinecraftArguments
                    .Replace("${auth_player_name}", options.Username)
                    .Replace("${version_name}", options.VersionId)
                    .Replace("${game_directory}", QuotePath(gameDir))
                    .Replace("${assets_root}", QuotePath(Path.Combine(mcDir, "assets")))
                    .Replace("${assets_index_name}", versionInfo.AssetIndex?.Id ?? "legacy")
                    .Replace("${auth_uuid}", options.UUID)
                    .Replace("${auth_access_token}", options.AccessToken)
                    .Replace("${user_type}", clientType)
                    .Replace("${version_type}", versionInfo.Type);
                
                args.AddRange(gameArgs.Split(' '));
            }
            else if (versionInfo.Arguments != null)
            {
                // 新版格式
                // 这里简化处理，实际上应该解析Arguments对象并应用规则
                if (versionInfo.Arguments.Game != null)
                {
                    foreach (var arg in versionInfo.Arguments.Game)
                    {
                        if (arg is string strArg)
                        {
                            string processedArg = strArg
                                .Replace("${auth_player_name}", options.Username)
                                .Replace("${version_name}", options.VersionId)
                                .Replace("${game_directory}", QuotePath(gameDir))
                                .Replace("${assets_root}", QuotePath(Path.Combine(mcDir, "assets")))
                                .Replace("${assets_index_name}", versionInfo.AssetIndex?.Id ?? "legacy")
                                .Replace("${auth_uuid}", options.UUID)
                                .Replace("${auth_access_token}", options.AccessToken)
                                .Replace("${user_type}", clientType)
                                .Replace("${version_type}", versionInfo.Type);
                            
                            args.Add(processedArg);
                        }
                    }
                }
            }
            else
            {
                // 如果没有参数格式，则使用默认参数
                args.Add("--username");
                args.Add(options.Username);
                args.Add("--version");
                args.Add(options.VersionId);
                args.Add("--gameDir");
                args.Add(QuotePath(gameDir));
                args.Add("--assetsDir");
                args.Add(QuotePath(Path.Combine(mcDir, "assets")));
                args.Add("--assetIndex");
                args.Add(versionInfo.AssetIndex?.Id ?? "legacy");
                args.Add("--uuid");
                args.Add(options.UUID);
                args.Add("--accessToken");
                args.Add(options.AccessToken);
                args.Add("--userType");
                args.Add(clientType);
                args.Add("--versionType");
                args.Add(versionInfo.Type);
            }
            
            // 窗口大小
            if (!options.FullScreen)
            {
                args.Add("--width");
                args.Add(options.WindowWidth.ToString());
                args.Add("--height");
                args.Add(options.WindowHeight.ToString());
            }
            else
            {
                args.Add("--fullscreen");
            }
            
            // 添加额外的游戏参数
            if (options.ExtraGameArgs != null && options.ExtraGameArgs.Count > 0)
            {
                args.AddRange(options.ExtraGameArgs);
            }
            
            // 拼接所有参数
            return string.Join(" ", args);
        }
        
        /// <summary>
        /// 确保目录存在
        /// </summary>
        private void EnsureDirectoryExists(string path)
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
        }
        
        /// <summary>
        /// 为路径加上引号（如果包含空格）
        /// </summary>
        private string QuotePath(string path)
        {
            // 统一路径分隔符为当前系统的分隔符
            path = path.Replace('\\', Path.DirectorySeparatorChar).Replace('/', Path.DirectorySeparatorChar);
            
            // 如果路径包含空格，则加上引号
            if (path.Contains(" "))
            {
                return $"\"{path}\"";
            }
            return path;
        }
        
        /// <summary>
        /// 记录进程输出
        /// </summary>
        private async Task LogProcessOutputAsync(Process process)
        {
            var logDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "PCL.Neo", "logs");
            EnsureDirectoryExists(logDir);
            
            string logFile = Path.Combine(logDir, $"game_{DateTime.Now:yyyyMMdd_HHmmss}.log");
            
            using (var writer = new StreamWriter(logFile, false, Encoding.UTF8))
            {
                writer.WriteLine($"PCL.Neo Game Log - {DateTime.Now}");
                writer.WriteLine("---------------------------------------------");
                
                try
                {
                    string? line;
                    while ((line = await process.StandardOutput.ReadLineAsync()) != null)
                    {
                        await writer.WriteLineAsync($"[STDOUT] {line}");
                        await writer.FlushAsync();
                    }
                }
                catch (Exception ex)
                {
                    await writer.WriteLineAsync($"[ERROR] Error reading standard output: {ex.Message}");
                }
                
                try
                {
                    string? line;
                    while ((line = await process.StandardError.ReadLineAsync()) != null)
                    {
                        await writer.WriteLineAsync($"[STDERR] {line}");
                        await writer.FlushAsync();
                    }
                }
                catch (Exception ex)
                {
                    await writer.WriteLineAsync($"[ERROR] Error reading standard error: {ex.Message}");
                }
            }
        }
        
        /// <summary>
        /// 导出游戏日志
        /// </summary>
        public async Task ExportGameLogsAsync(string filePath)
        {
            var logDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "PCL.Neo", "logs");
            if (!Directory.Exists(logDir) || !Directory.GetFiles(logDir).Any())
            {
                // 如果没有日志，创建一个空日志
                await File.WriteAllTextAsync(filePath, "没有找到游戏日志");
                return;
            }
            
            // 获取最新的日志文件
            var logFile = Directory.GetFiles(logDir, "game_*.log")
                .OrderByDescending(f => File.GetCreationTime(f))
                .FirstOrDefault();
                
            if (string.IsNullOrEmpty(logFile))
            {
                await File.WriteAllTextAsync(filePath, "没有找到游戏日志");
                return;
            }
            
            // 复制日志
            File.Copy(logFile, filePath, true);
        }
    }
} 