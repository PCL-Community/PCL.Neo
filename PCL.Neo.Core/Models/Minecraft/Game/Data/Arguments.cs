using PCL.Neo.Core.Models.Minecraft.Game.Data.GaJvArguments;
using System.Text.Json.Serialization;

namespace PCL.Neo.Core.Models.Minecraft.Game.Data;

public sealed record Arguments
{
    [JsonPropertyName("jvm")]
    [JsonConverter(typeof(ListOfArgumentElementConverter))]
    public List<ArgumentElement> Jvm { get; set; } = [];

    [JsonPropertyName("game")]
    [JsonConverter(typeof(ListOfArgumentElementConverter))]
    public List<ArgumentElement> Game { get; set; } = [];
}