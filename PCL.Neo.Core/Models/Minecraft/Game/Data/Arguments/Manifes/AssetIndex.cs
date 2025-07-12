using System.Text.Json.Serialization;

namespace PCL.Neo.Core.Models.Minecraft.Game.Data.Arguments.Manifes;

public class AssetIndex
{
    [JsonPropertyName("objects")]
    public Dictionary<string, AssetObject> Objects { get; init; } = new();

    [JsonPropertyName("map_to_resources")]
    public bool MapToResources { get; init; }
}

public class AssetObject
{
    [JsonPropertyName("hash")]
    public string Hash { get; set; } = string.Empty;

    [JsonPropertyName("size")]
    public int Size { get; set; }
}
