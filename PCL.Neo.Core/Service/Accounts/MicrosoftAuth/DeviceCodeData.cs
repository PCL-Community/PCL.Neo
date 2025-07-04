using PCL.Neo.Core.Service.Accounts.Storage;

namespace PCL.Neo.Core.Service.Accounts.MicrosoftAuth;

public static class DeviceCodeData
{
    public record DeviceCodeInfo(string DeviceCode, string UserCode, string VerificationUri, int Interval);

    public record DeviceCodeAccessToken(string AccessToken, string RefreshToken, DateTimeOffset ExpiresIn);

    public record McAccountInfo(
        List<Skin> Skins,
        List<Cape> Capes,
        string UserName,
        string Uuid);
}
