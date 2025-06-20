using System.Text.Json.Serialization;

namespace PCL.Neo.Core.Service.Accounts.MicrosoftAuth;

public static class DeviceCodeData
{
    public record DeviceCodeInfo(
        [property: JsonPropertyName("device_code")] string DeviceCode,
        [property: JsonPropertyName("user_code")] string UserCode,
        [property: JsonPropertyName("verification_uri")] string VerificationUri,
        [property: JsonPropertyName("interval")] int Interval);

    public record DeviceCodeAccessToken(string AccessToken, string RefreshToken, DateTimeOffset ExpiresIn);

    public record McAccountInfo(
        List<Storage.Skin> Skins,//皮肤数据
        List<Storage.Cape> Capes,//披风数据
        string UserName,//玩家名
        string Uuid);//UUID
}