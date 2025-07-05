using System.Text.Json.Serialization;

namespace PCL.Neo.Core.Service.Game.Data;

internal record LatestRepoRelease
{
    [JsonPropertyName("assets")] public required List<AssetsInfo> Assets { get; init; }

    internal record AssetsInfo
    {
        [JsonPropertyName("browser_download_url")]
        public required string DownloadUrl { get; init; }
    }
}