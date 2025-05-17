using System.Collections.Immutable;
using System.Text.Json.Serialization;

namespace PCL.Neo.Core.Service.Accounts.OAuthService;

#nullable disable

public static class OAuthData
{
    public static class RequestUrls
    {
        public static readonly Uri AuthCodeUri =
            new("https://login.microsoftonline.com/consumers/oauth2/v2.0/authorize");

        public static readonly Uri DeviceCode =
            new("https://login.microsoftonline.com/consumers/oauth2/v2.0/devicecode");

        public static readonly Uri TokenUri = new("https://login.microsoftonline.com/consumers/oauth2/v2.0/token");
        public static readonly Uri XboxLiveAuth = new("https://user.auth.xboxlive.com/user/authenticate");
        public static readonly Uri XstsAuth = new("https://xsts.auth.xboxlive.com/xsts/authorize");

        public static readonly Uri MinecraftAccessTokenUri =
            new("https://api.minecraftservices.com/authentication/login_with_xbox");

        public static readonly Uri CheckHasMc = new("https://api.minecraftservices.com/entitlements/mcstore");
        public static readonly Uri PlayerUuidUri = new("https://api.minecraftservices.com/minecraft/profile");
    }

    public static class FormUrlReqData
    {
        // TODO: 配置微软OAuth客户端ID
        public const string ClientId = "";
        
        // TODO: 配置微软OAuth重定向URI
        public static readonly Uri RedirectUri = new("http://127.0.0.1:5050");
        
        // TODO: 配置微软OAuth客户端密钥
        public const string ClientSecret = "";

        public static string GetAuthCodeData() =>
            $"{RequestUrls.AuthCodeUri}?client_id={ClientId}&response_type=code&redirect_uri={RedirectUri}&response_mode=query&scope=XboxLive.signin offline_access";

        public static IReadOnlyDictionary<string, string> DeviceCodeData { get; } =
            new Dictionary<string, string> { { "client_id", ClientId }, { "scope", "XboxLive.signin offline_access" } }
                .ToImmutableDictionary();

        public static IReadOnlyDictionary<string, string> UserAuthStateData { get; } =
            new Dictionary<string, string>
            {
                { "grant_type", "urn:ietf:params:oauth:grant-type:device_code" },
                { "client_id", ClientId },
                { "device_code", "" }
            }.ToImmutableDictionary();

        public static IReadOnlyDictionary<string, string> AuthTokenData { get; } =
            new Dictionary<string, string>
            {
                { "client_id", ClientId },
                { "code", "" },
                { "grant_type", "authorization_code" },
                { "redirect_uri", RedirectUri.ToString() },
                { "scope", "XboxLive.signin offline_access" }
            }.ToImmutableDictionary();

        public static IReadOnlyDictionary<string, string> RefreshTokenData { get; } =
            new Dictionary<string, string>
            {
                { "client_id", ClientId },
                { "client_secret", ClientSecret },
                { "refresh_token", "" },
                { "grant_type", "refresh_token" },
                { "scope", "XboxLive.signin offline_access" }
            }.ToImmutableDictionary();
    }

    public static class RequireData
    {
        public record XboxLiveAuthRequire(
            [property: JsonPropertyName("PropertiesData")]
            OAuthData.RequireData.XboxLiveAuthRequire.PropertiesData Properties)
        {
            public const string TokenType = "JWT";
            public static string RelyingParty => "http://auth.xboxlive.com";

            public record PropertiesData(
                [property: JsonPropertyName("RpsTicket")]
                string RpsTicket)
            {
                public const string AuthMethod = "RPS";
                public const string SiteName = "user.auth.xboxlive.com";
            }
        }

        public record XstsRequire(
            OAuthData.RequireData.XstsRequire.PropertiesData Properties)
        {
            public const string RelyingParty = "rp://api.minecraftservices.com/";
            public const string TokenType = "JWT";

            public record PropertiesData(
                [property: JsonPropertyName("UserTokens")]
                List<string> UserTokens)
            {
                public const string SandboxId = "RETAIL";
            }
        }

        public class MinecraftAccessTokenRequire
        {
            [JsonPropertyName("identityToken")] public string IdentityToken { get; set; }
        }
    }

    public static class ResponseData
    {
        public record AccessTokenResponse
        {
            [JsonPropertyName("token_type")] public string TokenType { get; set; }
            [JsonPropertyName("scope")] public string Scope { get; set; }
            [JsonPropertyName("expires_in")] public int ExpiresIn { get; set; }
            [JsonPropertyName("ext_expires_in")] public int ExtExpiresIn { get; set; }
            [JsonPropertyName("access_token")] public string AccessToken { get; set; }
            [JsonPropertyName("refresh_token")] public string RefreshToken { get; set; }
        }

        public record DeviceCodeResponse(
            [property: JsonPropertyName("device_code")]
            string DeviceCode,
            [property: JsonPropertyName("user_code")]
            string UserCode,
            [property: JsonPropertyName("verification_uri")]
            string VerificationUri,
            [property: JsonPropertyName("expires_in")]
            int ExpiresIn,
            [property: JsonPropertyName("interval")]
            int Interval,
            [property: JsonPropertyName("message")]
            string Message
        );

        public record UserAuthStateResponse(
            [property: JsonPropertyName("token_type")]
            string TokenType,
            [property: JsonPropertyName("scope")] string Scope,
            [property: JsonPropertyName("expires_in")]
            int ExpiresIn,
            [property: JsonPropertyName("access_token")]
            string AccessToken,
            [property: JsonPropertyName("refresh_token")]
            string RefreshToken,
            [property: JsonPropertyName("error")] string Error,
            [property: JsonPropertyName("error_description")]
            string ErrorDescription,
            [property: JsonPropertyName("correlation_id")]
            string CorrelationId
        );

        public record XboxResponse
        {
            [JsonPropertyName("IssueInstant")] public DateTime IssueInstant { get; set; }
            [JsonPropertyName("NotAfter")] public DateTime NotAfter { get; set; }
            [JsonPropertyName("Token")] public string Token { get; set; }
            [JsonPropertyName("DisplayClaims")] public DisplayClaimsData DisplayClaims { get; set; }

            public record DisplayClaimsData
            {
                [JsonPropertyName("xui")] public List<XuiData> Xui { get; set; }

                public record XuiData
                {
                    [JsonPropertyName("uhs")] public string Uhs { get; set; }
                }
            }
        }

        public record MinecraftAccessTokenResponse
        {
            [JsonPropertyName("username")] public string Username { get; set; }
            [JsonPropertyName("roles")] public List<object> Roles { get; set; }
            [JsonPropertyName("access_token")] public string AccessToken { get; set; }
            [JsonPropertyName("token_type")] public string TokenType { get; set; }
            [JsonPropertyName("expires_in")] public int ExpiresIn { get; set; }
        }

        public record CheckHaveGameResponse
        {
            [JsonPropertyName("items")] public List<Item> Items { get; set; }
            [JsonPropertyName("signature")] public string Signature { get; set; }
            [JsonPropertyName("keyId")] public string KeyId { get; set; }

            public record Item
            {
                [JsonPropertyName("name")] public string Name { get; set; }
                [JsonPropertyName("signature")] public string Signature { get; set; }
            }
        }

        public record MinecraftPlayerUuidResponse
        {
            [JsonPropertyName("id")] public string Uuid { get; set; }
            [JsonPropertyName("name")] public string Name { get; set; }
            [JsonPropertyName("skins")] public List<Skin> Skins { get; set; }
            [JsonPropertyName("capes")] public List<Cape> Capes { get; set; }

            public record Skin
            {
                [JsonPropertyName("id")] public string Id { get; set; }
                [JsonPropertyName("state")] public string State { get; set; }
                [JsonPropertyName("url")] public string Url { get; set; }
                [JsonPropertyName("variant")] public string Variant { get; set; }
                [JsonPropertyName("textureKey")] public string TextureKey { get; set; }
                [JsonPropertyName("alias")] public string Alias { get; set; }
            }

            public record Cape(
                [property: JsonPropertyName("id")] string Id,
                [property: JsonPropertyName("state")] string State,
                [property: JsonPropertyName("url")] string Url,
                [property: JsonPropertyName("alias")] string Alias
            );
        }
    }
}