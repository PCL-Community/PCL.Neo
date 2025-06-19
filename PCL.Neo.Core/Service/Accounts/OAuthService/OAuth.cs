using Flurl.Http;
using PCL.Neo.Core.Models.Profile;
using PCL.Neo.Core.Service.Accounts.OAuthService.Exceptions;
using PCL.Neo.Core.Utils;
using System.Diagnostics.CodeAnalysis;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using System.Threading;
using static PCL.Neo.Core.Service.Accounts.OAuthService.OAuthData.RequireData;

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
        var request = Net.Request(OAuthData.RequestUrls.TokenUri.Value);
        var jsonRsq = await request.PostAsync(new FormUrlEncodedContent(authTokenData));
        var temp = await jsonRsq.GetStringAsync();
        var result = JsonSerializer.Deserialize<OAuthData.ResponseData.AccessTokenResponse>(temp);
        return result;
    }

    [DynamicDependency(DynamicallyAccessedMemberTypes.PublicConstructors, typeof(OAuthData.RequireData))]
    public static async Task<OAuthData.ResponseData.XboxResponse> GetXboxTokenAsync(string accessToken)
    {
        var xblContent = new XBLTokenPayload
        {
            Properties = new XBLProperties
            {
                AuthMethod = "RPS",
                SiteName = "user.auth.xboxlive.com",
                RpsTicket = $"d={accessToken}"
            },
            RelyingParty = "http://auth.xboxlive.com",
            TokenType = "JWT"
        };
        var request = Net.Request("https://user.auth.xboxlive.com/user/authenticate");

        /*var result = await Net.SendHttpRequestAsync<OAuthData.ResponseData.XboxResponse>(
            HttpMethod.Post,
            OAuthData.RequestUrls.XboxLiveAuth.Value,
            jsonContent);*/
        try
        {
            var b = JsonSerializer.Serialize(xblContent);
            var xblJsonReq = await request.PostAsync(new StringContent(b,Encoding.UTF8, "application/json"));
            var temp = await xblJsonReq.GetStringAsync();
            var result = JsonSerializer.Deserialize<OAuthData.ResponseData.XboxResponse>(temp);
            return result;
        }
        catch(Exception ex)
        {
            throw ex;
        }
        
        
    }

    [DynamicDependency(DynamicallyAccessedMemberTypes.PublicConstructors, typeof(OAuthData.RequireData))]
    public static async Task<string> GetXstsTokenAsync(string xblToken)
    {
        var request = Net.Request("https://xsts.auth.xboxlive.com/xsts/authorize");
        var xstsContent = new XSTSTokenPayload
        {
            Properties = new XSTSProperties
            {
                SandboxId = "RETAIL",
                UserTokens = [xblToken]
            },
            RelyingParty = "rp://api.minecraftservices.com/",
            TokenType = "JWT"
        };
        var a = JsonSerializer.Serialize(xstsContent);
        var xstsJsonReq = await request.PostAsync(new StringContent(a, Encoding.UTF8, "application/json"));
        /*var response = await Net.SendHttpRequestAsync<OAuthData.ResponseData.XboxResponse>(
            HttpMethod.Post,
            OAuthData.RequestUrls.XstsAuth.Value,
            jsonContent);*/
        var temp = await xstsJsonReq.GetStringAsync();
        var response = JsonSerializer.Deserialize<OAuthData.ResponseData.XboxResponse>(temp);
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