using PCL.Neo.Core.Service.Accounts.Storage;
using PCL.Neo.Core.Utils;

namespace PCL.Neo.Core.Service.Accounts.OAuthService;

public class MinecraftInfo
{
    public static List<Skin> CollectSkins(
        IEnumerable<OAuthData.ResponseData.MinecraftPlayerUuidResponse.Skin> skins)
    {
        return skins.Select(skin => new
            {
                skin,
                state = skin.State switch
                {
                    "ACTIVE" => AccountState.Active,
                    "INACTIVE" => AccountState.Inactive,
                    _ => throw new ArgumentOutOfRangeException()
                },
                url = new Uri(skin.Url)
            })
            .Select(t =>
                new Skin(t.skin.Id, t.url, t.skin.Variant, t.skin.TextureKey,
                    t.state))
            .ToList();
    }

    public static List<Cape> CollectCapes(
        IEnumerable<OAuthData.ResponseData.MinecraftPlayerUuidResponse.Cape> capes)
    {
        return capes.Select(cape => new
            {
                cape,
                state = cape.State switch
                {
                    "ACTIVE" => AccountState.Active,
                    "INACTIVE" => AccountState.Inactive,
                    _ => throw new ArgumentOutOfRangeException()
                },
                url = new Uri(cape.Url)
            })
            .Select(t =>
                new Cape(t.cape.Id, t.state, t.url, t.cape.Alias))
            .ToList();
    }

    public static async Task<string> GetMinecraftAccessTokenAsync(string uhs, string xstsToken)
    {
        var jsonContent = new OAuthData.RequireData.MinecraftAccessTokenRequire
        {
            IdentityToken = $"XBL3.0 x={uhs};{xstsToken}"
        };

        var response = await Net.SendHttpRequestAsync<OAuthData.ResponseData.MinecraftAccessTokenResponse>(
            HttpMethod.Post,
            OAuthData.RequestUrls.MinecraftAccessTokenUri.Value,
            jsonContent);

        return response.AccessToken;
    }

    public static async Task<bool> IsHaveGameAsync(string accessToken)
    {
        var response = await Net.SendHttpRequestAsync<OAuthData.ResponseData.CheckHaveGameResponse>(
            HttpMethod.Get,
            OAuthData.RequestUrls.CheckHasMc.Value,
            bearerToken: accessToken);

        return response.Items.Any(it => !string.IsNullOrEmpty(it.Signature));
    }

    public static async Task<OAuthData.ResponseData.MinecraftPlayerUuidResponse>
        GetPlayerUuidAsync(string accessToken)
    {
        return await Net.SendHttpRequestAsync<OAuthData.ResponseData.MinecraftPlayerUuidResponse>(
            HttpMethod.Get,
            OAuthData.RequestUrls.PlayerUuidUri.Value,
            bearerToken: accessToken);
    }
}
