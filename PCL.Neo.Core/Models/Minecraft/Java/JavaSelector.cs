using PCL.Neo.Core.Models.Minecraft.Game.Data.Arguments.Manifest;
using PCL.Neo.Core.Utils;

namespace PCL.Neo.Core.Models.Minecraft.Java;

/// <summary>
/// Java选择器，用于为不同版本的游戏选择最合适的Java
/// </summary>
public static class JavaSelector
{
    /// <summary>
    /// Java兼容分数结构
    /// </summary>
    public record JavaCompatibilityScore
    {
        /// <summary>
        /// Java运行时
        /// </summary>
        public required JavaRuntime Runtime { get; init; }

        /// <summary>
        /// 兼容性得分(越高越兼容)
        /// </summary>
        public int Score { get; init; }

        /// <summary>
        /// 推荐级别
        /// </summary>
        public RecommendationLevel RecommendationLevel { get; init; }

        /// <summary>
        /// 推荐原因
        /// </summary>
        public string Reason { get; init; } = string.Empty;
    }

    /// <summary>
    /// Java推荐级别
    /// </summary>
    public enum RecommendationLevel
    {
        /// <summary>
        /// 完美匹配，官方指定版本
        /// </summary>
        Perfect = 4,

        /// <summary>
        /// 高度推荐，适合该版本的最佳选择
        /// </summary>
        Recommended = 3,

        /// <summary>
        /// 可用，能满足基本需求
        /// </summary>
        Acceptable = 2,

        /// <summary>
        /// 勉强可用，可能会有兼容性问题
        /// </summary>
        Marginal = 1,

        /// <summary>
        /// 不兼容，不应使用
        /// </summary>
        Incompatible = 0
    }

    /// <summary>
    /// 为游戏实体选择最合适的Java
    /// </summary>
    /// <param name="version">游戏信息清单</param>
    /// <param name="availableJavas">可用的Java列表</param>
    /// <returns>排序后的Java兼容性得分列表</returns>
    public static List<JavaCompatibilityScore> SelectJavaForGame(
        VersionManifest version,
        IEnumerable<JavaRuntime> availableJavas)
    {
        // 如果没有可用的Java，返回空列表
        var availableJava = availableJavas
            .Where(java => java.Compability == JavaCompability.Yes); // 只选择兼容的Java

        if (!availableJava.Any())
        {
            return [];
        }

        // 获取游戏推荐的Java版本范围
        var (minJavaVersion, maxJavaVersion) = version.MatchJavaVersionSpan();

        // 如果游戏有明确指定Java版本
        var hasSpecificJavaRequirement = version.JavaVersion is { MajorVersion: > 0 };

        var specificJavaVersion =
            hasSpecificJavaRequirement ? version.JavaVersion?.MajorVersion ?? 0 : 0;

        // 对每个Java评分
        var result = availableJava.Select(java =>
                ScoreJavaForGame(java, minJavaVersion, maxJavaVersion, specificJavaVersion))
            .OrderByDescending(score => score.Score)
            .ToList();

        return result;
    }

    /// <summary>
    /// 为Java根据游戏要求评分
    /// </summary>
    private static JavaCompatibilityScore ScoreJavaForGame(
        JavaRuntime java,
        int minJavaVersion,
        int maxJavaVersion,
        int specificJavaVersion)
    {
        var score = 0;
        string reason;
        RecommendationLevel level;

        // 1. 版本兼容性检查
        if (specificJavaVersion > 0)
        {
            // 有明确的Java版本要求
            if (java.SlugVersion == specificJavaVersion)
            {
                // 完美匹配
                score += 1000;
                level = RecommendationLevel.Perfect;
                reason = $"完美匹配游戏指定的Java {specificJavaVersion}";
            }
            else if (java.SlugVersion > specificJavaVersion)
            {
                // 版本高于需求
                var versionDiff = java.SlugVersion - specificJavaVersion;
                if (versionDiff <= 3)
                {
                    // 版本接近，可能兼容
                    score += 500 - versionDiff * 50;
                    level = RecommendationLevel.Acceptable;
                    reason = $"版本高于游戏需求(Java {specificJavaVersion})，但仍可能兼容";
                }
                else
                {
                    // 版本差距大，可能不兼容
                    score += 200;
                    level = RecommendationLevel.Marginal;
                    reason = $"版本明显高于游戏需求(Java {specificJavaVersion})，可能存在兼容性问题";
                }
            }
            else
            {
                // 版本低于需求
                var versionDiff = specificJavaVersion - java.SlugVersion;
                score -= 100 * versionDiff;
                level = versionDiff > 3 ? RecommendationLevel.Incompatible : RecommendationLevel.Marginal;
                reason = $"版本低于游戏需求(Java {specificJavaVersion})，不推荐使用";
            }
        }
        else
        {
            // 根据版本范围判断
            if (java.SlugVersion >= minJavaVersion && java.SlugVersion <= maxJavaVersion)
            {
                // 在版本范围内
                score += 800;
                level = RecommendationLevel.Recommended;
                reason = $"版本适合此游戏(Java {minJavaVersion}-{maxJavaVersion})";

                // 特殊处理：偏好中间版本
                if (maxJavaVersion > minJavaVersion)
                {
                    var idealVersion = (minJavaVersion + maxJavaVersion) / 2;
                    var versionDiff = Math.Abs(java.SlugVersion - idealVersion);
                    score -= versionDiff * 10; // 越接近理想版本越好
                }
            }
            else if (java.SlugVersion < minJavaVersion)
            {
                // 版本过低
                var versionDiff = minJavaVersion - java.SlugVersion;
                score -= 200 * versionDiff;
                level = RecommendationLevel.Incompatible;
                reason = $"版本过低，游戏需要至少Java {minJavaVersion}";
            }
            else
            {
                // 版本过高
                var versionDiff = java.SlugVersion - maxJavaVersion;
                if (versionDiff <= 3)
                {
                    // 版本高但接近
                    score += 400 - versionDiff * 50;
                    level = RecommendationLevel.Acceptable;
                    reason = $"版本高于游戏推荐范围(Java {maxJavaVersion})，但可能兼容";
                }
                else
                {
                    // 版本明显过高
                    score += 100;
                    level = RecommendationLevel.Marginal;
                    reason = $"版本明显高于游戏推荐范围(Java {maxJavaVersion})，可能存在兼容性问题";
                }
            }
        }

        // 2. 架构兼容性分数
        if (java.Is64Bit)
        {
            score += 100; // 64位Java通常性能更好
        }

        // 3. 厂商偏好分数
        var vendor = JavaVerifier.JavaVendor.Unknown;
        try
        {
            // 尝试获取厂商信息
            if (!string.IsNullOrEmpty(java.Implementor))
            {
                if (java.Implementor.Contains("Oracle", StringComparison.OrdinalIgnoreCase))
                    vendor = JavaVerifier.JavaVendor.Oracle;
                else if (java.Implementor.Contains("Adopt", StringComparison.OrdinalIgnoreCase))
                    vendor = JavaVerifier.JavaVendor.AdoptOpenJDK;
                else if (java.Implementor.Contains("Eclipse", StringComparison.OrdinalIgnoreCase))
                    vendor = JavaVerifier.JavaVendor.AdoptiumEclipse;
                else if (java.Implementor.Contains("Microsoft", StringComparison.OrdinalIgnoreCase))
                    vendor = JavaVerifier.JavaVendor.Microsoft;
                else if (java.Implementor.Contains("Amazon", StringComparison.OrdinalIgnoreCase))
                    vendor = JavaVerifier.JavaVendor.Amazon;
                else if (java.Implementor.Contains("Azul", StringComparison.OrdinalIgnoreCase))
                    vendor = JavaVerifier.JavaVendor.Azul;
            }
        }
        catch
        {
            // 忽略任何异常
        }

        // 根据厂商给予额外分数
        switch (vendor)
        {
            case JavaVerifier.JavaVendor.Oracle:
            case JavaVerifier.JavaVendor.AdoptiumEclipse:
            case JavaVerifier.JavaVendor.AdoptOpenJDK:
                score += 60; // 提高主流厂商的分数
                break;
            case JavaVerifier.JavaVendor.Microsoft:
            case JavaVerifier.JavaVendor.Amazon:
            case JavaVerifier.JavaVendor.Azul:
                score += 40; // 其他知名厂商
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        // 4. JDK优先于JRE，因为JDK包含工具更加全面
        if (!java.IsJre)
        {
            score += 40;
        }

        // 确保设置正确的推荐级别
        if (level != RecommendationLevel.Perfect && level != RecommendationLevel.Incompatible)
        {
            level = score switch
            {
                >= 800 => RecommendationLevel.Recommended,
                >= 500 => RecommendationLevel.Acceptable,
                >= 200 => RecommendationLevel.Marginal,
                _ => RecommendationLevel.Incompatible
            };
        }

        return new JavaCompatibilityScore
        {
            Runtime = java,
            Score = score,
            RecommendationLevel = level,
            Reason = reason
        };
    }

    /// <summary>
    /// 获取推荐级别的描述
    /// </summary>
    /// <param name="level">推荐级别</param>
    /// <returns>中文描述</returns>
    public static string GetRecommendationDescription(RecommendationLevel level)
    {
        return level switch
        {
            RecommendationLevel.Perfect => "完美兼容",
            RecommendationLevel.Recommended => "较好兼容",
            RecommendationLevel.Acceptable => "可用",
            RecommendationLevel.Marginal => "基本可用",
            RecommendationLevel.Incompatible => "不兼容",
            _ => "未知"
        };
    }
}
