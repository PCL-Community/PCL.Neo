using PCL.Neo.Core.Service.Accounts.Storage;
using PCL.Neo.Core.Utils.Net;

namespace PCL.Neo.Core.Service.Accounts.OAuthService;

public static class MinecraftInfo
{
    public static List<Skin> CollectSkins(
        IEnumerable<OAuthData.ResponseData.MinecraftPlayerUuidResponse.Skin>? skins)
    {
        if (skins == null)
        {
            return [];
        }

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
        IEnumerable<OAuthData.ResponseData.MinecraftPlayerUuidResponse.Cape>? capes)
    {
        if (capes == null)
        {
            return [];
        }

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

        var response = await StaticNet
            .SendJsonAsync<OAuthData.ResponseData.MinecraftAccessTokenResponse,
                OAuthData.RequireData.MinecraftAccessTokenRequire>(
                HttpMethod.Post,
                OAuthData.RequestUrls.MinecraftAccessTokenUri.Value,
                jsonContent);

        return response.AccessToken;
    }

    public static async Task<bool> IsHaveGameAsync(string accessToken)
    {
        var response = await StaticNet.SendAsync<OAuthData.ResponseData.CheckHaveGameResponse>(HttpMethod.Get,
            OAuthData.RequestUrls.CheckHasMc.Value, accessToken);

        return response.Items.Any(it => !string.IsNullOrEmpty(it.Signature));
    }

    public static async Task<OAuthData.ResponseData.MinecraftPlayerUuidResponse>
        GetPlayerUuidAsync(string accessToken)
    {
        var response = await StaticNet.SendAsync<OAuthData.ResponseData.MinecraftPlayerUuidResponse>(HttpMethod.Get,
            OAuthData.RequestUrls.PlayerUuidUri.Value, accessToken);

        return response;
    }
}
