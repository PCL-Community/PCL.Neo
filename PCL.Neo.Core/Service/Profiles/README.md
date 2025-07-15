# ProfileService 使用文档

ProfileService 是 PCL.Neo 的档案管理服务，用于管理 Minecraft 游戏的档案信息，包括游戏版本、Mod加载器类型等配置信息。

## 1. 主要功能

ProfileService 提供以下主要功能：

- 加载和保存游戏档案
- 管理游戏配置信息
- 处理多个游戏版本
- 支持不同类型的游戏加载器（Vanilla、Forge、Fabric等）

## 2. 主要方法说明

### 2.1 档案加载方法

#### LoadProfilesDefaultAsync

```csharp
Task<IEnumerable<ProfileInfo>> LoadProfilesDefaultAsync()
```

从默认目录加载所有档案信息。

#### LoadProfilesAsync

```csharp
Task<IEnumerable<ProfileInfo>> LoadProfilesAsync(string targetDir)
```

从指定目录加载所有档案信息。

#### GetProfileAsync

```csharp
Task<ProfileInfo> GetProfileAsync(string targetDir, string profileName)
```

从指定目录加载指定名称的档案。

### 2.2 游戏加载方法

#### GetTargetGameAsync

```csharp
Task<GameInfo> GetTargetGameAsync(string targetDir, string gameName)
```

从指定目录加载特定的游戏版本信息。

### 2.3 档案保存方法

#### SaveProfilesAsync

```csharp
Task<bool> SaveProfilesAsync(string targetDir, ProfileInfo profile)
```

将档案信息保存到指定目录。

#### SaveProfilesDefaultAsync

```csharp
Task<bool> SaveProfilesDefaultAsync(ProfileInfo profile)
```

将档案信息保存到默认目录。

### 2.4 游戏信息管理方法

#### SaveGameInfoToProfileDefaultAsync

```csharp
Task<bool> SaveGameInfoToProfileDefaultAsync(ProfileInfo profile, GameInfo game)
```

将游戏信息保存到指定档案的默认目录。

#### SaveGameInfoToProfileAsync

```csharp
Task<bool> SaveGameInfoToProfileAsync(ProfileInfo profile, GameInfo game, string targetDir)
```

将游戏信息保存到指定档案和目录。

#### DeleteGame

```csharp
bool DeleteGame(GameInfo game, ProfileInfo profile)
```

从档案中删除指定的游戏。

## 3. 使用示例

### 3.1 加载档案

```csharp
// 从默认目录加载所有档案
var profiles = await _profileService.LoadProfilesDefaultAsync();

// 加载特定档案
var profile = await _profileService.GetProfileAsync(targetDir, "ProfileName");
```

### 3.2 保存档案

```csharp
// 创建新的档案信息
var profile = new ProfileInfo
{
    ProfileName = "TestProfile",
    TargetDir = targetDir,
    Games = new List<GameInfo>()
};

// 保存到默认目录
await _profileService.SaveProfilesDefaultAsync(profile);
```

### 3.3 管理游戏信息

```csharp
// 加载特定游戏版本
var game = await _profileService.GetTargetGameAsync(targetDir, "1.20.6-Fabric");

// 将游戏信息添加到档案
await _profileService.SaveGameInfoToProfileAsync(profile, game, targetDir);
```

## 4. 注意事项

1. 目录结构要求：
  - 目标目录必须包含以下子目录：
    - assets
    - libraries
    - versions
  - 如果缺少必要的子目录，将抛出 InvalidOperationException

2. 文件验证：
  - 游戏版本目录必须包含对应的 .jar 和 .json 文件
  - 文件缺失时将抛出 FileNotFoundException

3. 异常处理：
  - 建议捕获并处理可能的异常：
    - InvalidOperationException（无效操作）
    - DirectoryNotFoundException（目录不存在）
    - FileNotFoundException（文件不存在）

4. 线程安全：
  - 所有异步方法都是线程安全的
  - 建议使用 await 等待操作完成

5. 性能考虑：
  - 档案信息会被缓存
  - 建议复用 ProfileService 实例
  - 对于批量操作，优先使用批量方法而不是多次调用单个方法

## 5. 最佳实践

1. 使用依赖注入：

```csharp
services.AddSingleton<IProfileService, ProfileService>();
```

2. 异常处理示例：

```csharp
try
{
    var profile = await _profileService.GetProfileAsync(targetDir, profileName);
    // 处理档案信息
}
catch (InvalidOperationException ex)
{
    // 处理目录结构无效的情况
}
catch (FileNotFoundException ex)
{
    // 处理文件缺失的情况
}
```

3. 批量操作示例：

```csharp
// 加载所有档案
var profiles = await _profileService.LoadProfilesDefaultAsync();

// 批量处理
foreach (var profile in profiles)
{
    // 处理每个档案
}
```

4. 保存前验证：

```csharp
if (Directory.Exists(targetDir))
{
    await _profileService.SaveProfilesAsync(targetDir, profile);
}
```
