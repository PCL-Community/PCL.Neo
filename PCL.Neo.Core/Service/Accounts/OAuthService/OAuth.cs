using PCL.Neo.Core.Service.Accounts.OAuthService.Exceptions;
using PCL.Neo.Core.Utils;
using System.Diagnostics.CodeAnalysis;

namespace PCL.Neo.Core.Service.Accounts.OAuthService;

#pragma warning disable IL2026 // fixed by DynamicDependency

public static class OAuth
{
    public static async Task<OAuthData.ResponseData.AccessTokenResponse> RefreshTokenAsync(string refreshToken)
    {
        var authTokenData = new Dictionary<string, string>(OAuthData.FormUrlReqData.RefreshTokenData.Value)
        {
            ["refresh_token"] = refreshToken
        };

        return await Net.SendHttpRequestAsync<OAuthData.ResponseData.AccessTokenResponse>(
            HttpMethod.Post,
            OAuthData.RequestUrls.TokenUri.Value,
            new FormUrlEncodedContent(authTokenData));
    }

    [DynamicDependency(DynamicallyAccessedMemberTypes.PublicConstructors, typeof(OAuthData.RequireData))]
    public static async Task<OAuthData.ResponseData.XboxResponse> GetXboxTokenAsync(string accessToken)
    {
        var jsonContent =
            new OAuthData.RequireData.XboxLiveAuthRequire
            {
                Properties = new OAuthData.RequireData.XboxLiveAuthRequire.PropertiesData(accessToken)
            };

        return await Net.SendHttpRequestAsync<OAuthData.ResponseData.XboxResponse>(
            HttpMethod.Post,
            OAuthData.RequestUrls.XboxLiveAuth.Value,
            jsonContent);
    }

    [DynamicDependency(DynamicallyAccessedMemberTypes.PublicConstructors, typeof(OAuthData.RequireData))]
    public static async Task<string> GetXstsTokenAsync(string xblToken)
    {
        var jsonContent =
            new OAuthData.RequireData.XstsRequire(new OAuthData.RequireData.XstsRequire.PropertiesData([xblToken]));

        var response = await Net.SendHttpRequestAsync<OAuthData.ResponseData.XboxResponse>(
            HttpMethod.Post,
            OAuthData.RequestUrls.XstsAuth.Value,
            jsonContent);

        return response.Token;
    }

    public static async Task<string> GetMinecraftTokenAsync(string accessToken)
    {
        var xboxToken = await GetXboxTokenAsync(accessToken);
        var xstsToken = await GetXstsTokenAsync(xboxToken.Token);
        var minecraftAccessToken =
            await MinecraftInfo.GetMinecraftAccessTokenAsync(xboxToken.DisplayClaims.Xui.First().Uhs, xstsToken);

        if (!await MinecraftInfo.IsHaveGameAsync(minecraftAccessToken))
        {
            throw new NotHaveGameException("Logged-in user does not own any game!");
        }

        return minecraftAccessToken;
    }

    #region XblToken

    public record XblToken(string Token, string Uhs);

    #endregion
}
