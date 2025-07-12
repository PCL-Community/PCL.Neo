using System.Text.Json.Serialization;

namespace PCL.Neo.Core.Models.Minecraft.Game.Data.Arguments.Manifes;

public sealed record Arguments
{
    [JsonPropertyName("jvm")]
    [JsonConverter(typeof(ListOfArgumentElementConverter))]
    public List<ArgumentElement> Jvm { get; init; } = [];

    [JsonPropertyName("game")]
    [JsonConverter(typeof(ListOfArgumentElementConverter))]
    public List<ArgumentElement> Game { get; init; } = [];
}
