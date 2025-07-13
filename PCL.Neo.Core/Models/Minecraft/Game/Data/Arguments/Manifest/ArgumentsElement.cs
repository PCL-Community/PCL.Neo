using System.Text.Json.Serialization;

namespace PCL.Neo.Core.Models.Minecraft.Game.Data.Arguments.Manifest;

/// <summary>
/// Base class for any argument element (either a simple string or a ruled object).
/// </summary>
[JsonConverter(typeof(ArgumentElementConverter))]
public abstract class ArgumentElement;

/// <summary>
/// Represents a simple string argument.
/// </summary>
public class StringArgument(string value) : ArgumentElement
{
    public string Value { get; } = value;

    public override string ToString()
    {
        return $"String: \"{Value}\"";
    }
}

/// <summary>
/// Represents an argument that is applied based on a set of rules.
/// </summary>
public class RuledArgument : ArgumentElement
{
    [JsonPropertyName("rules")]
    public List<Rule> Rules { get; init; } = [];

    [JsonPropertyName("value")]
    [JsonConverter(typeof(StringOrStringListConverter))]
    public List<string> Value { get; init; } = [];

    public override string ToString()
    {
        return $"Ruled: Values='{string.Join(' ', Value)}', RulesCount={Rules.Count}";
    }
}
