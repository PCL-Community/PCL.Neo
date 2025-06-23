using System.Text.Json;
using System.Text.Json.Serialization;

namespace PCL.Neo.Core.Models.Minecraft.Game.Data.GaJvArguments;

/// <summary>
/// Base class for any argument element (either a simple string or a ruled object).
/// </summary>
public abstract class ArgumentElement
{
}

/// <summary>
/// Represents a simple string argument.
/// </summary>
public class StringArgument(string value) : ArgumentElement
{
    public string Value { get; } = value;
    public override string ToString() => $"String: \"{Value}\"";
}

/// <summary>
/// Represents an argument that is applied based on a set of rules.
/// </summary>
public class RuledArgument : ArgumentElement
{
    [JsonPropertyName("rules")] public List<Rule> Rules { get; set; } = [];

    // The 'value' can be a string or List<string> in JSON.
    [JsonPropertyName("value")]
    [JsonConverter(typeof(StringOrStringListConverter))]
    public List<string> Value { get; set; } = [];

    public override string ToString() => $"Ruled: Values='{string.Join(" ", Value)}', RulesCount={Rules.Count}";
}

/// <summary>
/// Defines a rule with an action and optional OS/feature conditions.
/// </summary>
public class Rule
{
    [JsonPropertyName("action")] public string Action { get; set; } = "allow"; // Default to "allow" if not specified

    [JsonPropertyName("os")] public OsSpec? Os { get; set; }

    [JsonPropertyName("features")] public FeaturesSpec? Features { get; set; }
}

/// <summary>
/// Specifies OS-related conditions for a rule.
/// </summary>
public class OsSpec
{
    [JsonPropertyName("name")] public string? Name { get; set; } // e.g., "windows", "osx", "linux"

    [JsonPropertyName("version")] public string? Version { get; set; } // Regex pattern

    [JsonPropertyName("arch")] public string? Arch { get; set; } // e.g., "x86", "x64"
}

/// <summary>
/// Specifies feature-related conditions for a rule.
/// Boolean properties are nullable to distinguish "not set" from "set to false".
/// </summary>
public class FeaturesSpec
{
    public KeyValuePair<string, JsonElement> Feature { get; set; }
}
