using System;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.IO;

namespace PCL.Neo.Core.Service.Audio;

/// <summary>
/// Linux平台特定的音频服务实现，支持不同的音频系统(PulseAudio/ALSA)
/// </summary>
public class LinuxAudioService : AudioService
{
    private readonly bool _hasPulseAudio;
    private readonly bool _hasAlsa;
    
    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="options">音频选项</param>
    public LinuxAudioService(AudioOptions? options = null) : base(options)
    {
        // 检测音频系统
        _hasPulseAudio = CheckCommand("pactl", "--version");
        _hasAlsa = CheckCommand("amixer", "--version");
        
        LogInfo($"Linux音频系统初始化: PulseAudio={_hasPulseAudio}, ALSA={_hasAlsa}");
    }
    
    /// <summary>
    /// 检查命令是否可用
    /// </summary>
    private bool CheckCommand(string command, string arguments = "")
    {
        try
        {
            using var process = new Process();
            process.StartInfo = new ProcessStartInfo
            {
                FileName = command,
                Arguments = arguments,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };
            
            process.Start();
            process.WaitForExit(1000); // 等待最多1秒
            return process.ExitCode == 0;
        }
        catch
        {
            return false;
        }
    }
    
    /// <summary>
    /// 执行Shell命令
    /// </summary>
    private string ExecuteCommand(string command, string arguments)
    {
        try
        {
            using var process = new Process();
            process.StartInfo = new ProcessStartInfo
            {
                FileName = command,
                Arguments = arguments,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };
            
            process.Start();
            string output = process.StandardOutput.ReadToEnd();
            process.WaitForExit();
            
            return output.Trim();
        }
        catch (Exception ex)
        {
            LogError($"执行命令时出错: {command} {arguments}", ex);
            return string.Empty;
        }
    }
    
    /// <summary>
    /// 开始播放（Linux特定实现）
    /// </summary>
    /// <param name="filePath">音频文件路径</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <remarks>
    /// 此实现根据文件类型选择合适的Linux播放器程序：
    /// - 对MP3文件优先使用mpg123
    /// - 对WAV文件使用aplay
    /// </remarks>
    protected override async Task<bool> StartPlaybackAsync(string filePath, CancellationToken cancellationToken)
    {
        try
        {
            return await Task.Run(() =>
            {
                // 根据文件扩展名选择播放器
                string fileExtension = Path.GetExtension(filePath).ToLowerInvariant();
                string playerCommand;
                string playerArgs;
                
                if (fileExtension == ".mp3")
                {
                    if (CheckCommand("mpg123", "--version"))
                    {
                        playerCommand = "mpg123";
                        playerArgs = $"\"{filePath}\"";
                    }
                    else if (CheckCommand("mplayer", "-v"))
                    {
                        playerCommand = "mplayer";
                        playerArgs = $"-really-quiet \"{filePath}\"";
                    }
                    else
                    {
                        LogError("Linux系统缺少MP3播放器（需要mpg123或mplayer）");
                        return false;
                    }
                }
                else
                {
                    if (CheckCommand("aplay", "--version"))
                    {
                        playerCommand = "aplay";
                        playerArgs = $"\"{filePath}\"";
                    }
                    else if (CheckCommand("mplayer", "-v"))
                    {
                        playerCommand = "mplayer";
                        playerArgs = $"-really-quiet \"{filePath}\"";
                    }
                    else
                    {
                        LogError("Linux系统缺少音频播放器（需要aplay或mplayer）");
                        return false;
                    }
                }
                
                LogInfo($"Linux播放文件: {filePath}，使用: {playerCommand}");
                
                var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = playerCommand,
                        Arguments = playerArgs,
                        UseShellExecute = false,
                        CreateNoWindow = true,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true
                    },
                    EnableRaisingEvents = true
                };
                
                process.Exited += (sender, args) =>
                {
                    OnPlaybackFinished(this);
                    process.Dispose();
                };

                bool started = process.Start();
                if (!started)
                {
                    LogError($"无法启动{playerCommand}进程");
                    return false;
                }

                // 储存当前进程
                _currentProcess = process;
                
                // 创建取消令牌链接
                cancellationToken.Register(() =>
                {
                    try
                    {
                        if (!process.HasExited)
                        {
                            process.Kill();
                        }
                    }
                    catch { /* 忽略错误 */ }
                });
                
                return true;
            });
        }
        catch (Exception ex)
        {
            LogError("Linux音频播放出错", ex);
            return false;
        }
    }

    /// <summary>
    /// 暂停播放（Linux特定实现）
    /// </summary>
    /// <remarks>
    /// Linux平台使用SIGSTOP信号暂停音频播放进程
    /// </remarks>
    protected override Task<bool> PausePlaybackAsync()
    {
        return Task.Run(() =>
        {
            if (_currentProcess == null || _currentProcess.HasExited)
                return false;
            
            try
            {
                LogInfo("Linux暂停播放");
                int pid = _currentProcess.Id;
                // 发送SIGSTOP信号暂停进程
                ExecuteCommand("kill", $"-STOP {pid}");
                return true;
            }
            catch (Exception ex)
            {
                LogError("Linux暂停播放出错", ex);
                return false;
            }
        });
    }

    /// <summary>
    /// 继续播放（Linux特定实现）
    /// </summary>
    /// <remarks>
    /// Linux平台使用SIGCONT信号恢复音频播放进程
    /// </remarks>
    protected override Task<bool> ResumePlaybackAsync()
    {
        return Task.Run(() =>
        {
            if (_currentProcess == null || _currentProcess.HasExited)
                return false;
            
            try
            {
                LogInfo("Linux继续播放");
                int pid = _currentProcess.Id;
                // 发送SIGCONT信号继续进程
                ExecuteCommand("kill", $"-CONT {pid}");
                return true;
            }
            catch (Exception ex)
            {
                LogError("Linux继续播放出错", ex);
                return false;
            }
        });
    }

    /// <summary>
    /// 停止播放（Linux特定实现）
    /// </summary>
    protected override Task<bool> StopPlaybackAsync()
    {
        return Task.Run(() =>
        {
            if (_currentProcess == null)
                return true;
                
            try
            {
                LogInfo("Linux停止播放");
                if (!_currentProcess.HasExited)
                {
                    _currentProcess.Kill();
                }
                _currentProcess.Dispose();
                _currentProcess = null;
                return true;
            }
            catch (Exception ex)
            {
                LogError("Linux停止播放出错", ex);
                return false;
            }
        });
    }

    /// <summary>
    /// 设置音量（Linux特定实现）
    /// </summary>
    /// <param name="volume">音量值（0.0 - 1.0）</param>
    /// <remarks>
    /// 根据系统情况使用不同的音量控制方式：
    /// - 优先使用PulseAudio (pactl)
    /// - 其次使用ALSA (amixer)
    /// </remarks>
    protected override Task<bool> SetVolumeInternalAsync(float volume)
    {
        return Task.Run(() =>
        {
            try
            {
                // 将0-1范围的音量映射到0-100%
                int volumePercent = (int)(volume * 100);
                LogInfo($"设置Linux音量: {volumePercent}%");
                
                if (_hasPulseAudio)
                {
                    // 使用PulseAudio设置音量
                    ExecuteCommand("pactl", $"set-sink-volume @DEFAULT_SINK@ {volumePercent}%");
                    return true;
                }
                else if (_hasAlsa)
                {
                    // 使用ALSA设置音量
                    ExecuteCommand("amixer", $"set Master {volumePercent}% unmute");
                    return true;
                }
                else
                {
                    LogError("Linux系统未找到支持的音量控制方法");
                    return false;
                }
            }
            catch (Exception ex)
            {
                LogError("设置Linux音量时出错", ex);
                return false;
            }
        });
    }
} 