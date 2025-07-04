namespace PCL.Neo.Core.Service.Accounts.OAuthService.RedirectServer;

public class AuthCode : IObserver<RedirectAuthCode>
{
    public ManualResetEvent IsGetAuthCode = new(false);

    public RedirectAuthCode? AuthCodeValue { get; private set; }

    /// <inheritdoc />
    public void OnCompleted() {}

    /// <inheritdoc />
    public void OnError(Exception error)
    {
        this.LogAuthError("获取微软授权码时发生错误", error);
        throw error;
    }

    /// <inheritdoc />
    public void OnNext(RedirectAuthCode value)
    {
        AuthCodeValue = value;
        this.LogAuthInfo($"成功获取微软授权码: {value.Code.Substring(0, 5)}...");
        IsGetAuthCode.Set();
    }

    public RedirectAuthCode GetAuthCode()
    {
        this.LogAuthDebug("等待获取微软授权码...");
        IsGetAuthCode.WaitOne();

        if (AuthCodeValue == null)
        {
            this.LogAuthError("授权码获取失败，返回值为空");
            throw new InvalidOperationException("授权码获取失败");
        }

        this.LogAuthDebug("授权码获取完成");
        return AuthCodeValue;
    }
}
