using System.Text.Json.Serialization;

namespace PCL.Neo.Core.Models.Minecraft.Mod.Data;

/// <summary>
/// 模组信息
/// </summary>
internal record FabricModInfo
{
    public record ContactInfo
    {
        [JsonPropertyName("email ")]
        public string Email { get; set; } = string.Empty;

        [JsonPropertyName("irc")]
        public string Irc { get; set; } = string.Empty;

        [JsonPropertyName("homepage")]
        public string Homepage { get; set; } = string.Empty;

        [JsonPropertyName("issues")]
        public string Issues { set; get; } = string.Empty;

        [JsonPropertyName("sources")]
        public string Sources { get; set; } = string.Empty;
    }

    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("version")]
    public string Version { get; set; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;

    [JsonPropertyName("icon")]
    public string Icon { get; set; } = string.Empty;

    [JsonPropertyName("contact")]
    public ContactInfo? Contact { get; set; }
}
