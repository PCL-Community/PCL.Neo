using System.Text.Json.Serialization;

namespace PCL.Neo.Core.Models.Minecraft.Game.Data.Arguments.Manifes;

public sealed record Arguments
{
    [JsonPropertyName("game")]
    public List<object>? Game { get; set; } = new();

    [JsonPropertyName("jvm")]
    public List<object>? Jvm { get; set; } = new();

    public List<string> GameArguments { get; init; } = new();

    private static readonly Dictionary<string, bool> GameRules = new()
    {
        { "is_demo_user", false },
        { "has_custom_resolution", true },
        { "has_quick_plays_support", false },
        { "is_quick_play_singleplayer", false },
        { "is_quick_play_multiplayer", false },
        { "is_quick_play_realms", false }
    };

    /// <summary>
    /// The Custom Values for the arguments. Init needed.
    /// </summary>
    public static Dictionary<string, string> ArgumentsCustomValue = new()
    {
        { "auth_player_name", string.Empty },
        { "version_name", string.Empty },
        { "game_directory", string.Empty },
        { "assets_root", string.Empty },
        { "assets_index_name", string.Empty },
        { "auth_uuid", string.Empty },
        { "auth_access_token", string.Empty },
        { "clientid", string.Empty },
        { "auth_xuid", string.Empty },
        { "user_type", string.Empty },
        { "version_type", string.Empty },
        { "resolution_width", string.Empty },
        { "resolution_height", string.Empty },
        { "quickPlayPath", string.Empty },
        { "quickPlaySingleplayer", string.Empty },
        { "quickPlayMultiplayer", string.Empty },
        { "quickPlayRealms", string.Empty },
        { "natives_directory", string.Empty },
        { "launcher_name", string.Empty },
        { "launcher_version", string.Empty },
        { "classpath", string.Empty }
    };

    private static IEnumerable<string> ReplaceCustomValue(IEnumerable<string> arguments)
    {
        List<string> result = [];
        foreach (var arg in arguments)
        {
            var match = CustomValueRegex().Match(arg);
            if (!match.Success)
            {
                result.Add(arg);
                continue;
            }

            var key = match.Groups[1].Value;
            if (ArgumentsCustomValue.TryGetValue(key, out var value))
            {
                CustomValueRegex().Replace(arg, value);
                result.Add(arg);
            }
            else
            {
                throw new ArgumentException($"Requied value ${key} not found.");
            }
        }

        return result;
    }

    /*
    public Arguments(MetadataFile metadata)
    {
        Game = new List<object>();
        Jvm = new List<object>();

        var arguments = metadata.Arguments;

        var gameArgu = arguments.Game
            .Where(it => it.Rules is null || GameArgumentsFilter(it.Rules.FirstOrDefault()!))
            .SelectMany(it => it.Value);

        var jvmArgu = arguments.Jvm
            .Where(it => it.Rules is null || JvmArgumentsFilter(it.Rules.FirstOrDefault()!))
            .SelectMany(it => it.Value);

        var gameResult = ReplaceCustomValue(gameArgu);
        var jvmResult = ReplaceCustomValue(jvmArgu);

        GameArguments = gameResult.Concat(jvmResult).ToList();
    }
    */

    /// <inheritdoc />
    public override string ToString()
    {
        return string.Join(" ", GameArguments);
    }

    [GeneratedRegex(@"\$\{([^}]+)\}")]
    private static partial Regex CustomValueRegex();
}
