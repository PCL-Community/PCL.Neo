using System.Text.Json.Serialization;

namespace PCL.Neo.Core.Service.Accounts.IYggdrasilAuth.Data;

internal sealed record YggdrasilUser
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("properties")]
    public List<YggdrasilProperty>? Properties { get; set; }
}
