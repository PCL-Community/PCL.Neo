using PCL.Neo.Core.Models.Minecraft.Game.Data.Arguments.Manifest;

namespace PCL.Neo.Core.Models.Minecraft.Java;

public static class JavaVersionMatcherForGameEntityExtension
{
    /// <summary>
    /// 根据游戏版本计算其适配的Java版本范围
    /// </summary>
    /// <param name="versionManifest">游戏版本信息</param>
    /// <returns>适配的Java版本范围</returns>
    public static (int min, int max) MatchJavaVersionSpan(this VersionManifest versionManifest)
    {
        if (versionManifest.Type == "release")
        {
        }
        else if (versionManifest.Type == "snapshot")
        {
        }

        return (0, 0);
    }
}
