using PCL.Neo.Core.Models.Minecraft.Game.Data;

namespace PCL.Neo.Core.Models.Minecraft.Java;

public static class JavaVersionMatcherForGameEntityExtension
{
    /// <summary>
    /// 根据游戏版本计算其适配的Java版本范围
    /// </summary>
    /// <param name="versionManifes">游戏版本信息</param>
    /// <returns>适配的Java版本范围</returns>
    public static (int min, int max) MatchJavaVersionSpan(this VersionManifes versionManifes)
    {
        if (versionManifes.Type == "release")
        {
        }
        else if (versionManifes.Type == "snapshot")
        {
        }
        return (0, 0);
    }
}