using Microsoft.Extensions.Configuration;
using Serilog;

namespace PCL.Neo.Core.Utils.Logger;

public sealed class NewLogger : IDisposable
{
    private readonly Serilog.Core.Logger _logger;

    public event LogDelegate.OnAssertLogEvent? OnAssertLogEvent;
    public event LogDelegate.OnDebugLogEvent? OnDebugLogEvent;
    public event LogDelegate.OnDeveloperLogEvent? OnDeveloperLogEvent;
    public event LogDelegate.OnFeedbackLogEvent? OnFeedbackLogEvent;
    public event LogDelegate.OnHintLogEvent? OnHintLogEvent;
    public event LogDelegate.OnMsgboxLogEvent? OnMessageBoxLogEvent;

    public enum LogLevel
    {
        Debug,
        Developer,
        Hint,
        MsgBox,
        Feedback,
        Assert,
        None
    }

    public NewLogger(string targetLogDir)
    {
        if (Directory.Exists(targetLogDir) == false)
        {
            Directory.CreateDirectory(targetLogDir);
        }

        var configuration = new ConfigurationBuilder()
            .SetBasePath(Const.PathWithoutName)
            .AddJsonFile("loggersettings.json")
            .Build();

        _logger = new LoggerConfiguration()
            .ReadFrom.Configuration(configuration)
            .CreateLogger();

        Log.Logger = _logger;
    }

    ~NewLogger()
    {
        Log.CloseAndFlush();
    }

    private void Announce(LogLevel level, string message, Exception? ex)
    {
        switch (level)
        {
            case LogLevel.None:
                break;
            case LogLevel.MsgBox:
                OnMessageBoxLogEvent?.Invoke(new LogEventArgvs { Message = message, Exception = ex });
                break;
            case LogLevel.Hint:

                OnHintLogEvent?.Invoke(new LogEventArgvs { Message = message, Exception = ex });
                break;
            case LogLevel.Feedback:
                OnFeedbackLogEvent?.Invoke(new LogEventArgvs { Message = message, Exception = ex });
                break;
#if DEBUG // in debug mode

            case LogLevel.Debug:
                OnDebugLogEvent?.Invoke(new LogEventArgvs { Message = message, Exception = ex });
                break;
            case LogLevel.Developer:
                OnDeveloperLogEvent?.Invoke(new LogEventArgvs { Message = message, Exception = ex });
                break;
#else // not in debug mode
            case LogLevel.Debug:
            case LogLevel.Developer:
                break;
#endif

            case LogLevel.Assert:
                OnAssertLogEvent?.Invoke(new LogEventArgvs { Message = message, Exception = ex });
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(level), level, null);
        }
    }

    #region Logger

    public void LogDebug(string message, Exception? ex = null, LogLevel level = LogLevel.None)
    {
        _logger.Debug(ex, message);
        Announce(level, message, ex);
    }

    public void LogInformation(string message, Exception? ex = null, LogLevel level = LogLevel.None)
    {
        _logger.Information(ex, message);
        Announce(level, message, ex);
    }

    public void LogWarning(string message, Exception? ex = null, LogLevel level = LogLevel.None)
    {
        _logger.Warning(ex, message);
        Announce(level, message, ex);
    }

    public void LogError(string message, Exception? ex = null, LogLevel level = LogLevel.None)
    {
        _logger.Error(ex, message);
        Announce(level, message, ex);
    }

    public void LogFatal(string message, Exception? ex = null, LogLevel level = LogLevel.None)
    {
        _logger.Fatal(ex, message);
        Announce(level, message, ex);
    }

    #endregion

    /// <inheritdoc />
    public void Dispose()
    {
        _logger.Dispose();
    }

    public static readonly NewLogger Logger = new(Path.Combine(Const.PathWithoutName, "logs"));
}
