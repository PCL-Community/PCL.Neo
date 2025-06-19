namespace PCL.Neo.Core.Service.Accounts.MicrosoftAuth;

public static class DeviceCodeData
{
    public record DeviceCodeInfo(string device_code, string user_code, string verification_uri, int interval);

    public record DeviceCodeAccessToken(string AccessToken, string RefreshToken, DateTimeOffset ExpiresIn);

    public record McAccountInfo(
        List<Storage.Skin> Skins,//皮肤数据
        List<Storage.Cape> Capes,//披风数据
        string UserName,//玩家名
        string Uuid);//UUID
}