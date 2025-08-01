using PCL.Neo.Core.Models.Configuration;
using PCL.Neo.Core.Models.Configuration.Data;
using System.Collections.Immutable;
using System.Text.Json.Serialization;

namespace PCL.Neo.Core.Service.Accounts.OAuthService;

public static class OAuthData
{
    public static class RequestUrls
    {
        public static readonly Lazy<Uri> OAuth2BaseUri = new(() =>
            new Uri("https://login.microsoftonline.com/consumers/oauth2/v2.0"));

        /// <summary>
        /// 获取授权码模式下的授权码地址
        /// </summary>
        public static readonly Lazy<Uri> AuthCodeUri = new(() =>
            new Uri(OAuth2BaseUri.Value, "/authorize"));

        /// <summary>
        /// 获取设备码模式下的授权码地址
        /// </summary>
        public static readonly Lazy<Uri> DeviceCode = new(() =>
            new Uri(OAuth2BaseUri.Value, "/devicecode"));

        /// <summary>
        /// 获取令牌
        /// </summary>
        public static readonly Lazy<Uri> TokenUri = new(() =>
            new Uri(OAuth2BaseUri.Value, "/token"));

        /// <summary>
        /// XboxLive验证地址
        /// </summary>
        public static readonly Lazy<Uri> XboxLiveAuth = new(() =>
            new Uri("https://user.auth.xboxlive.com/user/authenticate"));

        /// <summary>
        /// Xsts验证地址
        /// </summary>
        public static readonly Lazy<Uri> XstsAuth = new(() =>
            new Uri("https://xsts.auth.xboxlive.com/xsts/authorize"));

        /// <summary>
        /// Mc通行令牌获取地址
        /// </summary>
        public static readonly Lazy<Uri> MinecraftAccessTokenUri = new(() =>
            new Uri("https://api.minecraftservices.com/authentication/login_with_xbox"));

        /// <summary>
        /// 检查是否拥有Mc地址
        /// </summary>
        public static readonly Lazy<Uri> CheckHasMc = new(() =>
            new Uri("https://api.minecraftservices.com/entitlements/mcstore"));

        /// <summary>
        /// 获取玩家UUID的地址
        /// </summary>
        public static readonly Lazy<Uri> PlayerUuidUri = new(() =>
            new Uri("https://api.minecraftservices.com/minecraft/profile"));
    }

    public static class FormUrlReqData
    {
        private static OAuth2Configurations? _configurations;

        private static OAuth2Configurations Configurations =>
            _configurations ??= ConfigurationManager.Instance.GetConfiguration<OAuth2Configurations>()!;

        /// <summary>
        /// 获取授权码的地址
        /// </summary>
        public static readonly Lazy<string> GetAuthCodeData = new(() =>
            $"{RequestUrls.AuthCodeUri}?client_id={Configurations.ClientId}&response_type=code&redirect_uri=127.0.0.1:{Configurations.RedirectPort}&response_mode=query&scope=XboxLive.signin offline_access");

        /// <summary>
        /// 设备码申请参数
        /// </summary>
        public static Lazy<IReadOnlyDictionary<string, string>> DeviceCodeData { get; } =
            new(() =>
                new Dictionary<string, string>
                    {
                        { "client_id", Configurations.ClientId }, { "scope", "XboxLive.signin offline_access" }
                    }
                    .ToImmutableDictionary());

        /// <summary>
        /// 用户授权状态查询参数
        /// </summary>
        public static Lazy<IReadOnlyDictionary<string, string>> UserAuthStateData { get; } =
            new(() =>
                new Dictionary<string, string>
                {
                    { "grant_type", "urn:ietf:params:oauth:grant-type:device_code" },
                    { "client_id", Configurations.ClientId }
                }.ToImmutableDictionary());

        /// <summary>
        /// 授权令牌参数
        /// </summary>
        public static Lazy<IReadOnlyDictionary<string, string>> AuthTokenData { get; } =
            new(() =>
                new Dictionary<string, string>
                {
                    { "client_id", Configurations.ClientId },
                    { "grant_type", "authorization_code" },
                    { "redirect_uri", $"127.0.0.1:{Configurations.RedirectPort}" },
                    { "scope", "XboxLive.signin offline_access" }
                }.ToImmutableDictionary());

        /// <summary>
        /// 刷新令牌参数
        /// </summary>
        public static Lazy<IReadOnlyDictionary<string, string>> RefreshTokenData { get; } =
            new(() => new Dictionary<string, string>
            {
                { "client_id", Configurations.ClientId },
                { "client_secret", Configurations.ClientSecret },
                { "grant_type", "refresh_token" },
                { "scope", "XboxLive.signin offline_access" }
            }.ToImmutableDictionary());
    }

    public static class RequireData
    {
        public sealed record XboxLiveAuthRequire
        {
            [property: JsonPropertyName("PropertiesData")]
            public required PropertiesData Properties { get; set; }

            public const string TokenType = "JWT";
            public static string RelyingParty => "http://auth.xboxlive.com";

            public sealed record PropertiesData(
                [property: JsonPropertyName("RpsTicket")]
                string RpsTicket)
            {
                public const string AuthMethod = "RPS";
                public const string SiteName = "user.auth.xboxlive.com";
            }
        }

        public sealed record XstsRequire(
            XstsRequire.PropertiesData Properties)
        {
            public const string RelyingParty = "rp://api.minecraftservices.com/";
            public const string TokenType = "JWT";

            public sealed record PropertiesData(
                [property: JsonPropertyName("UserTokens")]
                List<string> UserTokens)
            {
                public const string SandboxId = "RETAIL";
            }
        }

        public sealed record MinecraftAccessTokenRequire
        {
            [JsonPropertyName("identityToken")]
            public string IdentityToken { get; set; }
        }
    }

    public static class ResponseData
    {
        public sealed record AccessTokenResponse
        {
            [JsonPropertyName("expires_in")]
            public required int ExpiresIn { get; set; }

            [JsonPropertyName("ext_expires_in")]
            public required int ExtExpiresIn { get; set; }

            [JsonPropertyName("access_token")]
            public required string AccessToken { get; set; }

            [JsonPropertyName("refresh_token")]
            public required string RefreshToken { get; set; }
        }

        public sealed record UserAuthStateResponse
        {
            [property: JsonPropertyName("expires_in")]
            public required int ExpiresIn { get; set; }

            [property: JsonPropertyName("access_token")]
            public required string AccessToken { get; set; }

            [property: JsonPropertyName("refresh_token")]
            public required string RefreshToken { get; set; }

            [property: JsonPropertyName("error")]
            public string? Error { get; set; }

            [property: JsonPropertyName("error_description")]
            public string? ErrorDescription { get; set; }

            [property: JsonPropertyName("correlation_id")]
            public string? CorrelationId { get; set; }
        }

        public sealed record XboxResponse
        {
            /*
                        [JsonPropertyName("IssueInstant")] public DateTime IssueInstant { get; set; }
            */
            /*
                        [JsonPropertyName("NotAfter")] public DateTime NotAfter { get; set; }
            */
            [JsonPropertyName("Token")]
            public required string Token { get; set; }

            [JsonPropertyName("DisplayClaims")]
            public required DisplayClaimsData DisplayClaims { get; set; }

            public record DisplayClaimsData
            {
                [JsonPropertyName("xui")]
                public required List<XuiData> Xui { get; set; }

                public record XuiData
                {
                    [JsonPropertyName("uhs")]
                    public required string Uhs { get; set; }
                }
            }
        }

        public sealed record MinecraftAccessTokenResponse
        {
            [JsonPropertyName("username")]
            public required string Username { get; set; }

            [JsonPropertyName("roles")]
            public required List<object> Roles { get; set; }

            [JsonPropertyName("access_token")]
            public required string AccessToken { get; set; }

            [JsonPropertyName("token_type")]
            public required string TokenType { get; set; }

            [JsonPropertyName("expires_in")]
            public required int ExpiresIn { get; set; }
        }

        public sealed record CheckHaveGameResponse
        {
            [JsonPropertyName("items")]
            public required List<Item> Items { get; set; }
            /*
                        [JsonPropertyName("signature")] public string Signature { get; set; }
            */
            /*
                        [JsonPropertyName("keyId")] public string KeyId { get; set; }
            */

            public sealed record Item
            {
                [JsonPropertyName("name")]
                public required string Name { get; set; }

                [JsonPropertyName("signature")]
                public string? Signature { get; set; }
            }
        }

        public sealed record MinecraftPlayerUuidResponse
        {
            [JsonPropertyName("id")]
            public required string Uuid { get; set; }

            [JsonPropertyName("name")]
            public required string Name { get; set; }

            [JsonPropertyName("skins")]
            public List<Skin>? Skins { get; set; }

            [JsonPropertyName("capes")]
            public List<Cape>? Capes { get; set; }

            public sealed record Skin
            {
                [JsonPropertyName("id")]
                public required string Id { get; set; }

                [JsonPropertyName("state")]
                public required string State { get; set; }

                [JsonPropertyName("url")]
                public required string Url { get; set; }

                [JsonPropertyName("variant")]
                public required string Variant { get; set; }

                [JsonPropertyName("textureKey")]
                public required string TextureKey { get; set; }

                [JsonPropertyName("alias")]
                public string? Alias { get; set; }
            }

            public sealed record Cape
            {
                [JsonPropertyName("id")]
                public required string Id { get; set; }

                [JsonPropertyName("state")]
                public required string State { get; set; }

                [JsonPropertyName("url")]
                public required string Url { get; set; }

                [JsonPropertyName("alias")]
                public string? Alias { get; set; }
            }
        }
    }
}
