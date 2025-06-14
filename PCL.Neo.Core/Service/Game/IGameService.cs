using PCL.Neo.Core.Models.Minecraft.Game.Data;
using PCL.Neo.Core.Models.Minecraft.Java;

namespace PCL.Neo.Core.Service.Game
{
    /// <summary>
    /// 游戏服务接口，提供游戏版本管理、下载、校验等功能
    /// </summary>
    public interface IGameService
    {
        /// <summary>
        /// 获取游戏版本列表
        /// </summary>
        /// <param name="minecraftDirectory">Minecraft 目录</param>
        /// <param name="forceRefresh">是否强制刷新远程</param>
        /// <returns>版本信息列表</returns>
        Task<List<VersionManifes>> GetVersionsAsync(string? minecraftDirectory = null, bool forceRefresh = false);

        /// <summary>
        /// 下载指定版本的游戏
        /// </summary>
        /// <param name="versionId">版本ID</param>
        /// <param name="progressCallback">进度回调</param>
        /// <returns>是否成功</returns>
        Task<bool> DownloadVersionAsync(string versionId, IProgress<int>? progressCallback = null);

        /// <summary>
        /// 检查版本是否已安装
        /// </summary>
        /// <param name="versionId">版本ID</param>
        /// <param name="minecraftDirectory">Minecraft 目录</param>
        /// <returns>是否已安装</returns>
        [Obsolete]
        bool IsVersionInstalled(string versionId, string? minecraftDirectory = null);

        /// <summary>
        /// 删除版本
        /// </summary>
        /// <param name="versionId">版本ID</param>
        /// <param name="minecraftDirectory">Minecraft 目录</param>
        void DeleteVersionAsync(string versionId, string? minecraftDirectory = null);

        /// <summary>
        /// 检查Java版本是否兼容指定的Minecraft版本
        /// </summary>
        /// <param name="javaRuntime">Java运行时</param>
        /// <param name="minecraftVersion">Minecraft版本</param>
        /// <returns>是否兼容</returns>
        bool IsJavaCompatibleWithGame(JavaRuntime javaRuntime, string minecraftVersion);

        /// <summary>
        /// 默认Java运行时组合
        /// </summary>
        (JavaRuntime? Java8, JavaRuntime? Java17, JavaRuntime? Java21) DefaultJavaRuntimes { get; }
    }
}
