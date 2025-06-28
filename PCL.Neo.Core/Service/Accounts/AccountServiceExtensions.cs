using Microsoft.Extensions.DependencyInjection;
using PCL.Neo.Core.Service.Accounts.Exceptions;
using PCL.Neo.Core.Service.Accounts.MicrosoftAuth;
using PCL.Neo.Core.Service.Profiles;

namespace PCL.Neo.Core.Service.Accounts
{
    /// <summary>
    /// 账户和档案服务的注册扩展方法
    /// </summary>
    public static class AccountServiceExtensions
    {
        /// <summary>
        /// 注册账户和档案相关服务
        /// </summary>
        public static IServiceCollection AddAccountServices(this IServiceCollection services)
        {
            // 注册微软认证服务
            services.AddSingleton<IMicrosoftAuthService, MicrosoftAuthService>();
            
            // 注册账户服务
            services.AddSingleton<IAccountService, AccountService>();
            
            // 注册档案服务
            services.AddSingleton<IProfileService, ProfileService>();
            
            return services;
        }
        
        /// <summary>
        /// 使用账户和档案服务进行全局异常处理
        /// </summary>
        public static void UseAccountExceptionHandling()
        {
            // 设置全局未处理异常处理器
            AppDomain.CurrentDomain.UnhandledException += (_, e) =>
            {
                if (e.ExceptionObject is not Exception ex)
                {
                    return;
                }

                try
                {
                    // 使用我们的日志扩展方法
                    var service = new object(); // 临时对象，用于调用扩展方法

                    if (ex is YggdrasilAuthException yggEx)
                    {
                        service.LogYggdrasilError(
                            $"未处理的外置登录异常: {yggEx.Message}\n服务器: {yggEx.ServerUrl}\n错误类型: {yggEx.ErrorType}\n错误内容: {yggEx.ErrorContent}",
                            yggEx);
                    }
                    else
                    {
                        service.LogAccountError($"未处理的账户系统异常: {ex.Message}", ex);
                    }
                }
                catch
                {
                    // 如果日志记录失败，确保不会引发新异常
                }
            };
        }
    }
} 