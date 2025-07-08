using System.Text.Json.Serialization;

namespace PCL.Neo.Core.Service.Accounts.IYggdrasilAuth.Data;

internal sealed record YggdrasilProfile
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;
}
