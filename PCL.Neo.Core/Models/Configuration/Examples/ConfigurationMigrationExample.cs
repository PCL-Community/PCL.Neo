namespace PCL.Neo.Core.Models.Configuration.Examples;

/// <summary>
/// 配置迁移和访问器使用示例
/// </summary>
public static class ConfigurationMigrationExample
{
    /// <summary>
    /// 游戏设置配置类 - 用于演示配置迁移
    /// </summary>
    [ConfigurationInfo("GameSettings.json")]
    public class GameSettings
    {
        /// <summary>
        /// 配置版本
        /// </summary>
        public int Version { get; set; } = 1;
        
        /// <summary>
        /// 游戏分辨率宽度
        /// </summary>
        public int ResolutionWidth { get; set; } = 1280;
        
        /// <summary>
        /// 游戏分辨率高度
        /// </summary>
        public int ResolutionHeight { get; set; } = 720;
        
        /// <summary>
        /// 游戏音量
        /// </summary>
        public int Volume { get; set; } = 80;
        
        // 版本2新增字段
        /// <summary>
        /// 是否启用垂直同步
        /// </summary>
        public bool VSync { get; set; } = true;
        
        // 版本3新增字段
        /// <summary>
        /// 图形质量
        /// </summary>
        public string GraphicsQuality { get; set; } = "Medium";
    }
    
    /// <summary>
    /// 注册迁移示例
    /// </summary>
    public static void RegisterMigrations()
    {
        var manager = ConfigurationManager.Instance;
        
        // 注册从版本1迁移到版本2的操作
        manager.RegisterMigration<GameSettings>(settings =>
        {
            if (settings.Version == 1)
            {
                Console.WriteLine("正在将GameSettings从版本1迁移到版本2...");
                // 设置新增的VSync字段默认值
                settings.VSync = true;
                // 更新版本号
                settings.Version = 2;
            }
        });
        
        // 注册从版本2迁移到版本3的操作
        manager.RegisterMigration<GameSettings>(settings =>
        {
            if (settings.Version == 2)
            {
                Console.WriteLine("正在将GameSettings从版本2迁移到版本3...");
                // 设置新增的GraphicsQuality字段默认值
                settings.GraphicsQuality = "Medium";
                // 更新版本号
                settings.Version = 3;
            }
        });
    }
    
    /// <summary>
    /// 使用配置访问器示例
    /// </summary>
    public static async Task UseAccessorExampleAsync()
    {
        // 确保已注册迁移
        RegisterMigrations();
        
        // 获取配置访问器
        var accessor = ConfigurationManager.Instance.GetAccessor<GameSettings>();
        
        // 加载配置 (会自动应用迁移)
        var settings = await accessor.GetConfigAsync();
        Console.WriteLine($"当前配置版本: {settings.Version}");
        Console.WriteLine($"分辨率: {settings.ResolutionWidth}x{settings.ResolutionHeight}");
        
        // 更新配置
        await accessor.UpdateAsync(s =>
        {
            s.ResolutionWidth = 1920;
            s.ResolutionHeight = 1080;
            s.GraphicsQuality = "High";
        });
        
        Console.WriteLine("配置已更新");
        
        // 备份配置
        bool backupResult = await accessor.BackupAsync();
        Console.WriteLine($"配置备份{(backupResult ? "成功" : "失败")}");
        
        // 重置为默认值
        if (await accessor.ResetToDefaultAsync())
        {
            var defaultSettings = await accessor.GetConfigAsync();
            Console.WriteLine($"已重置为默认值: {defaultSettings.ResolutionWidth}x{defaultSettings.ResolutionHeight}");
        }
    }
} 