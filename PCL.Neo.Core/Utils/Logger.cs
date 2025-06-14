using System.Collections.Concurrent;
using System.Diagnostics;
using System.Text.RegularExpressions;
using static PCL.Neo.Const;

namespace PCL.Neo.Utils;

public class Logger
{
    private const int FlushInterval = 150;

    public enum LogLevel
    {
        /// <summary>
        /// 不提示，只记录日志。
        /// </summary>
        Normal = 0,

        /// <summary>
        /// 只提示开发者。
        /// </summary>
        Developer = 1,

        /// <summary>
        /// 只提示开发者与调试模式用户。
        /// </summary>
        Debug = 2,

        /// <summary>
        /// 弹出提示所有用户。
        /// </summary>
        Hint = 3,

        /// <summary>
        /// 弹窗，不要求反馈。
        /// </summary>
        Msgbox = 4,

        /// <summary>
        /// 弹窗，要求反馈。
        /// </summary>
        Feedback = 5,

        /// <summary>
        /// 弹窗，结束程序。
        /// </summary>
        Assert = 6
    }

    public delegate void LogDelegate(string message);

    private static Logger? _instance;

    private LogDelegate _hintLogDelegate = _ => { };
    private LogDelegate _feedbackLogDelegate = _ => { };
    private LogDelegate _developerLogDelegate = _ => { };
    private LogDelegate _assertLogDelegate = _ => { };
    private LogDelegate _msgboxLogDelegate = _ => { };
    private LogDelegate _debugLogDelegate = _ => { };
    private readonly StreamWriter? _logStream;
    private readonly ConcurrentQueue<string> _logQueue = new();
    private readonly System.Timers.Timer _logTimer;

    public static void InitLogger(string logFilePath)
    {
        if (_instance == null) _instance = new Logger(logFilePath);
    }

    public static Logger GetInstance()
    {
        if (_instance != null) return _instance;
        throw new Exception("Logger not initialized.");
    }

    public static void Stop()
    {
        if (_instance == null) throw new Exception("Logger not initialized.");
        _instance.Flush();
        _instance._logTimer.Stop();
        if (_instance._logStream != null)
        {
            _instance._logStream.Flush();
            _instance._logStream.Dispose();
        }

        _instance = null;
    }

    private Logger(string logFilePath)
    {
        bool isInitSuccess = true;
        _logTimer = new System.Timers.Timer(FlushInterval) { AutoReset = true };
        _logTimer.Elapsed += (_, _) => Flush();
        try
        {
            File.Create($"{logFilePath}Log1.txt").Dispose();
        }
        catch (IOException ex)
        {
            isInitSuccess = false;
            Log(ex, "日志初始化失败（疑似文件占用问题）");
        }
        catch (Exception ex)
        {
            isInitSuccess = false;
            Log(ex, "日志初始化失败", LogLevel.Developer);
        }
        _logTimer.Start();

        if (!isInitSuccess) return;
        try
        {
            _logStream = new StreamWriter($"{logFilePath}Log1.txt");
        }
        catch (Exception ex)
        {
            _logStream = null;
            Log(ex, "日志写入失败", LogLevel.Hint);
        }
    }

    private void Flush()
    {
        if (_logStream != null)
        {
            string? log;
            while (!_logQueue.IsEmpty)
            {
                if (_logQueue.TryDequeue(out log))
                {
                    _logStream.Write(log);
                }
            }
        }
        else
        {
            _logQueue.Clear();
        }
    }

    public void SetDelegate(LogLevel level, LogDelegate logDelegate)
    {
        switch (level)
        {
            case LogLevel.Developer: _developerLogDelegate = logDelegate; break;
            case LogLevel.Debug: _debugLogDelegate = logDelegate; break;
            case LogLevel.Hint: _hintLogDelegate = logDelegate; break;
            case LogLevel.Msgbox: _msgboxLogDelegate = logDelegate; break;
            case LogLevel.Assert: _assertLogDelegate = logDelegate; break;
            case LogLevel.Feedback: _feedbackLogDelegate = logDelegate; break;
        }
    }

    public void Log(string text, LogLevel level = LogLevel.Normal, string title = "出现错误")
    {
        string logText = $"[{TimeDateUtils.GetTimeNow()}] {text}{CrLf}";

        _logQueue.Enqueue(logText);
#if DEBUG
        Debug.Write(logText);
#endif
        string msg = Regex.Replace(text, @"\[[^\]]+?\] ", "");
        switch (level)
        {
#if DEBUG
            case LogLevel.Developer: _developerLogDelegate.Invoke(msg); break;
            case LogLevel.Debug: _debugLogDelegate.Invoke(msg); break;
#else
            case LogLevel.Developer: break;
            case LogLevel.Debug: _debugLogDelegate.Invoke(msg); break; // TODO modedebug
#endif
            case LogLevel.Hint: _hintLogDelegate.Invoke(msg); break;
            case LogLevel.Msgbox: _msgboxLogDelegate.Invoke(msg); break;
            case LogLevel.Feedback: _feedbackLogDelegate.Invoke(msg); break;
            case LogLevel.Assert: _assertLogDelegate.Invoke(msg); break;
        }
    }

    public void Log(Exception ex, string desc, LogLevel level = LogLevel.Debug, string title = "出现错误")
    {
        if (ex is ThreadInterruptedException) return;
        // TODO Exception log
    }
}