namespace PCL.Neo.Core.Service.Accounts.Storage;

// Enum for Active/Inactive state, same as original
public enum AccountState
{
    Active,
    Inactive
}

// Extended UserTypeEnum
public static class UserTypeConstants
{
    public const string Msa = "msa";
    public const string Mojang = "mojang"; // Kept for reference, though not a primary target class here
    public const string Legacy = "legacy"; // Kept for reference
    public const string Yggdrasil = "yggdrasil";
    public const string Offline = "offline";
}

// Record for Cape, same as original but using AccountState
public record Cape(string Id, AccountState State, Uri Url, string Alias);

// Record for Skin, same as original but using AccountState
public record Skin(string Id, Uri Url, string Variant, string TextureKey, AccountState State);

// Record for OAuthTokenData, same as original
public record OAuthTokenData(string AccessToken, string RefreshToken, DateTimeOffset ExpiresAt);
