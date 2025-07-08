using PCL.Neo.Core.Service.Accounts.Storage;

namespace PCL.Neo.Core.Service.Accounts.IYggdrasilAuth;

/// <summary>
/// Yggdrasil外置登录认证服务接口
/// </summary>
public interface IYggdrasilAuthService
{
    /// <summary>
    /// 使用用户名和密码进行验证，获取外置登录账户
    /// </summary>
    /// <param name="serverUrl">外置登录服务器URL</param>
    /// <param name="username">用户名</param>
    /// <param name="password">密码</param>
    /// <param name="timeoutMs">请求超时时间(毫秒)</param>
    /// <returns>验证成功的账户</returns>
    Task<YggdrasilAccount> AuthenticateAsync(string serverUrl, string username, string password, int timeoutMs = 10000);

    /// <summary>
    /// 刷新外置登录账户的令牌
    /// </summary>
    /// <param name="account">需要刷新的账户</param>
    /// <param name="timeoutMs">请求超时时间(毫秒)</param>
    /// <returns>更新后的账户</returns>
    Task<YggdrasilAccount> RefreshAsync(YggdrasilAccount account, int timeoutMs = 10000);

    /// <summary>
    /// 验证外置登录账户的令牌是否有效
    /// </summary>
    /// <param name="account">需要验证的账户</param>
    /// <param name="timeoutMs">请求超时时间(毫秒)</param>
    /// <returns>令牌是否有效</returns>
    Task<bool> ValidateAsync(YggdrasilAccount account, int timeoutMs = 5000);

    /// <summary>
    /// 注销外置登录账户
    /// </summary>
    /// <param name="account">需要注销的账户</param>
    /// <param name="timeoutMs">请求超时时间(毫秒)</param>
    /// <returns>异步任务</returns>
    Task SignOutAsync(YggdrasilAccount account, int timeoutMs = 5000);

    /// <summary>
    /// 使外置登录账户的令牌失效
    /// </summary>
    /// <param name="account">需要处理的账户</param>
    /// <param name="timeoutMs">请求超时时间(毫秒)</param>
    /// <returns>异步任务</returns>
    Task InvalidateAsync(YggdrasilAccount account, int timeoutMs = 5000);
}
