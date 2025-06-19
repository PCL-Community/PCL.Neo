using Flurl.Http;
using PCL.Neo.Core.Utils;
using System.Text;
using System.Text.Json;

namespace PCL.Neo.Core.Service.Accounts.OAuthService;

public partial class MinecraftInfo
{
    public static List<Storage.Skin> CollectSkins(
        IEnumerable<OAuthData.ResponseData.MinecraftPlayerUuidResponse.Skin> skins) =>
        skins.Select(skin => new
            {
                skin,
                state = skin.State switch
                {
                    "ACTIVE" => Storage.AccountState.Active,
                    "INACTIVE" => Storage.AccountState.Inactive,
                    _ => throw new ArgumentOutOfRangeException()
                },
                url = new Uri(skin.Url)
            })
            .Select(t =>
                new Storage.Skin(t.skin.Id, t.url, t.skin.Variant, t.skin.TextureKey,
                    t.state))
            .ToList();

    public static List<Storage.Cape> CollectCapes(
        IEnumerable<OAuthData.ResponseData.MinecraftPlayerUuidResponse.Cape> capes) =>
        capes.Select(cape => new
            {
                cape,
                state = cape.State switch
                {
                    "ACTIVE" => Storage.AccountState.Active,
                    "INACTIVE" => Storage.AccountState.Inactive,
                    _ => throw new ArgumentOutOfRangeException()
                },
                url = new Uri(cape.Url)
            })
            .Select(t =>
                new Storage.Cape(t.cape.Id, t.state, t.url, t.cape.Alias))
            .ToList();

    public static async Task<string> GetMinecraftAccessTokenAsync(string uhs, string xstsToken)
    {
        var jsonContent = new OAuthData.RequireData.MinecraftAccessTokenRequire
        {
            IdentityToken = $"XBL3.0 x={uhs};{xstsToken}"
        };

        /*var response = await Net.SendHttpRequestAsync<OAuthData.ResponseData.MinecraftAccessTokenResponse>(
            HttpMethod.Post,
            OAuthData.RequestUrls.MinecraftAccessTokenUri.Value,
            jsonContent);*/
        var request = Net.Request("https://api.minecraftservices.com/authentication/login_with_xbox");
        var a = JsonSerializer.Serialize(jsonContent);
        var jsonResponse = await request.PostAsync(new StringContent(a, Encoding.UTF8, "application/json"));
        var temp = await jsonResponse.GetStringAsync();
        var response = JsonSerializer.Deserialize<OAuthData.ResponseData.MinecraftAccessTokenResponse>(temp);
        return response.AccessToken;
    }

    public static async Task<bool> IsHaveGameAsync(string accessToken)
    {
        /*
        var response = await Net.SendHttpRequestAsync<OAuthData.ResponseData.CheckHaveGameResponse>(
            HttpMethod.Get,
            OAuthData.RequestUrls.CheckHasMc.Value,
            bearerToken: accessToken);*/
        var request = Net.Request(OAuthData.RequestUrls.CheckHasMc.Value);
        var jsonRsp = await request.WithHeader("Authorization", $"Bearer {accessToken}").GetAsync();
        var temp = await jsonRsp.GetStringAsync();
        var response = JsonSerializer.Deserialize<OAuthData.ResponseData.CheckHaveGameResponse>(temp);
        return response.Items.Any(it => !string.IsNullOrEmpty(it.Signature));
    }

    public static async Task<OAuthData.ResponseData.MinecraftPlayerUuidResponse>GetPlayerUuidAsync(string accessToken)
    {
        /*await Net.SendHttpRequestAsync<OAuthData.ResponseData.MinecraftPlayerUuidResponse>(
            HttpMethod.Get,
            OAuthData.RequestUrls.PlayerUuidUri.Value,
            bearerToken: accessToken);*/
        var request = Net.Request(OAuthData.RequestUrls.PlayerUuidUri.Value);
        var jsonRsp = await request.WithHeader("Authorization", $"Bearer {accessToken}").GetAsync();
        var temp = await jsonRsp.GetStringAsync();
        var result = JsonSerializer.Deserialize<OAuthData.ResponseData.MinecraftPlayerUuidResponse>(temp);
        return result;
    }
}