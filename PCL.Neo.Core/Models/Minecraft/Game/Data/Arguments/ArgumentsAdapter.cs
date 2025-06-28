using PCL.Neo.Core.Models.Minecraft.Game.Data.Arguments.Manifes;
using PCL.Neo.Core.Utils;
using System.Text.Json;

namespace PCL.Neo.Core.Models.Minecraft.Game.Data.Arguments;

public class ArgumentsAdapter
public class ArgumentsAdapter
{
    public Dictionary<string, string> Arguments { get; } = new();
    public CustomFeatures Features { get; } = new();

    private static VersionManifes GetVersionManifes(string gameDir, string versionId)
    {
        var fileName = $"{versionId}.json";
        var jsonFile = Path.Combine(gameDir, fileName);
        if (File.Exists(jsonFile) == false)
        {
            throw new FileNotFoundException("Version manifes file not found.", fileName);
        }

        var fileContent = File.ReadAllText(jsonFile);
        var versionManifes = JsonSerializer.Deserialize<VersionManifes>(fileContent,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        ArgumentNullException.ThrowIfNull(versionManifes);

        if (string.IsNullOrEmpty(versionManifes.Name))
        {
            versionManifes.Name = versionManifes.Id;
        }

        versionManifes.JsonOriginData = fileContent;

        return versionManifes;
    }

    public ArgumentsAdapter(GameInfo info, LaunchOptions options, Dictionary<string, string> extraOptions)
    public ArgumentsAdapter(GameInfo info, LaunchOptions options, Dictionary<string, string> extraOptions)
    {
        ArgumentNullException.ThrowIfNull(info);
        ArgumentNullException.ThrowIfNull(options);

        var versionManifes = GetVersionManifes(info.GameDirectory, options.VersionId);

        // game arguments
        Arguments.Add("${game_directory}", info.GameDirectory);
        Arguments.Add("${assets_root}",
            DirectoryUtil.QuotePath(Path.Combine(info.RootDirectory, "assets")));
        Arguments.Add("${auth_player_name}", options.Username);
        Arguments.Add("${version_name}", options.VersionId);
        Arguments.Add("${assets_index_name}", versionManifes.AssetIndex?.Id ?? "legacy");
        Arguments.Add("${auth_uuid}", options.UUID);
        Arguments.Add("${auth_access_token}", options.AccessToken);
        Arguments.Add("${clientid}", "unkonw"); // TODO: unknow arguments
        Arguments.Add("${auth_xuid}", "unknow"); // TODO: unknow arguments
        Arguments.Add("${user_type}", options.IsOfflineMode ? "legacy" : "msa");
        Arguments.Add("${version_type}", versionManifes.Type);
        Arguments.Add("${classpath}", info.ClassPath);
        Arguments.Add("${classpath_separator}", Const.Os == Const.RunningOs.Windows ? ";" : ":");
        Arguments.AddRange(extraOptions);

        // window arguments
        if (options.FullScreen == false)
        {
            Features.HasCustomResolution = true;
            Arguments.Add("${resolution_width}", options.WindowWidth.ToString());
            Arguments.Add("${resolution_height}", options.WindowHeight.ToString());
        }
    }
}