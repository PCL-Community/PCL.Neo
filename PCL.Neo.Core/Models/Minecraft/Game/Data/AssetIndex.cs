using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace PCL.Neo.Core.Models.Minecraft.Game.Data
{
    public class AssetIndex
    {
        [JsonPropertyName("objects")]
        public Dictionary<string, AssetObject> Objects { get; set; } = new Dictionary<string, AssetObject>();
        
        [JsonPropertyName("map_to_resources")]
        public bool MapToResources { get; set; }
    }

    public class AssetObject
    {
        [JsonPropertyName("hash")]
        public string Hash { get; set; } = string.Empty;
        
        [JsonPropertyName("size")]
        public int Size { get; set; }
    }
} 