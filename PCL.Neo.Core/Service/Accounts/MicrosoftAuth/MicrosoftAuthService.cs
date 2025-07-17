using PCL.Neo.Core.Service.Accounts.OAuthService;
using PCL.Neo.Core.Service.Accounts.Storage;
using PCL.Neo.Core.Utils.Net;
using System.Diagnostics;
using System.Net.Http.Headers;
using System.Reactive.Linq;

namespace PCL.Neo.Core.Service.Accounts.MicrosoftAuth;

public class MicrosoftAuthService : IMicrosoftAuthService
{
    private readonly INetService _netService = new NetService("MicrosoftAuthService");

    /// <inheritdoc />
    public IObservable<DeviceFlowState> StartDeviceCodeFlow()
    {
        return Observable.Create<DeviceFlowState>(async observer =>
        {
            try
            {
                // get device code
                var deviceCodeInfo = await RequestDeviceCodeAsync().ConfigureAwait(false);

                // show for user and open browser
                OpenBrowserAsync(deviceCodeInfo.VerificationUri);
                observer.OnNext(new DeviceFlowAwaitUser(deviceCodeInfo.UserCode, deviceCodeInfo.VerificationUri));

                // polling server
                observer.OnNext(new DeviceFlowPolling());
                var tokenInfo = await PollForTokenAsync(deviceCodeInfo.DeviceCode, deviceCodeInfo.Interval)
                    .ConfigureAwait(false);

                // get user mc token
                var mcToken = await GetUserMinecraftAccessTokenAsync(tokenInfo.AccessToken).ConfigureAwait(false);

                // get user account info
                var accountInfo = await GetUserAccountInfoAsync(mcToken).ConfigureAwait(false);
                var account = new MsaAccount
                {
                    McAccessToken = mcToken,
                    OAuthToken = new OAuthTokenData(tokenInfo.AccessToken, tokenInfo.RefreshToken, tokenInfo.ExpiresIn),
                    UserName = accountInfo.UserName,
                    UserProperties = string.Empty,
                    Uuid = accountInfo.Uuid,
                    Capes = accountInfo.Capes,
                    Skins = accountInfo.Skins
                };

                observer.OnNext(new DeviceFlowSucceeded(account));
            }
            catch (Exception e)
            {
                observer.OnError(e);
            }

            observer.OnCompleted();
        });
    }

    /// <inheritdoc />
    public async Task<DeviceCodeData.DeviceCodeInfo> RequestDeviceCodeAsync()
    {
        var content = new FormUrlEncodedContent(OAuthData.FormUrlReqData.DeviceCodeData.Value)
        {
            Headers = { ContentType = new MediaTypeHeaderValue("application/x-www-form-urlencoded") }
        };

        var result = await _netService.SendAsync<DeviceCodeData.DeviceCodeInfo>(HttpMethod.Post,
            OAuthData.RequestUrls.DeviceCode.Value.ToString()).ConfigureAwait(false);
        return result;
    }

    /// <inheritdoc />
    public async Task<DeviceCodeData.DeviceCodeAccessToken> PollForTokenAsync(
        string deviceCode, int interval)
    {
        var tempInterval = interval;
        var content = new Dictionary<string, string>(OAuthData.FormUrlReqData.UserAuthStateData.Value)
        {
            ["device_code"] = deviceCode
        };

        var msg = new FormUrlEncodedContent(content)
        {
            Headers = { ContentType = new MediaTypeHeaderValue("application/x-www-form-urlencoded") }
        };

        var request = new HttpRequestMessage(HttpMethod.Post, OAuthData.RequestUrls.TokenUri.Value);
        request.Content = msg;

        while (true)
        {
            await Task.Delay(tempInterval).ConfigureAwait(false);

            var tempResult = await _netService
                .SendAsync<OAuthData.ResponseData.UserAuthStateResponse>(request).ConfigureAwait(false);

            // handle response
            if (!string.IsNullOrEmpty(tempResult.Error))
            {
                switch (tempResult.Error)
                {
                    case "authorization_declined":
                        throw new DeviceFlowDeclined();
                    case "expired_token":
                        throw new DeviceFlowExpired();
                    case "bad_verification_code":
                        throw new DeviceFlowDeclined();
                    case "slow_down":
                        tempInterval = Math.Min(tempInterval * 2, 900); // Adjust polling interval
                        continue;
                    case "authorization_pending":
                        continue; // Keep polling
                    default:
                        throw new DeviceFlowUnkonw();
                }
            }

            // create result
            var result = new DeviceCodeData.DeviceCodeAccessToken(tempResult.AccessToken,
                tempResult.RefreshToken,
                DateTimeOffset.UtcNow.AddSeconds(tempResult.ExpiresIn));

            return result;
        }
    }

    /// <inheritdoc />
    public async Task<string> GetUserMinecraftAccessTokenAsync(
        string accessToken)
    {
        var minecraftToken = await OAuth.GetMinecraftTokenAsync(accessToken).ConfigureAwait(false);
        return minecraftToken;
    }

    /// <inheritdoc />
    public async Task<DeviceCodeData.McAccountInfo> GetUserAccountInfoAsync(string accessToken)
    {
        var playerInfo = await MinecraftInfo.GetPlayerUuidAsync(accessToken).ConfigureAwait(false);
        var capes = MinecraftInfo.CollectCapes(playerInfo.Capes);
        var skins = MinecraftInfo.CollectSkins(playerInfo.Skins);
        var uuid = playerInfo.Uuid;

        return new DeviceCodeData.McAccountInfo(skins, capes, playerInfo.Name, uuid);
    }

    /// <inheritdoc />
    public async Task<OAuthTokenData> RefreshTokenAsync(string refreshToken)
    {
        var newToken = await OAuth.RefreshTokenAsync(refreshToken).ConfigureAwait(false);
        var newTokenData = new OAuthTokenData(newToken.AccessToken, newToken.RefreshToken,
            new DateTimeOffset(DateTime.Now, TimeSpan.FromSeconds(newToken.ExpiresIn)));

        return newTokenData;
    }

    private static void OpenBrowserAsync(string requiredUrl)
    {
        var processStartInfo =
            new ProcessStartInfo
            {
                FileName = requiredUrl, UseShellExecute = false
            }; // #WARN this method may cant run on linux and macos

        Process.Start(processStartInfo);
    }
}
