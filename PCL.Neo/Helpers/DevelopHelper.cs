using PCL.Neo.Utils;
using System;

namespace PCL.Neo.Helpers;

public class DevelopHelper
{
    /// <summary>
    /// 在开始前运行测试。
    /// </summary>
    public static void RunTest()
    {
        #region LoggerTest

        Logger.InitLogger(Const.Path);
        var L = Logger.GetInstance();
        L.SetDelegate(Logger.LogLevel.Feedback, s =>
        {
            Console.WriteLine($"FEEDBACK:{s}");
        });
        L.Log($"Start:{new DateTimeOffset(DateTime.UtcNow).ToUnixTimeMilliseconds()}", Logger.LogLevel.Feedback);
        L.Log(TimeDateUtils.GetTimeSpanString(new TimeSpan(0, 4, 32, 0), false));
        L.Log(TimeDateUtils.GetTimeSpanString(-new TimeSpan(400, 0, 0, 0), false));
        L.Log("FeedBack", Logger.LogLevel.Feedback);
        L.Log($"End:{new DateTimeOffset(DateTime.UtcNow).ToUnixTimeMilliseconds()}", Logger.LogLevel.Feedback);
        Logger.Stop();

        #endregion
    }
}
