using PCL.Neo.Core.Models.Configuration.Data;

namespace PCL.Neo.Core.Models.Configuration.Examples;

/// <summary>
/// 配置系统混合方式使用示例
/// </summary>
public static class ConfigurationHybridExample
{
    /// <summary>
    /// 基础配置类 - 所有配置类可继承此类
    /// </summary>
    public abstract class BaseSettings
    {
        /// <summary>
        /// 配置版本
        /// </summary>
        public string Version { get; set; } = "1.0.0";
    }
    
    /// <summary>
    /// 用户设置类 - 继承自基础配置类
    /// </summary>
    [ConfigurationInfo("UserSettings.json")]
    public class UserSettings : BaseSettings
    {
        /// <summary>
        /// 用户名
        /// </summary>
        public string Username { get; set; } = string.Empty;
        
        /// <summary>
        /// 上次登录时间
        /// </summary>
        public string LastLoginTime { get; set; } = string.Empty;
    }
    
    /// <summary>
    /// 配置提供者类 - 包装配置管理器，提供更简便的API
    /// </summary>
    public class SettingsProvider
    {
        private readonly ConfigurationManager _configManager = ConfigurationManager.Instance;
        
        /// <summary>
        /// 获取应用程序设置 (使用GlobalSettings静态类方式)
        /// </summary>
        public async Task<AppSettings> GetAppSettingsAsync()
        {
            string configPath = GlobalSettings.GetConfigFilePath(GlobalSettings.AppSettingsFile);
            
            // 从路径加载，如果不存在则创建默认配置
            var settings = _configManager.LoadFromPath<AppSettings>(configPath);
            if (settings == null)
            {
                settings = new AppSettings();
                await _configManager.SaveToPath(settings, configPath);
            }
            
            return settings;
        }
        
        /// <summary>
        /// 获取用户设置 (使用ConfigurationInfo特性方式)
        /// </summary>
        public async Task<UserSettings> GetUserSettingsAsync()
        {
            return await _configManager.GetOrCreateConfiguration<UserSettings>();
        }
        
        /// <summary>
        /// 保存设置
        /// </summary>
        public async Task<bool> SaveSettingsAsync<T>(T settings) where T : class, new()
        {
            if (settings is AppSettings appSettings)
            {
                string configPath = GlobalSettings.GetConfigFilePath(GlobalSettings.AppSettingsFile);
                return await _configManager.SaveToPath(appSettings, configPath);
            }
            
            return await _configManager.UpdateConfiguration(settings, null);
        }
    }
    
    /// <summary>
    /// 混合使用示例方法
    /// </summary>
    public static async Task RunExampleAsync()
    {
        var provider = new SettingsProvider();
        
        // 1. 使用GlobalSettings静态类方式
        var appSettings = await provider.GetAppSettingsAsync();
        appSettings.Theme = "Dark";
        await provider.SaveSettingsAsync(appSettings);
        
        // 2. 使用ConfigurationInfo特性方式
        var userSettings = await provider.GetUserSettingsAsync();
        userSettings.Username = "TestUser";
        userSettings.LastLoginTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        await provider.SaveSettingsAsync(userSettings);
        
        // 3. 直接使用ConfigurationManager
        var directConfig = await ConfigurationManager.Instance.GetOrCreateConfiguration<UserSettings>();
        Console.WriteLine($"当前用户: {directConfig.Username}");
    }
} 