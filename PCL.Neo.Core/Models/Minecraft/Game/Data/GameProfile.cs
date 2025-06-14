using System.Text.Json.Serialization;

namespace PCL.Neo.Core.Models.Minecraft.Game.Data;

public record GameProfile
{
    [JsonPropertyName("GameInfomation")] public required GameInfo Information { get; init; }
    [JsonPropertyName("LaunchOptions")] public required LaunchOptions Options { get; init; }
}