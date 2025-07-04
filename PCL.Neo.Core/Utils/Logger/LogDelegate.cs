namespace PCL.Neo.Core.Utils.Logger;

public sealed class LogDelegate
{
    public delegate void OnHintLogEvent(LogEventArgvs argvs);

    public delegate void OnFeedbackLogEvent(LogEventArgvs argvs);

    public delegate void OnDeveloperLogEvent(LogEventArgvs argvs);

    public delegate void OnAssertLogEvent(LogEventArgvs argvs);

    public delegate void OnMsgboxLogEvent(LogEventArgvs argvs);

    public delegate void OnDebugLogEvent(LogEventArgvs argvs);
}

public record LogEventArgvs
{
    public required string Message { get; init; }
    public Exception? Exception { get; init; } = null;
    public DateTimeOffset Timestamp { get; init; } = DateTimeOffset.Now;
}
