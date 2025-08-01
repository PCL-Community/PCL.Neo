using PCL.Neo.Core.Models.Minecraft.Game.Data.Arguments.Manifest;
using PCL.Neo.Core.Utils;

namespace PCL.Neo.Core.Models.Minecraft.Game.Data.Arguments;

public class ArgumentsAdapter
{
    public Dictionary<string, string> Arguments { get; } = new();
    public Dictionary<string, bool> Features { get; } = new();

    public ArgumentsAdapter(
        GameInfo info,
        LaunchOptions options,
        Dictionary<string, string> extraArgs,
        Manifest.VersionManifest manifest)
    {
        ArgumentNullException.ThrowIfNull(info);
        ArgumentNullException.ThrowIfNull(options);

        // game arguments
        Arguments.Add("${game_directory}", info.GameDirectory);
        Arguments.Add("${assets_root}",
            DirectoryUtil.QuotePath(Path.Combine(info.RootDirectory, "assets")));
        Arguments.Add("${game_assets}",
            DirectoryUtil.QuotePath(Path.Combine(info.RootDirectory, "assets")));
        Arguments.Add("${auth_player_name}", options.Username);
        Arguments.Add("${auth_session}", string.Empty); // idk what auth_session is...
        Arguments.Add("${version_name}", info.Name);
        Arguments.Add("${assets_index_name}", manifest.AssetIndex?.Id ?? "legacy");
        Arguments.Add("${auth_uuid}", options.UUID);
        Arguments.Add("${auth_access_token}", options.AccessToken);
        Arguments.Add("${auth_xuid}", "unknow");
        Arguments.Add("${clientid}", "unknow");
        Arguments.Add("${user_type}", options.IsOfflineMode ? "legacy" : "msa");
        Arguments.Add("${version_type}", manifest.Type);
        Arguments.Add("${classpath_separator}", Const.Os == Const.RunningOs.Windows ? ";" : ":");
        Arguments.AddRange(extraArgs);

        // window arguments
        if (options.FullScreen == false)
        {
            Features.Add("has_custom_resolution", true);
            Arguments.Add("${resolution_width}", options.WindowWidth.ToString());
            Arguments.Add("${resolution_height}", options.WindowHeight.ToString());
        }
    }
}
