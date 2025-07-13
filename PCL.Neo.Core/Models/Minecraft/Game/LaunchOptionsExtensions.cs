using PCL.Neo.Core.Models.Minecraft.Game.Data;
using System.Text;
using System.Text.Json;

namespace PCL.Neo.Core.Models.Minecraft.Game;

public static class LaunchOptionsExtensions // note: i dont konw why does extension method doesnt work...
{
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

#nullable disable
    public static async Task<LaunchOptions> GetLaunchOptions(string gameDir)
    {
        var optionsPath = GetOptionsFilePath(gameDir);

        if (!File.Exists(optionsPath))
        {
            throw new FileNotFoundException("Launch options file not found.", "launch_options.json");
        }

        var content = await File.ReadAllTextAsync(optionsPath);
        var options = JsonSerializer.Deserialize<LaunchOptions>(content, JsonOptions);

        return options;
    }
#nullable restore

    public static async Task SaveLaunchOptions(string gameDir, LaunchOptions options)
    {
        var optionsPath = GetOptionsFilePath(gameDir);
        var content = JsonSerializer.Serialize(options);

        await File.WriteAllTextAsync(optionsPath, content, Encoding.UTF8);
    }

    public static async Task UpdateLaunchOptions(string gameDir, LaunchOptions options)
    {
        var optionsPath = GetOptionsFilePath(gameDir);

        if (!File.Exists(optionsPath))
        {
            throw new FileNotFoundException("Launch options file not found.", "launch_options.json");
        }

        var content = JsonSerializer.Serialize(options);

        await File.WriteAllTextAsync(optionsPath, content, Encoding.UTF8);
    }

    private static string GetOptionsFilePath(string gameDir)
    {
        return Path.Combine(gameDir, "launch_options");
    }
}
