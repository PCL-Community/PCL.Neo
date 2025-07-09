# ProfileService

ProfileService 是一个用于管理 Minecraft 游戏的服务，它实现了 IProfileService 接口。该服务提供了加载、保存和管理游戏配置文件的功能。

## 特性

- 支持从指定目录加载.minecraft
- 支持保存配置文件到指定目录或默认目录
- 管理游戏版本信息和配置
- 支持删除游戏版本
- 提供异步操作支持

## 使用方法

### 依赖注入设置

在应用程序启动时，通过依赖注入注册 ProfileService：

```csharp
services.AddSingleton<IProfileService, ProfileService>();
```

### 基本使用

1. 从默认目录加载所有配置文件：
```csharp
var profile = await profileService.LoadProfilesDefaultAsync();
```

2. 从指定目录加载所有配置文件：

```csharp
var profileService = new ProfileService();
var profiles = await profileService.LoadProfilesAsync(profilePath);
```

3. 加载特定版本的游戏：

```csharp
var gameInfo = await profileService.LoadTargetGameAsync(targetDir, "1.20.6-Fabric");
```

4. 保存配置文件：

```csharp
var profile = new ProfileInfo 
{ 
    ProfileName = "MyProfile",
    TargetDir = targetDir,
    Games = new List<GameInfo>()
};
await profileService.SaveProfilesAsync(targetDir, profile);
```

5. 保存到默认位置（%AppData%/PCL.Neo/profiles）：

```csharp
await profileService.SaveProfilesDefaultAsync(profile);
```

6. 添加游戏到配置文件：

```csharp
var game = new GameInfo
{
    Name = "1.20.6",
    RootDirectory = targetDir,
    GameDirectory = Path.Combine(targetDir, "versions", "1.20.6"),
    IsIndie = true,
    Loader = GameType.Vanilla
};
await profileService.SaveGameInfoToProfileDefaultAsync(profile, game);
```

7. 删除游戏：

```csharp
var success = profileService.DeleteGame(game, profile);
```

## 示例

以下是一个完整的使用示例：

```csharp
public async Task ManageGameProfiles()
{
    var profileService = new ProfileService();
    var targetDir = @"C:\Games\Minecraft";

    // 加载现有配置文件
    var profile = await profileService.GetProfileAsync(targetDir, "Template");
    
    // 添加新游戏版本
    var newGame = new GameInfo
    {
        Name = "1.20.6-Fabric",
        RootDirectory = targetDir,
        GameDirectory = Path.Combine(targetDir, "versions", "1.20.6-Fabric"),
        IsIndie = true,
        Loader = GameType.Fabric
    };
    
    // 保存到配置文件
    await profileService.SaveGameInfoToProfileDefaultAsync(profile, newGame);
    
    // 保存整个配置文件
    await profileService.SaveProfilesDefaultAsync(profile);
}
```

## 测试示例

请参考 `ProfileServiceTest.cs` 中的单元测试示例：

```csharp
[Test]
public async Task LoadProfileAsync_ShouldLoadAllGames()
{
    var profile = await _service.GetProfileAsync(TempDir, "Template");
    Assert.That(profile, Is.Not.Null);
    Assert.That(profile.Games.Count, Is.EqualTo(29));
    await _service.SaveProfilesDefaultAsync(profile);
}

[Test]
public async Task SaveGameInfoToProfileDefaultAsync_AddsGame()
{
    var profile = new ProfileInfo { 
        ProfileName = "GameProfile", 
        TargetDir = TempDir, 
        Games = [] 
    };
    var game = new GameInfo
    {
        Name = "test",
        RootDirectory = TempDir,
        GameDirectory = "gd",
        IsIndie = false,
        Loader = GameType.Vanilla
    };
    var result = await _service.SaveGameInfoToProfileDefaultAsync(profile, game);
    Assert.That(result, Is.True);
    Assert.That(profile.Games, Has.Count.EqualTo(1));
}
```

## 注意事项

1. 目录验证：
   - 目标目录必须包含 "assets"、"libraries" 和 "versions" 子目录
   - 游戏版本目录必须包含对应的 .jar 和 .json 文件

2. 异常处理：
   - 无效目录会抛出 InvalidOperationException
   - 缺失游戏文件会抛出 FileNotFoundException
   - 缺失游戏目录会抛出 DirectoryNotFoundException

3. 配置文件存储：
   - 默认配置文件存储在 %AppData%/PCL.Neo/profiles 目录
   - 可以指定自定义目录存储配置文件

4. 异步操作：
   - 所有 IO 操作都是异步的，使用 async/await 模式
   - 推荐使用异步方法以避免阻塞主线程

## 配置文件结构

ProfileInfo 结构：
```csharp
public class ProfileInfo
{
    public string ProfileName { get; set; }     // 配置文件名称
    public string TargetDir { get; set; }       // 目标目录
    public List<GameInfo> Games { get; set; }   // 游戏列表
}
```

GameInfo 结构：
```csharp
public record GameInfo
{
    public string GameDirectory { get; set; }   // 游戏目录
    public string RootDirectory { get; set; }   // 根目录
    public string Name { get; set; }            // 游戏版本名
    public GameType Type { get; set; }       // 加载器类型
    public bool IsIndie { get; set; }           // 是否为独立版本
}
```
