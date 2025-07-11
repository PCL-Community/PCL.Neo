using PCL.Neo.Core.Models.Minecraft.Game.Data;
using PCL.Neo.Core.Utils.Logger;
using System.Text.Json;

namespace PCL.Neo.Core.Service.Profiles;

internal class VersionTypeHelper
{
    public static async Task<GameType> GetGameType(string gamePath, string gameName)
    {
        var manifestName = $"{gameName}.json";
        var manifestPath = Path.Combine(gamePath, manifestName);

        if (!File.Exists(manifestPath))
        {
            var ex = new FileNotFoundException("Version manifest file not found.", manifestName);
            NewLogger.Logger.LogError("Version manifest file not found.", ex);
            throw ex;
        }

        var manifestContent = await File.ReadAllTextAsync(manifestPath);
        using var jsonDoc = JsonDocument.Parse(manifestContent);
        var root = jsonDoc.RootElement;
        var releaseTime = root.GetProperty("releaseTime").GetString() ??
                          throw new ArgumentNullException(nameof(root), "Game release time not found.");

        GameType type = GameType.Unknown;

        if (IsFool(releaseTime))
        {
            type |= GameType.Fool;
        }

        var jsonType = root.GetProperty("type").GetString() ??
                       throw new ArgumentNullException(nameof(root), "Game type not found.");

        if (IsSnapshot(jsonType))
        {
            type |= GameType.Snapshot;
        }

        type |= GetLoaderType(manifestContent);

        return type;
    }

    private static bool IsFool(string timeStamp)
    {
        var time = DateTimeOffset.Parse(timeStamp);

        return time is { Month: 4, Day: 1 };
    }

    private static bool IsSnapshot(string type) =>
        string.Equals(type, "snapshot", StringComparison.OrdinalIgnoreCase);

    private static GameType GetLoaderType(string jsonContent)
    {
        var type = GameType.Unknown;
        if (jsonContent.Contains("optifine", StringComparison.OrdinalIgnoreCase))
        {
            type |= GameType.Optifine;
        }

        if (jsonContent.Contains("liteloader", StringComparison.OrdinalIgnoreCase))
        {
            type |= GameType.LiteLoader;
        }

        type |= jsonContent switch
        {
            _ when jsonContent.Contains("minecraftforge", StringComparison.OrdinalIgnoreCase) &&
                   !jsonContent.Contains("net.neoforge") =>
                GameType.Forge,
            _ when jsonContent.Contains("net.neoforge", StringComparison.OrdinalIgnoreCase) =>
                GameType.NeoForge,
            _ when jsonContent.Contains("net.fabricmc:fabric-loader", StringComparison.OrdinalIgnoreCase) =>
                GameType.Fabric,
            _ when jsonContent.Contains("org.quiltmc:quilt-loader", StringComparison.OrdinalIgnoreCase) =>
                GameType.Quilt,
            _ when jsonContent.Contains("labymod_data", StringComparison.OrdinalIgnoreCase) =>
                GameType.Labymod,
            _ when jsonContent.Contains("com.cleanroommc:cleanroom:", StringComparison.OrdinalIgnoreCase) =>
                GameType.Cleanroom,
            _ => GameType.Vanilla
        };

        return type;
    }
}

