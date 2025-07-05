using PCL.Neo.Core.Models.Minecraft.Game.Data;
using PCL.Neo.Core.Service.Profiles.Data;

namespace PCL.Neo.Core.Service.Profiles;

public interface IProfileService
{
    /// <summary>
    /// Load profile from the specified directory.
    /// </summary>
    /// <param name="targetDir">The directory where profile are located.</param>
    /// <returns>A collection of game profile.</returns>
    /// <exception cref="InvalidOperationException">Target directory is not vaild.</exception>
    /// <exception cref="FileNotFoundException">Game '.jar' and '.json' file not found.</exception>
    Task<ProfileInfo> LoadProfileAsync(string targetDir);

    /// <summary>
    /// Load profile from the specified directory by profile name.
    /// </summary>
    /// <param name="targetDir">The directory where the profile is located.</param>
    /// <param name="gameName">The name of the profile to load.</param>
    /// <returns>A game profile object.</returns>
    /// <exception cref="InvalidOperationException">Target directory is not vaild.</exception>
    /// <exception cref="FileNotFoundException">Game '.jar' and '.json' file not found.</exception>
    /// <exception cref="DirectoryNotFoundException">Target game directory not found.</exception>
    Task<GameInfo> LoadTargetGameAsync(string targetDir, string gameName);

    /// <summary>
    /// Save profile to the specified directory.
    /// </summary>
    /// <param name="targetDir">The directory where profile will be saved.</param>
    /// <param name="profile">A collection of game profile to save.</param>
    /// <returns>true if the profile was saved successfully; otherwise, false.</returns>
    Task<bool> SaveProfilesAsync(string targetDir, ProfileInfo profile);

    /// <summary>
    /// Save profile to the specified directory.
    /// </summary>
    /// <param name="profile">A collection of game profile to save.</param>
    /// <returns>true if the profile were saved successfully; otherwise, false.</returns>
    Task<bool> SaveProfilesDefaultAsync(ProfileInfo profile);

    /// <summary>
    /// Save game to given profile.
    /// </summary>
    /// <param name="profile">The profile that game belongs to.</param>
    /// <param name="game">The game profile to save.</param>
    /// <returns>true if the game was saved successfully; otherwise, false.</returns>
    Task<bool> SaveGameInfoToProfileDefaultAsync(ProfileInfo profile, GameInfo game);

    /// <summary>
    /// Save game to gave profile.
    /// </summary>
    /// <param name="profile">The profile that game belongs to.</param>
    /// <param name="game">The game profile to save.</param>
    /// <param name="targetDir">The target directory.</param>
    /// <returns>true if the game was saved successfully; otherwise, false.</returns>
    Task<bool> SaveGameInfoToProfileAsync(ProfileInfo profile, GameInfo game, string targetDir);

    /// <summary>
    /// Delete game from the profile.
    /// </summary>
    /// <param name="game">The game that should be deleted from the profile.</param>
    /// <param name="profile">The profile that contains target game, used to remove deleted game.</param>
    /// <returns>true if the game was deleted successfully; otherwise, false.</returns>
    bool DeleteGame(GameInfo game, ProfileInfo profile);
}