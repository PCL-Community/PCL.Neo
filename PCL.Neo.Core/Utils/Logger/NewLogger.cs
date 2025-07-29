using UltimateLogSystem;
using UltimateLogSystem.Formatters;

namespace PCL.Neo.Core.Utils.Logger;

public sealed class NewLogger : IDisposable
{
    private readonly UltimateLogSystem.Logger _logger;

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

    private NewLogger()
    {
        var logFilePaht = Path.Combine(Const.AppData, "logs");

#if DEBUG
        var confgig = new LoggerConfiguration()
            .SetMinimumLevel(UltimateLogSystem.LogLevel.Trace)
            .SetDefaultCategory("PCL.Neo")
            .AddConsoleWriter(new TextFormatter("[{timestamp}] [{level}] {message}"))
            .AddFileWriterWithDailyRolling($"{logFilePaht}/log.log",
                maxFileSize: 40 * 1024 * 1024,
                maxRollingFiles: 50,
                formatter: new TextFormatter("[{timestamp}] [{level}] {message}"),
                useDailyRolling: true);
#else
        var confgig = new LoggerConfiguration()
            .SetMinimumLevel(UltimateLogSystem.LogLevel.Warning)
            .SetDefaultCategory("PCL.Neo")
            .AddConsoleWriter(new TextFormatter("[{timestamp}] [{level}] {message}"))
            .AddFileWriterWithDailyRolling($"{logFilePaht}/log.log",
                maxFileSize: 10 * 1024 * 1024,
                maxRollingFiles: 5,
                formatter: new TextFormatter("[{timestamp}] [{level}] {message}"),
                useDailyRolling: true);
#endif


        _logger = LoggerFactory.CreateLogger(confgig);
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
        _logger.Debug(message, exception: ex);
        Announce(level, message, ex);
    }

    public void LogInformation(string message, Exception? ex = null, LogLevel level = LogLevel.None)
    {
        _logger.Info(message, exception: ex);
        Announce(level, message, ex);
    }

    public void LogWarning(string message, Exception? ex = null, LogLevel level = LogLevel.None)
    {
        _logger.Warning(message, exception: ex);
        Announce(level, message, ex);
    }

    public void LogError(string message, Exception? ex = null, LogLevel level = LogLevel.None)
    {
        _logger.Error(message, exception: ex);
        Announce(level, message, ex);
    }

    public void LogFatal(string message, Exception? ex = null, LogLevel level = LogLevel.None)
    {
        _logger.Fatal(message, exception: ex);
        Announce(level, message, ex);
    }

    #endregion

    /// <inheritdoc />
    public void Dispose()
    {
        _logger.Dispose();
    }

    public static readonly NewLogger Logger = new();
}
