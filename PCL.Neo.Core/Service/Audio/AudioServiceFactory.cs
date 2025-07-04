using PCL.Neo.Core.Utils;

namespace PCL.Neo.Core.Service.Audio;

/// <summary>
/// 音频服务工厂，负责创建适合当前平台的音频服务实例
/// </summary>
public static class AudioServiceFactory
{
    /// <summary>
    /// 创建适合当前平台的音频服务实例
    /// </summary>
    /// <param name="options">音频选项配置</param>
    /// <returns>音频服务接口实例</returns>
    public static IAudioService CreateForCurrentPlatform(AudioOptions? options = null)
    {
        options ??= new AudioOptions();

        // 根据当前操作系统创建相应的实现
        return SystemUtils.Os switch
        {
            SystemUtils.RunningOs.Windows => new WindowsAudioService(options),
            SystemUtils.RunningOs.MacOs => new MacOsAudioService(options),
            SystemUtils.RunningOs.Linux => new LinuxAudioService(options),
            _ => new AudioService(options) // 默认情况下使用基本实现
        };
    }

    /// <summary>
    /// 检查系统是否支持音频播放
    /// </summary>
    /// <returns>是否支持音频播放</returns>
    public static bool IsAudioSupported()
    {
        // 目前所有支持的平台都可以播放音频
        return true;
    }
}
