using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace PCL.Neo.Core.Models.Minecraft;

public class AssetIndexFile
{
    public class AssetInfo
    {
        [JsonPropertyName("hash")] public string Hash { get; set; } = string.Empty;
        [JsonPropertyName("size")] public int Size { get; set; } = 0;
    }

    private JsonObject _rawAssetIndex = new();

    [JsonPropertyName("map_to_resources")] public bool? MapToResources { get; set; }
    [JsonPropertyName("objects")] public Dictionary<string, AssetInfo> Objects { get; set; } = [];

    public static AssetIndexFile Parse(string json) =>
        Parse(JsonNode.Parse(json)?.AsObject() ??
              throw new Exception($"{nameof(AssetIndexFile)} Deserialization returned null"));

    public static AssetIndexFile Parse(JsonNode json) =>
        Parse(json.AsObject());

    public static AssetIndexFile Parse(JsonObject json)
    {
        var aif = json.Deserialize<AssetIndexFile>() ??
                  throw new Exception($"{nameof(AssetIndexFile)} Deserialization returned null");
        aif._rawAssetIndex = json;
        return aif;
    }
}