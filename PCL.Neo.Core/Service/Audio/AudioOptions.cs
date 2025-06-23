namespace PCL.Neo.Core.Service.Audio;

/// <summary>
/// 音频选项配置类
/// </summary>
public class AudioOptions
{
    /// <summary>
    /// 默认音量 (0.0 - 1.0)
    /// </summary>
    public float DefaultVolume { get; set; } = 0.7f;
    
    /// <summary>
    /// 是否启用音频系统
    /// </summary>
    public bool EnableAudio { get; set; } = true;
    
    /// <summary>
    /// 音频临时文件目录
    /// </summary>
    public string? TempDirectory { get; set; }
    
    /// <summary>
    /// 音频缓冲区大小（字节）
    /// </summary>
    public int BufferSize { get; set; } = 16384;
    
    /// <summary>
    /// 是否记录日志
    /// </summary>
    public bool EnableLogging { get; set; } = false;
    
    /// <summary>
    /// 音频错误重试次数
    /// </summary>
    public int RetryCount { get; set; } = 3;
    
    /// <summary>
    /// 音频错误重试延迟（毫秒）
    /// </summary>
    public int RetryDelayMs { get; set; } = 500;
} 