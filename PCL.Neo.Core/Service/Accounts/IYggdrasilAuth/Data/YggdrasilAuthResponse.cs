using PCL.Neo.Core.Service.Accounts.IYggdrasilAuth.Data;
using System.Text.Json.Serialization;

namespace PCL.Neo.Core.Service.Accounts.IYggdrasilAuth;

internal sealed record YggdrasilAuthResponse
{
    [JsonPropertyName("accessToken")]
    public string AccessToken { get; set; } = string.Empty;

    [JsonPropertyName("clientToken")]
    public string ClientToken { get; set; } = string.Empty;

    [JsonPropertyName("selectedProfile")]
    public YggdrasilProfile? SelectedProfile { get; set; }

    [JsonPropertyName("availableProfiles")]
    public List<YggdrasilProfile>? AvailableProfiles { get; set; }

    [JsonPropertyName("user")]
    public YggdrasilUser? User { get; set; }
}
