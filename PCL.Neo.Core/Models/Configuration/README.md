# 配置系统使用文档

配置系统用于管理应用程序的各种配置，提供了统一的API进行配置的读取、保存、迁移和备份等操作。本文档将介绍如何使用配置系统。

## 1. 配置类定义

配置类用于定义配置项的结构。有两种方式定义配置类：

### 1.1 使用特性标注方式

```csharp
[ConfigurationInfo("AppSettings.json")]
public record AppSettings
{
    public string Theme { get; set; } = "Light";
    public string Language { get; set; } = "zh-CN";
    public int DownloadThreads { get; set; } = 4;
}
```

### 1.2 使用继承方式（便于配置类分层管理）

```csharp
// 基础配置类
public abstract class BaseSettings
{
    public string Version { get; set; } = "1.0.0";
}

// 继承的配置类
[ConfigurationInfo("UserSettings.json")]
public class UserSettings : BaseSettings
{
    public string Username { get; set; } = string.Empty;
    public string LastLoginTime { get; set; } = string.Empty;
}
```

## 2. 基本使用方法

### 2.1 获取配置管理器实例

配置管理器采用单例模式，可以通过以下方式获取实例：

```csharp
var configManager = ConfigurationManager.Instance;
```

### 2.2 获取配置

```csharp
// 获取配置（如果不存在则返回null）
var appSettings = configManager.GetConfiguration<AppSettings>();

// 获取配置（如果不存在则创建默认配置）
var appSettings = await configManager.GetOrCreateConfiguration<AppSettings>();
```

### 2.3 保存配置

```csharp
// 创建新配置
var newConfig = new AppSettings { Theme = "Dark" };
await configManager.CreateConfiguration(newConfig, null);

// 更新已有配置
appSettings.Language = "en-US";
await configManager.UpdateConfiguration(appSettings, null);
```

## 3. 使用GlobalSettings管理配置文件路径

GlobalSettings类提供了集中管理配置文件路径的方式：

```csharp
public static class GlobalSettings
{
    public static string AppSettingsFile => "AppSettings.json";
    
    public static string GetConfigFilePath(string fileName)
    {
        return Path.Combine(AppContext.BaseDirectory, "Configs", fileName);
    }
}

// 使用方式
string configPath = GlobalSettings.GetConfigFilePath(GlobalSettings.AppSettingsFile);
var appSettings = configManager.LoadFromPath<AppSettings>(configPath);
```

## 4. 配置迁移

配置迁移用于处理配置结构变化的情况，确保老版本的配置文件能够正确升级到新版本。

### 4.1 定义带版本的配置类

```csharp
[ConfigurationInfo("GameSettings.json")]
public class GameSettings
{
    // 配置版本号
    public int Version { get; set; } = 1;
    
    // 版本1字段
    public int ResolutionWidth { get; set; } = 1280;
    public int ResolutionHeight { get; set; } = 720;
    
    // 版本2新增字段
    public bool VSync { get; set; } = true;
}
```

### 4.2 注册迁移操作

```csharp
// 注册从版本1迁移到版本2的操作
configManager.RegisterMigration<GameSettings>(settings =>
{
    if (settings.Version == 1)
    {
        // 设置新增字段的默认值
        settings.VSync = true;
        // 更新版本号
        settings.Version = 2;
    }
});
```

### 4.3 自动迁移

配置管理器会在加载配置时自动应用已注册的迁移操作。

## 5. 使用配置访问器

配置访问器提供了更便捷的API进行配置操作：

```csharp
// 获取配置访问器
var accessor = ConfigurationManager.Instance.GetAccessor<AppSettings>();

// 获取配置
var settings = await accessor.GetConfigAsync();

// 更新配置
await accessor.UpdateAsync(s =>
{
    s.Theme = "Dark";
    s.Language = "en-US";
});

// 备份配置
await accessor.BackupAsync();

// 重置为默认值
await accessor.ResetToDefaultAsync();
```

## 6. 完整示例

```csharp
// 注册迁移（应在应用启动时进行）
ConfigurationManager.Instance.RegisterMigration<GameSettings>(settings =>
{
    if (settings.Version == 1)
    {
        settings.VSync = true;
        settings.Version = 2;
    }
});

// 使用配置访问器
var accessor = ConfigurationManager.Instance.GetAccessor<GameSettings>();
var settings = await accessor.GetConfigAsync();

// 更新配置
await accessor.UpdateAsync(s => s.ResolutionWidth = 1920);

// 备份配置
await accessor.BackupAsync();
```

## 7. 最佳实践

1. **使用配置访问器**：尽可能使用配置访问器API，它提供了更简洁的操作方式。

2. **注册迁移**：对于可能随版本变化的配置，务必添加版本字段并注册迁移操作。

3. **使用默认值**：在配置类中为所有属性提供合理的默认值，确保创建新配置时有正确的初始状态。

4. **错误处理**：配置系统使用try-catch处理异常，通过返回值而非异常表达操作结果，请检查返回值。

5. **路径管理**：使用GlobalSettings管理配置文件路径，避免硬编码。

## 8. 常见问题

1. **配置加载失败**：确保配置文件存在且格式正确。可以使用GetOrCreateConfiguration方法自动创建不存在的配置。

2. **配置迁移不生效**：确保在加载配置前已注册迁移操作，且版本号判断条件正确。

3. **配置保存失败**：检查文件权限和路径是否正确。配置系统会尝试创建不存在的目录。 