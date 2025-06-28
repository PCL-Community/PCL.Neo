namespace PCL.Neo.Utils;

public static class TimeDateUtils
{
    /// <summary>
    /// 获取格式类似于“11:08:52.037”的当前时间的字符串。
    /// </summary>
    public static string GetTimeNow() => DateTime.Now.ToString("HH':'mm':'ss'.'fff");

    /// <summary>
    /// 获取系统运行时间（毫秒），保证为正 Long 且大于 1，但可能突变减小。
    /// </summary>
    public static long GetTimeTick() => Environment.TickCount + 2147483651L;

    public enum UnixTimeType
    {
        Seconds,
        Milliseconds
    }

    /// <summary>
    /// 获取十进制 Unix 时间戳。
    /// </summary>
    public static long GetUnixTimestamp(UnixTimeType type)
    {
        return type switch
        {
            UnixTimeType.Seconds => new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds(),
            UnixTimeType.Milliseconds => new DateTimeOffset(DateTime.UtcNow).ToUnixTimeMilliseconds(),
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
        };
    }

    /// <summary>
    /// 时间戳转化为日期。
    /// </summary>
    public static DateTime GetDate(long timeStamp, UnixTimeType type)
    {
        return type switch
        {
            UnixTimeType.Seconds => DateTimeOffset.FromUnixTimeSeconds(timeStamp).LocalDateTime,
            UnixTimeType.Milliseconds => DateTimeOffset.FromUnixTimeMilliseconds(timeStamp).LocalDateTime,
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
        };
    }

    /// <summary>
    /// 将 UTC 时间转化为当前时区的时间。
    /// </summary>
    public static DateTime GetLocalTime(DateTime utcDate)
    {
        return TimeZoneInfo.ConvertTimeFromUtc(utcDate, TimeZoneInfo.Local);
    }

    /// <summary>
    /// 将时间间隔转换为类似“5 分 10 秒前”的易于阅读的形式。
    /// </summary>
    public static string GetTimeSpanString(TimeSpan span, bool isShortForm)
    {
        bool isFuture = span.TotalMilliseconds > 0;
        string suffix = isFuture ? "后" : "前";
        TimeSpan absoluteSpan = span.Duration(); // 获取时间间隔的绝对值

        return isShortForm
            ? $"{GetShortFormTimeString(absoluteSpan)}{suffix}"
            : $"{GetLongFormTimeString(absoluteSpan)}{suffix}";
    }

    /// <summary>
    /// 处理简短格式的时间字符串。
    /// </summary>
    private static string GetShortFormTimeString(TimeSpan span)
    {
        int months = span.Days / 30;

        // 按时间单位从长到短判断，优先显示更大单位
        if (months >= 12) return $"{months / 12} 年";
        if (months >= 2) return $"{months} 个月";
        if (span.Days >= 2) return $"{span.Days} 天";
        if (span.Hours >= 1) return $"{span.Hours} 小时";
        if (span.Minutes >= 1) return $"{span.Minutes} 分钟";
        if (span.Seconds >= 1) return $"{span.Seconds} 秒";
        return "1 秒"; // 处理小于1秒的情况
    }

    /// <summary>
    /// 处理详细格式的时间字符串。
    /// </summary>
    private static string GetLongFormTimeString(TimeSpan span)
    {
        int months = span.Days / 30;
        int remainingDays = span.Days % 30;

        // 复合时间单位拼接逻辑
        if (months >= 61) return $"{months / 12} 年";
        if (months >= 12) return CombineUnits(months / 12, "年", months % 12, "个月");
        if (months >= 4) return $"{months} 个月";
        if (months >= 1) return CombineUnits(months, "个月", remainingDays, "天");
        if (span.Days >= 4) return $"{span.Days} 天";
        if (span.Days >= 1) return CombineUnits(span.Days, "天", span.Hours, "小时");
        if (span.Hours >= 10) return $"{span.Hours} 小时";
        if (span.Hours >= 1) return CombineUnits(span.Hours, "小时", span.Minutes, "分钟");
        if (span.Minutes >= 10) return $"{span.Minutes} 分钟";
        if (span.Minutes >= 1) return CombineUnits(span.Minutes, "分", span.Seconds, "秒");
        if (span.Seconds >= 1) return $"{span.Seconds} 秒";
        return "1 秒";
    }

    /// <summary>
    /// 时间单位拼接辅助方法。
    /// </summary>
    private static string CombineUnits(int mainValue, string mainUnit, int remainderValue, string remainderUnit)
    {
        return remainderValue > 0
            ? $"{mainValue} {mainUnit} {remainderValue} {remainderUnit}"
            : $"{mainValue} {mainUnit}";
    }
}