using System;
using System.Threading;

namespace PCL.Neo.Utils;

public static class CoreUtils
{
    private static int _uuid = 1;

    public static int GetUuid()
    {
        return Interlocked.Increment(ref _uuid);
    }

    /// <summary>
    /// 指示接取到这个异常的函数进行重试。
    /// </summary>
    public class RestartException : Exception;

    /// <summary>
    /// 指示用户手动取消了操作，或用户已知晓操作被取消的原因。
    /// </summary>
    public class CancelledException : Exception;

    public enum LoadState
    {
        Waiting,
        Loading,
        Finished,
        Failed,
        Aborted
    }

    public enum Result
    {
        /// <summary>
        /// 执行成功，或进程被中断。
        /// </summary>
        Aborted = -1,

        /// <summary>
        /// 执行成功。
        /// </summary>
        Success = 0,

        /// <summary>
        /// 执行失败。
        /// </summary>
        Fail,

        /// <summary>
        /// 执行时出现未经处理的异常。
        /// </summary>
        Exception,

        /// <summary>
        /// 执行超时。
        /// </summary>
        Timeout,

        /// <summary>
        /// 取消执行。可能是由于不满足执行的前置条件。
        /// </summary>
        Cancel
    }
}
