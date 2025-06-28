using PCL.Neo.Core.Models.Minecraft.Game.Data;
using System.Diagnostics;

namespace PCL.Neo.Core.Models.Minecraft.Game;

/// <summary>
/// 游戏启动器接口，定义启动与日志导出功能
/// </summary>
public interface IGameLauncher
{
    /// <summary>
    /// 启动游戏
    /// </summary>
    /// <param name="profile">启动参数</param>
    /// <returns>游戏进程</returns>
    Task<Process> LaunchAsync(GameProfile profile);
}