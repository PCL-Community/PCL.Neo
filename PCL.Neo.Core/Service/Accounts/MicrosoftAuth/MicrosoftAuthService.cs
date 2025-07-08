using PCL.Neo.Core.Service.Accounts.Exceptions;
using PCL.Neo.Core.Service.Accounts.OAuthService;
using PCL.Neo.Core.Service.Accounts.OAuthService.Exceptions;
using PCL.Neo.Core.Service.Accounts.Storage;
using PCL.Neo.Core.Utils;
using System.Diagnostics;
using System.Net.Http.Headers;
using System.Reactive.Linq;
using System.Text.Json;

namespace PCL.Neo.Core.Service.Accounts.MicrosoftAuth;

public class MicrosoftAuthService : IMicrosoftAuthService
{
    /// <inheritdoc />
    public IObservable<DeviceFlowState> StartDeviceCodeFlow()
    {
        return Observable.Create<DeviceFlowState>(async observer =>
        {
            // get device code
            var deviceCodeResult = await RequestDeviceCodeAsync().ConfigureAwait(false);
            if (deviceCodeResult.IsFailure)
            {
                observer.OnError(deviceCodeResult.Error.Exception!);
                return;
            }

            var deviceCodeInfo = deviceCodeResult.Value;

            // show for user and open browser
            OpenBrowserAsync(deviceCodeInfo.VerificationUri);
            observer.OnNext(new DeviceFlowAwaitUser(deviceCodeInfo.UserCode, deviceCodeInfo.VerificationUri));

            // polling server
            var tokenResult = await PollForTokenAsync(deviceCodeInfo.DeviceCode, deviceCodeInfo.Interval)
                .ConfigureAwait(false);
            observer.OnNext(new DeviceFlowPolling());

            if (tokenResult.IsFailure)
            {
                observer.OnNext(tokenResult.Error);
                return;
            }

            var tokenInfo = tokenResult.Value;

            // get user mc token
            var mcToken = await GetUserMinecraftAccessTokenAsync(tokenInfo.AccessToken).ConfigureAwait(false);
            if (mcToken.IsFailure)
            {
                observer.OnError(mcToken.Error);
                return;
            }

            // get user account info
            var accountInfoResult = await GetUserAccountInfoAsync(mcToken.Value).ConfigureAwait(false);
            if (accountInfoResult.IsFailure)
            {
                observer.OnError(accountInfoResult.Error!);
                return;
            }

            var accountInfo = accountInfoResult.Value;

            var account = new MsaAccount
            {
                McAccessToken = mcToken.Value,
                OAuthToken = new OAuthTokenData(tokenInfo.AccessToken, tokenInfo.RefreshToken, tokenInfo.ExpiresIn),
                UserName = accountInfo.UserName,
                UserProperties = string.Empty,
                Uuid = accountInfo.Uuid,
                Capes = accountInfo.Capes,
                Skins = accountInfo.Skins
            };

            observer.OnNext(new DeviceFlowSucceeded(account));
            observer.OnCompleted();
        });
    }

    /// <inheritdoc />
    public async Task<Result<DeviceCodeData.DeviceCodeInfo, HttpError>> RequestDeviceCodeAsync()
    {
        var content = new FormUrlEncodedContent(OAuthData.FormUrlReqData.DeviceCodeData.Value)
        {
            Headers = { ContentType = new MediaTypeHeaderValue("application/x-www-form-urlencoded") }
        };

        try
        {
            var temp = await Net.SendHttpRequestAsync<DeviceCodeData.DeviceCodeInfo>(HttpMethod.Post,
                OAuthData.RequestUrls.DeviceCode.Value, content).ConfigureAwait(false);
            var result = new DeviceCodeData.DeviceCodeInfo(temp.DeviceCode, temp.UserCode, temp.VerificationUri,
                temp.Interval);
            return Result<DeviceCodeData.DeviceCodeInfo, HttpError>.Ok(result);
        }
        catch (HttpRequestException e)
        {
            return Result<DeviceCodeData.DeviceCodeInfo, HttpError>.Fail(new HttpError(null,
                "Network error while requesting device code.", Exception: e));
        }
        catch (JsonException e)
        {
            return Result<DeviceCodeData.DeviceCodeInfo, HttpError>.Fail(new HttpError(null,
                "Failed to parse device code response.", Exception: e));
        }
        catch (Exception e)
        {
            return Result<DeviceCodeData.DeviceCodeInfo, HttpError>.Fail(new HttpError(null,
                "An unexpected error occurred.", Exception: e));
        }
    }

    /// <inheritdoc />
    public async Task<Result<DeviceCodeData.DeviceCodeAccessToken, DeviceFlowError>> PollForTokenAsync(
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

        while (true)
        {
            try
            {
                await Task.Delay(tempInterval).ConfigureAwait(false);

                var tempResult =
                    await Net.SendHttpRequestAsync<OAuthData.ResponseData.UserAuthStateResponse>(HttpMethod.Post,
                        OAuthData.RequestUrls.TokenUri.Value, msg).ConfigureAwait(false);

                // handle response
                if (!string.IsNullOrEmpty(tempResult.Error))
                {
                    switch (tempResult.Error)
                    {
                        case "authorization_declined":
                            return Result<DeviceCodeData.DeviceCodeAccessToken, DeviceFlowError>.Fail(
                                new DeviceFlowError(new DeviceFlowDeclined(), null));
                        case "expired_token":
                            return Result<DeviceCodeData.DeviceCodeAccessToken, DeviceFlowError>.Fail(
                                new DeviceFlowError(new DeviceFlowExpired(), null));
                        case "bad_verification_code":
                            return Result<DeviceCodeData.DeviceCodeAccessToken, DeviceFlowError>.Fail(
                                new DeviceFlowError(new DeviceFlowDeclined(), null));
                        case "slow_down":
                            tempInterval = Math.Min(tempInterval * 2, 900); // Adjust polling interval
                            continue;
                        case "authorization_pending":
                            continue; // Keep polling
                        default:
                            return Result<DeviceCodeData.DeviceCodeAccessToken, DeviceFlowError>.Fail(
                                new DeviceFlowError(new DeviceFlowUnkonw(), null));
                    }
                }

                // create result
                var result = new DeviceCodeData.DeviceCodeAccessToken(tempResult.AccessToken,
                    tempResult.RefreshToken,
                    DateTimeOffset.UtcNow.AddSeconds((double)tempResult.ExpiresIn));

                return Result<DeviceCodeData.DeviceCodeAccessToken, DeviceFlowError>.Ok(result);
            }
            catch (HttpRequestException e)
            {
                return Result<DeviceCodeData.DeviceCodeAccessToken, DeviceFlowError>.Fail(
                    new DeviceFlowError(new DeviceFlowInternetError(), e));
            }
            catch (JsonException e)
            {
                return Result<DeviceCodeData.DeviceCodeAccessToken, DeviceFlowError>.Fail(
                    new DeviceFlowError(new DeviceFlowJsonError(), e));
            }
            catch (Exception e)
            {
                return Result<DeviceCodeData.DeviceCodeAccessToken, DeviceFlowError>.Fail(
                    new DeviceFlowError(new DeviceFlowUnkonw(), e));
            }
        }
    }

    /// <inheritdoc />
    public async Task<Result<string, NotHaveGameException>> GetUserMinecraftAccessTokenAsync(
        string accessToken)
    {
        try
        {
            var minecraftToken = await OAuth.GetMinecraftTokenAsync(accessToken).ConfigureAwait(false);
            return Result<string, NotHaveGameException>.Ok(minecraftToken);
        }
        catch (NotHaveGameException e)
        {
            return Result<string, NotHaveGameException>.Fail(
                new NotHaveGameException(e.Message));
        }
    }

    /// <inheritdoc />
    public async Task<Result<DeviceCodeData.McAccountInfo, Exception>> GetUserAccountInfoAsync(string accessToken)
    {
        try
        {
            var playerInfo = await MinecraftInfo.GetPlayerUuidAsync(accessToken).ConfigureAwait(false);
            var capes = MinecraftInfo.CollectCapes(playerInfo.Capes);
            var skins = MinecraftInfo.CollectSkins(playerInfo.Skins);
            var uuid = playerInfo.Uuid;

            return Result<DeviceCodeData.McAccountInfo, Exception>.Ok(
                new DeviceCodeData.McAccountInfo(skins, capes, playerInfo.Name, uuid));
        }
        catch (Exception e)
        {
            return Result<DeviceCodeData.McAccountInfo, Exception>.Fail(e);
        }
    }

    /// <inheritdoc />
    public async Task<Result<OAuthTokenData, Exception>> RefreshTokenAsync(string refreshToken)
    {
        var newToken = await OAuth.RefreshTokenAsync(refreshToken).ConfigureAwait(false);
        try
        {
            var newTokenData = new OAuthTokenData(newToken.AccessToken, newToken.RefreshToken,
                new DateTimeOffset(DateTime.Now, TimeSpan.FromSeconds(newToken.ExpiresIn)));

            return Result<OAuthTokenData, Exception>.Ok(newTokenData);
        }
        catch (Exception e)
        {
            return Result<OAuthTokenData, Exception>.Fail(e);
        }
    }

    private static void OpenBrowserAsync(string requiredUrl)
    {
        var processStartInfo =
            new ProcessStartInfo
            {
                FileName = requiredUrl, UseShellExecute = true
            }; // #WARN this method may cant run on linux and macos

        Process.Start(processStartInfo);
    }
}
