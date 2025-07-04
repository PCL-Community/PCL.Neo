using System.Text.Json.Serialization;

namespace PCL.Neo.Core.Service.Accounts.IYggdrasilAuth.Data;

internal sealed record YggdrasilErrorResponse
{
    [JsonPropertyName("error")]
    public string Error { get; set; } = string.Empty;

    [JsonPropertyName("errorMessage")]
    public string ErrorMessage { get; set; } = string.Empty;

    [JsonPropertyName("cause")]
    public string? Cause { get; set; }
}
