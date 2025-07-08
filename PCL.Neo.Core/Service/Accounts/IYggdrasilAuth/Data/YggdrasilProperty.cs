using System.Text.Json.Serialization;

namespace PCL.Neo.Core.Service.Accounts.IYggdrasilAuth.Data;

internal sealed record YggdrasilProperty
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("value")]
    public string Value { get; set; } = string.Empty;

    [JsonPropertyName("signature")]
    public string? Signature { get; set; }
}
