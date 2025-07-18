using PCL.Neo.Core.Models.Minecraft.Game.Data;
using PCL.Neo.Core.Models.Minecraft.Game.Data.Arguments.Manifest;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace PCL.Neo.Core.Service.Game;

/// <summary>
/// 游戏启动器接口，定义启动与日志导出功能
/// </summary>
public interface IGameLauncherService
{
    /// <summary>
    /// Version manifes information.
    /// </summary>
    VersionManifest? Manifest { get; }

    /// <summary>
    /// Game infomation.
    /// </summary>
    GameInfo Information { get; init; }

    /// <summary>
    /// Launch options.
    /// </summary>
    LaunchOptions Options { get; init; }

    /// <summary>
    /// 启动游戏
    /// </summary>
    /// <param name="arguments">启动参数</param>
    /// <returns>游戏进程</returns>
    Task<Process> LaunchAsync(Collection<string> arguments);

    /// <summary>
    /// 构建游戏启动命令
    /// </summary>
    /// <exception cref="DirectoryNotFoundException">Throw if directory not found.</exception>
    /// <exception cref="ArgumentNullException">Throw if Libraries is null.</exception>
    Task<Collection<string>> BuildLaunchCommandAsync();
}
