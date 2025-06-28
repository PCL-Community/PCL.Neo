namespace PCL.Neo.Core.Service.Audio;

/// <summary>
/// 音频服务接口，提供跨平台的音频播放功能
/// </summary>
public interface IAudioService
{
    /// <summary>
    /// 播放音频文件
    /// </summary>
    /// <param name="filePath">音频文件路径</param>
    /// <returns>播放操作的任务</returns>
    Task<bool> PlayAsync(string filePath);
    
    /// <summary>
    /// 从流播放音频
    /// </summary>
    /// <param name="stream">音频数据流</param>
    /// <param name="fileExtension">文件扩展名（如 .mp3, .wav）</param>
    /// <returns>播放操作的任务</returns>
    Task<bool> PlayAsync(Stream stream, string fileExtension = ".mp3");
    
    /// <summary>
    /// 暂停当前播放
    /// </summary>
    /// <returns>暂停操作的任务</returns>
    Task<bool> PauseAsync();
    
    /// <summary>
    /// 继续播放
    /// </summary>
    /// <returns>继续播放操作的任务</returns>
    Task<bool> ResumeAsync();
    
    /// <summary>
    /// 停止播放
    /// </summary>
    /// <returns>停止播放操作的任务</returns>
    Task<bool> StopAsync();
    
    /// <summary>
    /// 设置音量
    /// </summary>
    /// <param name="volume">音量值（0.0 - 1.0）</param>
    /// <returns>设置音量操作的任务</returns>
    Task<bool> SetVolumeAsync(float volume);
    
    /// <summary>
    /// 当前是否正在播放
    /// </summary>
    bool IsPlaying { get; }
    
    /// <summary>
    /// 当前是否已暂停
    /// </summary>
    bool IsPaused { get; }
    
    /// <summary>
    /// 播放完成事件
    /// </summary>
    event EventHandler PlaybackFinished;
} 