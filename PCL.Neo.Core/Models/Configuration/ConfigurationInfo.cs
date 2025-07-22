namespace PCL.Neo.Core.Models.Configuration;

/// <summary>
/// 全局配置设置，集中管理配置文件路径和默认值
/// </summary>
public static class ConfigurationInfo
{
    /// <summary>
    /// 应用程序配置文件路径
    /// </summary>
    public static string AppSettingsFile => "appSettings.json";

    /// <summary>
    /// OAuth2配置文件路径
    /// </summary>
    public static string OAuth2ConfigurationFile => "OAuth2Configuration.json";

    /// <summary>
    /// 获取配置文件的完整路径
    /// </summary>
    /// <param name="fileName">配置文件名</param>
    /// <returns>完整路径</returns>
    public static string GetConfigFilePath(string fileName)
    {
        return Path.Combine(AppContext.BaseDirectory, "configs", fileName);
    }
}
