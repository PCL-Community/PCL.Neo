# PCL.Neo 音频服务模块

本模块提供了PCL.Neo启动器的声音播放功能，使用跨平台设计支持Windows、macOS和Linux系统。

## 功能特点

- 通用音频接口 `IAudioService` 提供统一的音频控制功能
- 支持播放文件或内存流中的音频
- 支持播放控制（暂停/继续/停止）
- 支持音量控制
- 支持播放完成事件
- 跨平台设计，支持Windows、macOS和Linux

## 目录结构

- `IAudioService.cs` - 音频服务接口
- `AudioOptions.cs` - 音频选项配置类
- `AudioService.cs` - 跨平台基础实现类
- `WindowsAudioService.cs` - Windows平台特定实现
- `MacOsAudioService.cs` - macOS平台特定实现
- `LinuxAudioService.cs` - Linux平台特定实现
- `AudioServiceFactory.cs` - 音频服务工厂类，负责创建适合当前平台的服务实例

## 使用方法

1. 创建音频服务实例

```csharp
// 创建适合当前平台的音频服务
IAudioService audioService = AudioServiceFactory.CreateForCurrentPlatform();

// 或创建特定平台实现
IAudioService windowsAudio = new WindowsAudioService();
```

2. 播放音频文件

```csharp
// 播放音频文件
await audioService.PlayAsync(@"D:\music.mp3");

// 从流播放
using var stream = File.OpenRead(@"D:\music.mp3");
await audioService.PlayAsync(stream);
```

3. 控制播放

```csharp
// 暂停
await audioService.PauseAsync();

// 继续播放
await audioService.ResumeAsync();

// 停止
await audioService.StopAsync();

// 调整音量 (0.0-1.0)
await audioService.SetVolumeAsync(0.5f);
```

4. 监听播放完成事件

```csharp
audioService.PlaybackFinished += (sender, args) =>
{
    // 处理播放完成事件
};
```

5. 释放资源

```csharp
audioService.Dispose();
```

## 平台特定的音频实现

### Windows (WindowsAudioService)

- 使用Windows多媒体命令接口(MCI)实现高级音频控制
- 支持精确的暂停/继续控制
- 通过waveOutSetVolume实现系统级音量控制
- 通过状态监控确保准确的播放完成事件

### macOS (MacOsAudioService)

- 使用macOS原生的afplay命令播放音频
- 通过SIGSTOP和SIGCONT信号控制播放暂停/继续
- 使用AppleScript控制系统音量
- 通过进程监控实现播放完成事件

### Linux (LinuxAudioService)

- 根据文件类型智能选择合适的播放器(mpg123/aplay/mplayer)
- 通过SIGSTOP和SIGCONT信号控制播放暂停/继续
- 支持多种音频系统的音量控制:
  - PulseAudio: 通过pactl命令
  - ALSA: 通过amixer命令
- 自动检测可用的音频系统和命令

## 扩展和自定义

要创建自定义音频服务实现，可以:

1. 继承 `AudioService` 基类并重写以下方法:
  - `StartPlaybackAsync`: 实现音频播放
  - `PausePlaybackAsync`: 实现暂停功能
  - `ResumePlaybackAsync`: 实现继续播放功能
  - `StopPlaybackAsync`: 实现停止播放功能
  - `SetVolumeInternalAsync`: 实现音量控制
  - `OnPlaybackFinished`: 控制播放完成事件的触发

2. 使用受保护的成员进行状态管理:
  - `_currentProcess`: 当前播放进程
  - `LogInfo/LogError/LogDebug`: 记录日志的辅助方法

3. 修改 `AudioServiceFactory` 以使用新的自定义实现。

## 其他注意事项

- 所有音频服务实现都实现了 `IDisposable` 接口，使用完毕后应当调用 `Dispose()` 方法释放资源
- 音频服务默认会创建临时文件夹用于存储从流中读取的音频数据，可通过 `AudioOptions` 配置路径
- 如有日志需求，可通过 `AudioOptions.EnableLogging` 启用日志输出
- 基类 `AudioService` 已具备基本的跨平台能力，但平台特定实现提供了更优的性能和功能
