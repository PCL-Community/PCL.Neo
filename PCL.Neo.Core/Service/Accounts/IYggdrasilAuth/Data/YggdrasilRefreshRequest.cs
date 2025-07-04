using System.Text.Json.Serialization;

namespace PCL.Neo.Core.Service.Accounts.IYggdrasilAuth.Data;

internal sealed record YggdrasilRefreshRequest
{
    [JsonPropertyName("accessToken")]
    public string AccessToken { get; set; } = string.Empty;

    [JsonPropertyName("clientToken")]
    public string ClientToken { get; set; } = string.Empty;

    [JsonPropertyName("requestUser")]
    public bool RequestUser { get; set; } = true;
}
