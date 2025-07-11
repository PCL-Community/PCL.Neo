using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace PCL.Neo.Core.Models.Minecraft;

public class VersionManifestFile
{
    #region Model Classes

    public class VersionModel
    {
        [JsonPropertyName("id")] public string Id { get; set; } = string.Empty;
        [JsonPropertyName("type")] public ReleaseTypeEnum Type { get; set; } = ReleaseTypeEnum.Unknown;
        [JsonPropertyName("url")] public string Url { get; set; } = string.Empty;
        [JsonPropertyName("time")] public string Time { get; set; } = string.Empty;
        [JsonPropertyName("releaseTime")] public string ReleaseTime { get; set; } = string.Empty;
    }

    #endregion

    private JsonObject _rawVersionManifest = new();

    [JsonPropertyName("latest")] public Dictionary<ReleaseTypeEnum, string> Latest { get; set; } = [];
    [JsonPropertyName("versions")] public List<VersionModel> Versions { get; set; } = [];

    #region Parse Methods

    public static VersionManifestFile Parse(string json) =>
        Parse(JsonNode.Parse(json)?.AsObject() ??
              throw new Exception($"{nameof(VersionManifestFile)} Deserialization returned null"));

    public static VersionManifestFile Parse(JsonNode json) =>
        Parse(json.AsObject());

    public static VersionManifestFile Parse(JsonObject json)
    {
        var vmf = json.Deserialize<VersionManifestFile>() ??
                  throw new Exception($"{nameof(VersionManifestFile)} Deserialization returned null");
        vmf._rawVersionManifest = json;
        return vmf;
    }

    #endregion
}