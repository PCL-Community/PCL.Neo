namespace PCL.Neo.Core.Service.Accounts
{
    /// <summary>
    /// 日志级别
    /// </summary>
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

/// <summary>
/// 为账户服务提供日志记录扩展方法
/// </summary>
public static class LoggingExtensions
{
    private const string AccountServicePrefix = "[账户服务] ";
    private const string AuthServicePrefix = "[认证服务] ";
    private const string YggdrasilPrefix = "[外置登录] ";
    private const string MicrosoftPrefix = "[微软认证] ";
    private const string ProfileServicePrefix = "[档案服务] ";

    /// <summary>
    /// 记录账户服务调试信息
    /// </summary>
    public static void LogAccountDebug(this object service, string message)
    {
        try
        {
            Console.WriteLine(AccountServicePrefix + message);
            // 在实际环境中替换为真实日志调用
        }
        catch
        {
            // 如果日志系统初始化失败或不可用，则忽略
        }
    }

    /// <summary>
    /// 记录账户服务信息
    /// </summary>
    public static void LogAccountInfo(this object service, string message)
    {
        try
        {
            Console.WriteLine(AccountServicePrefix + message);
            // 在实际环境中替换为真实日志调用
        }
        catch
        {
            // 如果日志系统初始化失败或不可用，则忽略
        }
    }

    /// <summary>
    /// 记录账户服务警告
    /// </summary>
    public static void LogAccountWarning(this object service, string message)
    {
        try
        {
            Console.WriteLine(AccountServicePrefix + message);
            // 在实际环境中替换为真实日志调用
        }
        catch
        {
            // 如果日志系统初始化失败或不可用，则忽略
        }
    }

    /// <summary>
    /// 记录账户服务警告（带异常信息）
    /// </summary>
    public static void LogAccountWarning(this object service, string message, Exception? exception)
    {
        try
        {
            if (exception != null)
            {
                Console.WriteLine($"{AccountServicePrefix}{message}: {exception.Message}");
                Console.WriteLine(exception.StackTrace);
            }
            else
            {
                Console.WriteLine(AccountServicePrefix + message);
            }
            // 在实际环境中替换为真实日志调用
        }
        catch
        {
            // 如果日志系统初始化失败或不可用，则忽略
        }
    }

    /// <summary>
    /// 记录账户服务错误
    /// </summary>
    public static void LogAccountError(this object service, string message, Exception? exception = null)
    {
        try
        {
            if (exception != null)
            {
                Console.WriteLine($"{AccountServicePrefix}{message}: {exception.Message}");
                Console.WriteLine(exception.StackTrace);
            }
            else
            {
                Console.WriteLine(AccountServicePrefix + message);
            }
            // 在实际环境中替换为真实日志调用
        }
        catch
        {
            // 如果日志系统初始化失败或不可用，则忽略
        }
    }

    /// <summary>
    /// 记录认证服务调试信息
    /// </summary>
    public static void LogAuthDebug(this object service, string message)
    {
        try
        {
            Console.WriteLine(AuthServicePrefix + message);
            // 在实际环境中替换为真实日志调用
        }
        catch
        {
            // 如果日志系统初始化失败或不可用，则忽略
        }
    }

    /// <summary>
    /// 记录认证服务信息
    /// </summary>
    public static void LogAuthInfo(this object service, string message)
    {
        try
        {
            Console.WriteLine(AuthServicePrefix + message);
            // 在实际环境中替换为真实日志调用
        }
        catch
        {
            // 如果日志系统初始化失败或不可用，则忽略
        }
    }

    /// <summary>
    /// 记录认证服务警告
    /// </summary>
    public static void LogAuthWarning(this object service, string message)
    {
        try
        {
            Console.WriteLine(AuthServicePrefix + message);
            // 在实际环境中替换为真实日志调用
        }
        catch
        {
            // 如果日志系统初始化失败或不可用，则忽略
        }
    }

    /// <summary>
    /// 记录认证服务错误
    /// </summary>
    public static void LogAuthError(this object service, string message, Exception? exception = null)
    {
        try
        {
            if (exception != null)
            {
                Console.WriteLine($"{AuthServicePrefix}{message}: {exception.Message}");
                Console.WriteLine(exception.StackTrace);
            }
            else
            {
                Console.WriteLine(AuthServicePrefix + message);
            }
            // 在实际环境中替换为真实日志调用
        }
        catch
        {
            // 如果日志系统初始化失败或不可用，则忽略
        }
    }

    /// <summary>
    /// 记录微软认证调试信息
    /// </summary>
    public static void LogMicrosoftDebug(this object service, string message)
    {
        try
        {
            Console.WriteLine(MicrosoftPrefix + message);
            // 在实际环境中替换为真实日志调用
        }
        catch
        {
            // 如果日志系统初始化失败或不可用，则忽略
        }
    }

    /// <summary>
    /// 记录微软认证信息
    /// </summary>
    public static void LogMicrosoftInfo(this object service, string message)
    {
        try
        {
            Console.WriteLine(MicrosoftPrefix + message);
            // 在实际环境中替换为真实日志调用
        }
        catch
        {
            // 如果日志系统初始化失败或不可用，则忽略
        }
    }

    /// <summary>
    /// 记录微软认证警告
    /// </summary>
    public static void LogMicrosoftWarning(this object service, string message)
    {
        try
        {
            Console.WriteLine(MicrosoftPrefix + message);
            // 在实际环境中替换为真实日志调用
        }
        catch
        {
            // 如果日志系统初始化失败或不可用，则忽略
        }
    }

    /// <summary>
    /// 记录微软认证错误
    /// </summary>
    public static void LogMicrosoftError(this object service, string message, Exception? exception = null)
    {
        try
        {
            if (exception != null)
            {
                Console.WriteLine($"{MicrosoftPrefix}{message}: {exception.Message}");
                Console.WriteLine(exception.StackTrace);
            }
            else
            {
                Console.WriteLine(MicrosoftPrefix + message);
            }
            // 在实际环境中替换为真实日志调用
        }
        catch
        {
            // 如果日志系统初始化失败或不可用，则忽略
        }
    }

    /// <summary>
    /// 记录外置登录调试信息
    /// </summary>
    public static void LogYggdrasilDebug(this object service, string message)
    {
        try
        {
            Console.WriteLine(YggdrasilPrefix + message);
            // 在实际环境中替换为真实日志调用
        }
        catch
        {
            // 如果日志系统初始化失败或不可用，则忽略
        }
    }

    /// <summary>
    /// 记录外置登录信息
    /// </summary>
    public static void LogYggdrasilInfo(this object service, string message)
    {
        try
        {
            Console.WriteLine(YggdrasilPrefix + message);
            // 在实际环境中替换为真实日志调用
        }
        catch
        {
            // 如果日志系统初始化失败或不可用，则忽略
        }
    }

    /// <summary>
    /// 记录外置登录警告
    /// </summary>
    public static void LogYggdrasilWarning(this object service, string message)
    {
        try
        {
            Console.WriteLine(YggdrasilPrefix + message);
            // 在实际环境中替换为真实日志调用
        }
        catch
        {
            // 如果日志系统初始化失败或不可用，则忽略
        }
    }

    /// <summary>
    /// 记录外置登录错误
    /// </summary>
    public static void LogYggdrasilError(this object service, string message, Exception? exception = null)
    {
        try
        {
            if (exception != null)
            {
                Console.WriteLine($"{YggdrasilPrefix}{message}: {exception.Message}");
                Console.WriteLine(exception.StackTrace);
            }
            else
            {
                Console.WriteLine(YggdrasilPrefix + message);
            }
            // 在实际环境中替换为真实日志调用
        }
        catch
        {
            // 如果日志系统初始化失败或不可用，则忽略
        }
    }

    /// <summary>
    /// 记录档案服务调试信息
    /// </summary>
    public static void LogProfileDebug(this object service, string message)
    {
        try
        {
            Console.WriteLine(ProfileServicePrefix + message);
            // 在实际环境中替换为真实日志调用
        }
        catch
        {
            // 如果日志系统初始化失败或不可用，则忽略
        }
    }

    /// <summary>
    /// 记录档案服务信息
    /// </summary>
    public static void LogProfileInfo(this object service, string message)
    {
        try
        {
            Console.WriteLine(ProfileServicePrefix + message);
            // 在实际环境中替换为真实日志调用
        }
        catch
        {
            // 如果日志系统初始化失败或不可用，则忽略
        }
    }

    /// <summary>
    /// 记录档案服务警告
    /// </summary>
    public static void LogProfileWarning(this object service, string message)
    {
        try
        {
            Console.WriteLine(ProfileServicePrefix + message);
            // 在实际环境中替换为真实日志调用
        }
        catch
        {
            // 如果日志系统初始化失败或不可用，则忽略
        }
    }

    /// <summary>
    /// 记录档案服务错误
    /// </summary>
    public static void LogProfileError(this object service, string message, Exception? exception = null)
    {
        try
        {
            if (exception != null)
            {
                Console.WriteLine($"{ProfileServicePrefix}{message}: {exception.Message}");
                Console.WriteLine(exception.StackTrace);
            }
            else
            {
                Console.WriteLine(ProfileServicePrefix + message);
            }
            // 在实际环境中替换为真实日志调用
        }
        catch
        {
            // 如果日志系统初始化失败或不可用，则忽略
        }
    }

    /// <summary>
    /// 记录异常并抛出，确保错误被记录但传播出去
    /// </summary>
    public static void LogAndRethrow(this object service, Exception exception, string message,
        LogLevel level = LogLevel.Msgbox)
    {
        try
        {
            Console.WriteLine($"[严重错误] {message}: {exception.Message}");
            Console.WriteLine(exception.StackTrace);
            // 在实际环境中替换为真实日志调用
        }
        catch
        {
            // 如果日志系统初始化失败或不可用，则忽略
        }

        throw exception; // 重新抛出异常
    }
}
