using PCL.Neo.Core.Models.Minecraft.Game.Data;
using PCL.Neo.Core.Utils.Logger;
using System.Text.Json;

namespace PCL.Neo.Core.Service.Profiles;

internal class VersionTypeHelper
{
    /// <summary>
    /// Get game type from the version manifest file.
    /// </summary>
    /// <param name="gamePath">Game path.</param>
    /// <param name="gameName">Game name.</param>
    /// <returns>Game type.</returns>
    /// <exception cref="FileNotFoundException">Failed to get verision manifest file.</exception>
    /// <exception cref="ArgumentNullException">Failed to get release time property.</exception>
    public static async Task<GameType> GetGameType(string gamePath, string gameName)
    {
        var versionManifestName = $"{gameName}.json";
        var versionManifestPath = Path.Combine(gamePath, versionManifestName);

        if (!File.Exists(versionManifestPath))
        {
            var ex = new FileNotFoundException("Version manifest file not found.", versionManifestName);
            NewLogger.Logger.LogError("Version manifest file not found.", ex);
            throw ex;
        }

        var manifest = await File.ReadAllTextAsync(versionManifestPath);
        using var jsonDoc = JsonDocument.Parse(manifest);
        var root = jsonDoc.RootElement;

        var releaseTime = root.GetProperty("releaseTime").GetString() ??
                          throw new ArgumentNullException(nameof(root), "Game release time not found.");

        GameType gameType = GameType.Unknown;


        if (IsFool(releaseTime))
        {
            gameType |= GameType.Fool;
        }

        gameType |= DetermineLoaderType(ref root, ref manifest);

        var type = root.GetProperty("type").GetString() ??
                   throw new ArgumentNullException(nameof(root), "Game type not found.");

        if (IsSnapshot(type))
        {
            gameType |= GameType.Snapshot;
        }
        else
        {
            gameType |= GameType.Release;
        }

        return gameType;
    }

    private static bool IsFool(string timeStamp)
    {
        var time = DateTimeOffset.Parse(timeStamp);

        return time is { Month: 4, Day: 1 };
    }

    private static bool IsSnapshot(string type) =>
        string.Equals(type, "snapshot", StringComparison.OrdinalIgnoreCase);

    private static GameType DetermineLoaderType(ref JsonElement root, ref string originContent)
    {
        if (root.TryGetProperty("labymod_data", out JsonElement value))
        {
            return GameType.Labymod;
        }
        else if (originContent.Contains("net.neoforged", StringComparison.OrdinalIgnoreCase))
        {
            return GameType.NeoForge;
        }
        else if (originContent.Contains("net.fabricmc:fabric-loader", StringComparison.OrdinalIgnoreCase))
        {
            return GameType.Fabric;
        }
        else if (originContent.Contains("minecraftforge", StringComparison.OrdinalIgnoreCase))
        {
            return GameType.Forge;
        }
        else if (originContent.Contains("org.quiltmc:quilt-loader", StringComparison.OrdinalIgnoreCase))
        {
            return GameType.Quit;
        }
        else if (originContent.Contains("com.cleanroommc:cleanroom:", StringComparison.OrdinalIgnoreCase))
        {
            return GameType.Cleanroom;
        }
        else
        {
            return GameType.Vanilla;
        }
    }
}