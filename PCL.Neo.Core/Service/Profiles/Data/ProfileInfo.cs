using PCL.Neo.Core.Models.Minecraft.Game.Data;

namespace PCL.Neo.Core.Service.Profiles.Data;

public class ProfileInfo
{
    /// <summary>
    /// The profile name.
    /// </summary>
    public required string ProfileName { get; set; }

    /// <summary>
    /// The directory where the profiles are located.
    /// </summary>
    public required string TargetDir { get; set; }

    /// <summary>
    /// The game that this profile contains.
    /// </summary>
    public required List<GameInfo> Games { get; set; }
}
