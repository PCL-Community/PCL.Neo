namespace PCL.Neo.Core.Models.Configuration.Data;

/// <summary>
/// 应用程序全局设置
/// </summary>
[ConfigurationInfo("appSettings.json")]
public record AppSettings
{
    /// <summary>
    /// 应用程序主题
    /// </summary>
    public string Theme { get; set; } = "Light";

    /// <summary>
    /// 应用程序语言
    /// </summary>
    public string Language { get; set; } = "zh-CN";

    /// <summary>
    /// 下载线程数
    /// </summary>
    public int DownloadThreads { get; set; } = 4;

    /// <summary>
    /// 记住的Java路径
    /// </summary>
    public List<string> JavaPaths { get; set; } = new();
}
