using PCL.Neo.Core.Utils;

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
        GetPlayerUuidAsync(string accessToken) =>
        await Net.SendHttpRequestAsync<OAuthData.ResponseData.MinecraftPlayerUuidResponse>(
            HttpMethod.Get,
            OAuthData.RequestUrls.PlayerUuidUri.Value,
            bearerToken: accessToken);
}