using System.Runtime.InteropServices;
using System.Text.Json.Serialization;

namespace PCL.Neo.Core.Models.Minecraft.Game.Data;

public record Arguments
{
    [JsonPropertyName("game")] public List<object>? Game { get; set; } = [];

    [JsonPropertyName("jvm")] public List<object>? Jvm { get; set; } = [];
    public List<string> GameArguments { get; init; } = [];
}