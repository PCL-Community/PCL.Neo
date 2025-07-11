using PCL.Neo.Core.Models.Minecraft.Game.Data;
using PCL.Neo.Core.Service.Profiles.Data;
using PCL.Neo.Core.Utils.Logger;
using System.Text.Json;

namespace PCL.Neo.Core.Service.Profiles;

public class ProfileService : IProfileService
{
    private static readonly string[] RequiredSubDirectories = ["assets", "libraries", "versions"];

    /// <inheritdoc />
    public async Task<IEnumerable<ProfileInfo>> LoadProfilesDefaultAsync()
    {
        var path = GetDefaultProfileConfigPath();
        return await LoadProfilesAsync(path);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<ProfileInfo>> LoadProfilesAsync(string targetDir)
    {
        var profiles = Directory.GetFiles(targetDir, "*.json", SearchOption.TopDirectoryOnly);
        var contents = new List<string>();

        foreach (var profile in profiles)
        {
            contents.Add(await File.ReadAllTextAsync(profile));
        }

        return contents
            .Select(content => JsonSerializer.Deserialize<ProfileInfo>(content))
            .OfType<ProfileInfo>();
    }


    /// <inheritdoc />
    public async Task<ProfileInfo> GetProfileAsync(string targetDir, string profileName)
    {
        if (!ValidateDir(targetDir))
        {
            throw new InvalidOperationException(
                $"Target directory '{targetDir}' is missing required subdirectories: {string.Join(", ", RequiredSubDirectories)}");
        }

        var versionsDir = Path.Combine(targetDir, "versions");
        var versions = Directory.GetDirectories(versionsDir, "*", SearchOption.TopDirectoryOnly);

        var profiles = new ProfileInfo { ProfileName = profileName, TargetDir = targetDir, Games = [] };

        foreach (var version in versions)
        {
            var versionName = Path.GetFileName(version);
            ArgumentException.ThrowIfNullOrEmpty(versionName, nameof(versionName));

            var jarFile = Path.Combine(version, $"{versionName}.jar");
            var jsonFile = Path.Combine(version, $"{versionName}.json");

            if (!ValidateVersionDir(jarFile, jsonFile))
            {
                throw new FileNotFoundException(
                    $"Game files not found. Expected files:\n- {jarFile}\n- {jsonFile}");
            }

            var isIndie = Directory.Exists(Path.Combine(version, "saves"));
            var gameType = await GetGameType(version, versionName);
            profiles.Games.Add(GameInfo.Factory(targetDir, version, versionName, isIndie, gameType));
        }

        return profiles;
    }

    /// <inheritdoc />
    public async Task<GameInfo> LoadTargetGameAsync(string targetDir, string gameName)
    {
        if (!ValidateDir(targetDir))
        {
            throw new InvalidOperationException(
                $"Target directory '{targetDir}' is missing required subdirectories: {string.Join(", ", RequiredSubDirectories)}");
        }

        var gameDir = Path.Combine(targetDir, "versions", gameName);
        if (!Directory.Exists(gameDir))
        {
            throw new DirectoryNotFoundException(
                $"Game directory not found. Expected path: {gameDir}");
        }

        var jarFile = Path.Combine(gameDir, $"{gameName}.jar");
        var jsonFile = Path.Combine(gameDir, $"{gameName}.json");

        if (!ValidateVersionDir(jarFile, jsonFile))
        {
            throw new FileNotFoundException(
                $"Game files not found. Expected files:\n- {jarFile}\n- {jsonFile}");
        }

        var isIndie = Directory.Exists(Path.Combine(gameDir, "saves"));
        var gameType = await GetGameType(gameDir, gameName);
        var gameInfo = GameInfo.Factory(targetDir, gameDir, gameName, isIndie, gameType);

        return gameInfo;
    }

    /// <inheritdoc />
    public Task<bool> SaveProfilesAsync(string targetDir, ProfileInfo profile)
    {
        var profilePath = GetProfileFilePath(targetDir, profile.ProfileName);
        return SaveProfileToFileAsync(profile, profilePath);
    }

    /// <inheritdoc />
    public Task<bool> SaveProfilesDefaultAsync(ProfileInfo profile)
    {
        var profileConfigFolder = GetDefaultProfileConfigPath();
        var profileFilePath = GetProfileFilePath(profileConfigFolder, profile.ProfileName);
        return SaveProfileToFileAsync(profile, profileFilePath);
    }

    /// <inheritdoc />
    public Task<bool> SaveGameInfoToProfileDefaultAsync(ProfileInfo profile, GameInfo game)
    {
        profile.Games.Add(game);
        var profileConfigFolder = GetDefaultProfileConfigPath();
        var profileFilePath = GetProfileFilePath(profileConfigFolder, profile.ProfileName);
        return SaveProfileToFileAsync(profile, profileFilePath);
    }

    /// <inheritdoc />
    public Task<bool> SaveGameInfoToProfileAsync(ProfileInfo profile, GameInfo game, string targetDir)
    {
        profile.Games.Add(game);
        var profilePath = GetProfileFilePath(targetDir, profile.ProfileName);
        return SaveProfileToFileAsync(profile, profilePath);
    }

    /// <inheritdoc />
    public bool DeleteGame(GameInfo game, ProfileInfo profile)
    {
        profile.Games.Remove(game);
        var gamePath = game.GameDirectory;

        try
        {
            Directory.Delete(gamePath);
        }
        catch (Exception e)
        {
            NewLogger.Logger.LogError($"Can't delete target game: {game.Name}", e);
            return false;
        }

        return true;
    }

    private static bool ValidateDir(string targetDir) =>
        RequiredSubDirectories.All(subDir =>
            Directory.Exists(Path.Combine(targetDir, subDir)));

    private static bool ValidateVersionDir(string jarFile, string jsonFile) =>
        File.Exists(jarFile) && File.Exists(jsonFile);

    private static string GetProfileFilePath(string configDir, string profileName) =>
        Path.Combine(configDir, $"{profileName}.json");

    private static string GetDefaultProfileConfigPath()
    {
        var appDataFolder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        var neoFolder = Path.Combine(appDataFolder, "PCL.Neo");
        return Path.Combine(neoFolder, "profiles");
    }

    private static void EnsureDirectoryExists(string path)
    {
        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }
    }

    private static async Task<bool> SaveProfileToFileAsync(ProfileInfo profile, string filePath)
    {
        try
        {
            EnsureDirectoryExists(Path.GetDirectoryName(filePath)!);

            var profileContent = JsonSerializer.Serialize(profile);
            await File.WriteAllTextAsync(filePath, profileContent);
            return true;
        }
        catch (Exception e)
        {
            var errorMessage = $"Failed to save profile to '{filePath}'. Profile name: {profile.ProfileName}";
            NewLogger.Logger.LogError(errorMessage, e);
            return false;
        }
    }

    private static async Task<GameType> GetGameType(string gameDir, string gameName) =>
        await VersionTypeHelper.GetGameType(gameDir, gameName);
}
