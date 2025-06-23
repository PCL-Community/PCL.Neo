using System;
using System.IO;
using System.Threading.Tasks;

namespace PCL.Neo.Core.Service.Audio;

/// <summary>
/// 音频服务演示类，包含使用音频服务API的示例代码
/// </summary>
public static class AudioDemo
{
    /// <summary>
    /// 演示基本音频播放功能
    /// </summary>
    /// <param name="audioFilePath">音频文件路径</param>
    /// <returns>演示任务</returns>
    public static async Task BasicPlaybackDemo(string audioFilePath)
    {
        // 创建音频服务
        IAudioService audioService = AudioServiceFactory.CreateForCurrentPlatform();

        Console.WriteLine("开始播放音频...");
        bool result = await audioService.PlayAsync(audioFilePath);
        
        if (!result)
        {
            Console.WriteLine("播放失败，可能文件不存在或格式不支持");
            return;
        }
        
        // 等待3秒后暂停
        await Task.Delay(3000);
        Console.WriteLine("暂停播放...");
        await audioService.PauseAsync();
        
        // 等待2秒后恢复播放
        await Task.Delay(2000);
        Console.WriteLine("恢复播放...");
        await audioService.ResumeAsync();
        
        // 等待3秒后设置音量
        await Task.Delay(3000);
        Console.WriteLine("设置音量为50%...");
        await audioService.SetVolumeAsync(0.5f);
        
        // 等待3秒后停止播放
        await Task.Delay(3000);
        Console.WriteLine("停止播放...");
        await audioService.StopAsync();
    }
    
    /// <summary>
    /// 演示使用扩展方法
    /// </summary>
    /// <param name="audioFilePath">音频文件路径</param>
    /// <returns>演示任务</returns>
    public static async Task ExtensionsDemo(string audioFilePath)
    {
        // 创建音频服务
        IAudioService audioService = AudioServiceFactory.CreateForCurrentPlatform();
        
        // 播放音效并等待完成
        Console.WriteLine("播放音效并等待完成...");
        bool result = await audioService.PlaySoundAndWaitAsync(audioFilePath, 10000); // 10秒超时
        
        if (result)
        {
            Console.WriteLine("音效已完成播放");
        }
        else
        {
            Console.WriteLine("音效播放失败或超时");
        }
        
        // 音量渐变示例
        Console.WriteLine("开始播放并渐入音量...");
        await audioService.PlayAsync(audioFilePath);
        await audioService.SetVolumeAsync(0.0f); // 开始时音量为0
        await audioService.FadeVolumeAsync(1.0f, 3000); // 3秒内渐入
        
        await Task.Delay(3000);
        
        Console.WriteLine("渐出音量...");
        await audioService.FadeVolumeAsync(0.0f, 3000); // 3秒内渐出
        
        await Task.Delay(3000);
        await audioService.StopAsync();
    }
    
    /// <summary>
    /// 演示从流播放音频
    /// </summary>
    /// <param name="audioFilePath">用于演示的音频文件路径</param>
    /// <returns>演示任务</returns>
    public static async Task StreamPlaybackDemo(string audioFilePath)
    {
        // 创建音频服务
        IAudioService audioService = AudioServiceFactory.CreateForCurrentPlatform();
        
        // 从文件加载到流中（在实际应用中，这可能来自网络或嵌入资源）
        Console.WriteLine("从流播放音频...");
        using (FileStream stream = File.OpenRead(audioFilePath))
        {
            // 从流播放
            string extension = Path.GetExtension(audioFilePath);
            bool result = await audioService.PlayAsync(stream, extension);
            
            if (!result)
            {
                Console.WriteLine("从流播放失败");
                return;
            }
            
            // 等待10秒或播放完成
            Console.WriteLine("等待播放完成...");
            var completionSource = new TaskCompletionSource<bool>();
            
            EventHandler? handler = null;
            handler = (s, e) =>
            {
                audioService.PlaybackFinished -= handler;
                completionSource.SetResult(true);
            };
            
            audioService.PlaybackFinished += handler;
            
            // 10秒超时或等待播放完成
            var timeoutTask = Task.Delay(10000);
            await Task.WhenAny(completionSource.Task, timeoutTask);
            
            audioService.PlaybackFinished -= handler;
            await audioService.StopAsync();
        }
    }
} 