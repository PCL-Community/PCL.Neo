using PCL.Neo.Core.Models.Minecraft.Game.Data;

namespace PCL.Neo.Core.Models.Minecraft.Java;

using DefaultJavaRuntimeCombine = (JavaRuntime? Java8, JavaRuntime? Java17, JavaRuntime? Java21);

/// <summary>
/// Java管理器接口
/// </summary>
public interface IJavaManager
{
    /// <summary>
    /// Java列表
    /// </summary>
    List<JavaRuntime> JavaList { get; }

    /// <summary>
    /// 默认Java运行时
    /// </summary>
    DefaultJavaRuntimeCombine DefaultJavaRuntimes { get; }

    /// <summary>
    /// 初始化Java列表
    /// </summary>
    Task JavaListInitAsync();

    /// <summary>
    /// 手动添加Java
    /// </summary>
    /// <param name="javaDir">Java目录</param>
    /// <returns>Java运行时和是否更新当前</returns>
    Task<(JavaRuntime?, bool UpdateCurrent)> ManualAdd(string javaDir);

    /// <summary>
    /// 刷新Java列表
    /// </summary>
    Task Refresh();

    /// <summary>
    /// 获取适合游戏版本的Java列表
    /// </summary>
    /// <param name="gameEntity">游戏实体</param>
    /// <returns>Java兼容性分数列表</returns>
    List<JavaSelector.JavaCompatibilityScore> GetCompatibleJavas(GameEntityInfo gameEntity);

    /// <summary>
    /// 获取最适合游戏版本的Java
    /// </summary>
    /// <param name="gameEntity">游戏实体</param>
    /// <returns>最合适的Java或null</returns>
    JavaRuntime? GetBestJavaForGame(GameEntityInfo gameEntity);

    /// <summary>
    /// 获取Java的验证结果
    /// </summary>
    /// <param name="java">要验证的Java</param>
    /// <returns>Java验证结果</returns>
    Task<JavaVerifier.JavaVerifyResult> GetJavaVerificationAsync(JavaRuntime java);

    /// <summary>
    /// 清除验证结果缓存
    /// </summary>
    void ClearVerificationCache();
}
