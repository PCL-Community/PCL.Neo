namespace PCL.Neo.Core.Service.Audio;

/// <summary>
/// 音频播放器扩展方法，提供更便捷的API
/// </summary>
public static class AudioPlayerExtensions
{
    /// <summary>
    /// 播放简短音效
    /// </summary>
    /// <param name="audioService">音频服务</param>
    /// <param name="filePath">音频文件路径</param>
    /// <returns>是否成功开始播放</returns>
    public static async Task<bool> PlaySoundEffectAsync(this IAudioService audioService, string filePath)
    {
        if (audioService == null || string.IsNullOrEmpty(filePath))
            return false;
            
        return await audioService.PlayAsync(filePath);
    }
    
    /// <summary>
    /// 播放嵌入资源音效
    /// </summary>
    /// <param name="audioService">音频服务</param>
    /// <param name="resourceStream">资源流</param>
    /// <param name="fileExtension">文件扩展名</param>
    /// <returns>是否成功开始播放</returns>
    public static async Task<bool> PlayEmbeddedSoundAsync(
        this IAudioService audioService, 
        Stream resourceStream, 
        string fileExtension = ".mp3")
    {
        if (audioService == null || resourceStream == null)
            return false;
            
        return await audioService.PlayAsync(resourceStream, fileExtension);
    }
    
    /// <summary>
    /// 同步播放音效（会等待完成）
    /// </summary>
    /// <param name="audioService">音频服务</param>
    /// <param name="filePath">音频文件路径</param>
    /// <param name="timeoutMs">超时时间（毫秒），超过此时间将返回，-1表示无超时</param>
    /// <returns>是否成功完成播放</returns>
    public static async Task<bool> PlaySoundAndWaitAsync(
        this IAudioService audioService, 
        string filePath, 
        int timeoutMs = -1)
    {
        if (audioService == null || string.IsNullOrEmpty(filePath))
            return false;
            
        var completionSource = new TaskCompletionSource<bool>();
        EventHandler? handler = null;
        
        handler = (s, e) =>
        {
            audioService.PlaybackFinished -= handler;
            completionSource.SetResult(true);
        };
        
        audioService.PlaybackFinished += handler;
        
        if (!await audioService.PlayAsync(filePath))
        {
            audioService.PlaybackFinished -= handler;
            return false;
        }
        
        if (timeoutMs > 0)
        {
            // 创建超时任务
            var timeoutTask = Task.Delay(timeoutMs);
            
            // 等待完成播放或超时
            if (await Task.WhenAny(completionSource.Task, timeoutTask) == timeoutTask)
            {
                // 超时，停止播放并返回false
                audioService.PlaybackFinished -= handler;
                await audioService.StopAsync();
                return false;
            }
        }
        else
        {
            // 无超时，等待完成
            await completionSource.Task;
        }
        
        return true;
    }
    
    /// <summary>
    /// 渐变音量
    /// </summary>
    /// <param name="audioService">音频服务</param>
    /// <param name="targetVolume">目标音量</param>
    /// <param name="durationMs">渐变持续时间（毫秒）</param>
    /// <returns>操作任务</returns>
    public static async Task FadeVolumeAsync(
        this IAudioService audioService, 
        float targetVolume, 
        int durationMs = 1000)
    {
        if (audioService == null || durationMs <= 0)
            return;
            
        // 获取当前音量的方法不直接支持，所以我们从0开始渐变
        float currentVolume = 0.0f;
        float startVolume = currentVolume;
        float volumeDiff = targetVolume - startVolume;
        
        // 至少进行10次调整，确保渐变平滑
        int steps = Math.Max(10, durationMs / 50);
        int stepDelayMs = durationMs / steps;
        
        for (int i = 0; i <= steps; i++)
        {
            float progress = (float)i / steps;
            float newVolume = startVolume + (volumeDiff * progress);
            await audioService.SetVolumeAsync(newVolume);
            
            if (i < steps)
                await Task.Delay(stepDelayMs);
        }
    }
} 