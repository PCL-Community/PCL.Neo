using PCL.Neo.Core.Service.Accounts.OAuthService.RedirectServer;
using PCL.Neo.Core.Service.Accounts.Storage;
using System.Diagnostics;

namespace PCL.Neo.Core.Service.Accounts.OAuthService;

[Obsolete]
public class AuthCodeMode
{
    // 定义UserTokens类型 - 当这个类型在其他地方定义时，可以删除这个定义
    public record UserTokens(string AccessToken, string RefreshToken, int ExpiresIn);

    public static async Task<MsaAccount> LogInAsync()
    {
        try
        {
            var authCode             = GetAuthCode();
            var authToken            = await GetAuthTokenAsync(authCode).ConfigureAwait(false);
            var minecraftAccessToken = await OAuth.GetMinecraftTokenAsync(authToken.AccessToken).ConfigureAwait(false);

            var playerUuidAndName = await MinecraftInfo.GetPlayerUuidAsync(minecraftAccessToken).ConfigureAwait(false);

            return new MsaAccount()
            {
                McAccessToken = minecraftAccessToken,
                OAuthToken =
                    new OAuthTokenData(authToken.AccessToken, authToken.RefreshToken,
                        new DateTimeOffset(DateTime.Today, TimeSpan.FromSeconds(authToken.ExpiresIn))),
                UserName       = playerUuidAndName.Name,
                UserProperties = string.Empty,
                Uuid           = playerUuidAndName.Uuid,
                Capes          = MinecraftInfo.CollectCapes(playerUuidAndName.Capes),
                Skins          = MinecraftInfo.CollectSkins(playerUuidAndName.Skins)
            };
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
            // TODO: 记录此异常信息到日志系统
        }
    }

    private static string GetAuthCode()
    {
        var url = OAuthData.FormUrlReqData.GetAuthCodeData;
        // TODO: 在程序配置中设置重定向服务器端口
        var redirectServer = new RedirectServer.RedirectServer(5050, false);
        var authCode = new AuthCode();
        redirectServer.Subscribe(authCode);

        // TODO: 处理不同系统环境下的浏览器启动方式
        // TODO: 添加超时处理逻辑
        Process.Start(new ProcessStartInfo(url.Value) { UseShellExecute = true });

        try
        {
            redirectServer.StartListening();
            return authCode.GetAuthCode().Code;
        }
        finally
        {
            redirectServer.Close();
        }
    }

    private static async Task<UserTokens> GetAuthTokenAsync(string code)
    {
        var data = new Dictionary<string, string>(OAuthData.FormUrlReqData.AuthTokenData.Value) { ["code"] = code };

        // TODO: 完成获取Auth Token的请求逻辑，替换临时返回值
        return new UserTokens("temp_access_token", "temp_refresh_token", 3600);
    }
}