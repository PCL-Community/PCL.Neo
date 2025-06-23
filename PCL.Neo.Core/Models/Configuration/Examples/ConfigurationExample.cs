using System.Text.Json;
using System.Threading.Tasks;
using PCL.Neo.Core.Models.Configuration.Data;

namespace PCL.Neo.Core.Models.Configuration.Examples;

/// <summary>
/// 配置系统使用示例
/// </summary>
public static class ConfigurationExample
{
    /// <summary>
    /// 使用配置系统的示例方法
    /// </summary>
    public static async Task ExampleAsync()
    {
        // 获取配置管理器实例
        var configManager = ConfigurationManager.Instance;
        
        // 示例1: 使用GlobalSettings和ConfigurationInfo特性加载配置
        var appSettings = await configManager.GetOrCreateConfiguration<AppSettings>();
        
        // 修改配置
        appSettings.Theme = "Dark";
        appSettings.DownloadThreads = 8;
        
        // 更新配置
        bool updateSuccess = await configManager.UpdateConfiguration(appSettings, null);
        
        // 示例2: 使用显式路径加载配置
        string customConfigPath = GlobalSettings.GetConfigFilePath("custom-config.json");
        var customConfig = new AppSettings
        {
            Theme = "Custom",
            Language = "en-US"
        };
        
        // 保存到指定路径
        bool saveSuccess = await configManager.SaveToPath(customConfig, customConfigPath);
        
        // 从指定路径加载
        var loadedCustomConfig = configManager.LoadFromPath<AppSettings>(customConfigPath);
    }
} 