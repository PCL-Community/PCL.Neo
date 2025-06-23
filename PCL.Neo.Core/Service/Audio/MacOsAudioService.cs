using System;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PCL.Neo.Core.Service.Audio;

/// <summary>
/// macOS平台特定的音频服务实现，使用原生命令进行音频控制
/// </summary>
public class MacOsAudioService : AudioService
{
    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="options">音频选项</param>
    public MacOsAudioService(AudioOptions? options = null) : base(options)
    {
    }

    /// <summary>
    /// 执行macOS shell命令
    /// </summary>
    /// <param name="command">命令</param>
    /// <param name="arguments">参数</param>
    /// <returns>命令输出结果</returns>
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
    /// 执行AppleScript
    /// </summary>
    /// <param name="script">AppleScript脚本内容</param>
    /// <returns>脚本输出结果</returns>
    private string ExecuteAppleScript(string script)
    {
        return ExecuteCommand("osascript", $"-e '{script}'");
    }

    /// <summary>
    /// 开始播放（macOS特定实现）
    /// </summary>
    /// <param name="filePath">音频文件路径</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <remarks>
    /// 此实现使用macOS的afplay命令播放音频文件，并通过监控进程状态来触发完成事件。
    /// </remarks>
    protected override async Task<bool> StartPlaybackAsync(string filePath, CancellationToken cancellationToken)
    {
        try
        {
            return await Task.Run(() =>
            {
                LogInfo($"macOS播放文件: {filePath}");
                var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "afplay",
                        Arguments = $"\"{filePath}\"",
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
                    LogError("无法启动afplay进程");
                    return false;
                }

                // 储存当前进程到基类的protected成员
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
            LogError("macOS音频播放出错", ex);
            return false;
        }
    }

    /// <summary>
    /// 暂停播放（macOS特定实现）
    /// </summary>
    /// <remarks>
    /// macOS平台使用SIGSTOP信号暂停音频播放进程。
    /// </remarks>
    protected override Task<bool> PausePlaybackAsync()
    {
        return Task.Run(() =>
        {
            if (_currentProcess == null || _currentProcess.HasExited)
                return false;
            
            try
            {
                LogInfo("macOS暂停播放");
                int pid = _currentProcess.Id;
                // 发送SIGSTOP信号暂停进程
                ExecuteCommand("kill", $"-STOP {pid}");
                return true;
            }
            catch (Exception ex)
            {
                LogError("macOS暂停播放出错", ex);
                return false;
            }
        });
    }

    /// <summary>
    /// 继续播放（macOS特定实现）
    /// </summary>
    /// <remarks>
    /// macOS平台使用SIGCONT信号恢复音频播放进程。
    /// </remarks>
    protected override Task<bool> ResumePlaybackAsync()
    {
        return Task.Run(() =>
        {
            if (_currentProcess == null || _currentProcess.HasExited)
                return false;
            
            try
            {
                LogInfo("macOS继续播放");
                int pid = _currentProcess.Id;
                // 发送SIGCONT信号继续进程
                ExecuteCommand("kill", $"-CONT {pid}");
                return true;
            }
            catch (Exception ex)
            {
                LogError("macOS继续播放出错", ex);
                return false;
            }
        });
    }

    /// <summary>
    /// 停止播放（macOS特定实现）
    /// </summary>
    protected override Task<bool> StopPlaybackAsync()
    {
        return Task.Run(() =>
        {
            if (_currentProcess == null)
                return true;
                
            try
            {
                LogInfo("macOS停止播放");
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
                LogError("macOS停止播放出错", ex);
                return false;
            }
        });
    }

    /// <summary>
    /// 设置音量（macOS特定实现）
    /// </summary>
    /// <param name="volume">音量值（0.0 - 1.0）</param>
    /// <remarks>
    /// 使用AppleScript设置macOS的系统音量。
    /// </remarks>
    protected override Task<bool> SetVolumeInternalAsync(float volume)
    {
        return Task.Run(() =>
        {
            try
            {
                // 将0-1范围的音量映射到macOS的0-100范围
                int osVolume = (int)(volume * 100);
                LogInfo($"设置macOS系统音量: {osVolume}%");
                
                // 使用AppleScript设置系统音量
                string script = $"set volume output volume {osVolume}";
                ExecuteAppleScript(script);
                return true;
            }
            catch (Exception ex)
            {
                LogError("设置macOS音量时出错", ex);
                return false;
            }
        });
    }
} 