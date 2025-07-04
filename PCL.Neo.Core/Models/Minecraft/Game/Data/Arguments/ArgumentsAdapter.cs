using PCL.Neo.Core.Models.Minecraft.Game.Data.Arguments.Manifes;
using PCL.Neo.Core.Utils;

namespace PCL.Neo.Core.Models.Minecraft.Game.Data.Arguments;

public class ArgumentsAdapter
{
    public Dictionary<string, string> Arguments { get; } = new();
    public CustomFeatures Features { get; } = new();

    public ArgumentsAdapter(
        GameInfo info,
        LaunchOptions options,
        Dictionary<string, string> extraArgs,
        VersionManifes manifes)
    {
        ArgumentNullException.ThrowIfNull(info);
        ArgumentNullException.ThrowIfNull(options);

        // game arguments
        Arguments.Add("${game_directory}", info.GameDirectory);
        Arguments.Add("${assets_root}",
            DirectoryUtil.QuotePath(Path.Combine(info.RootDirectory, "assets")));
        Arguments.Add("${auth_player_name}", options.Username);
        Arguments.Add("${version_name}", options.VersionId);
        Arguments.Add("${assets_index_name}", manifes.AssetIndex?.Id ?? "legacy");
        Arguments.Add("${auth_uuid}", options.UUID);
        Arguments.Add("${auth_access_token}", options.AccessToken);
        Arguments.Add("${auth_xuid}", "unknow");
        Arguments.Add("${clientid}", "unknow");
        Arguments.Add("${user_type}", options.IsOfflineMode ? "legacy" : "msa");
        Arguments.Add("${version_type}", manifes.Type);
        Arguments.Add("${classpath_separator}", Const.Os == Const.RunningOs.Windows ? ";" : ":");
        Arguments.AddRange(extraArgs);

        // window arguments
        if (options.FullScreen == false)
        {
            Features.HasCustomResolution = true;
            Arguments.Add("${resolution_width}", options.WindowWidth.ToString());
            Arguments.Add("${resolution_height}", options.WindowHeight.ToString());
        }
    }
}