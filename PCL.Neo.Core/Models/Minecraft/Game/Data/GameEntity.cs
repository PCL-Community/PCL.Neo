namespace PCL.Neo.Core.Models.Minecraft.Game.Data;

public record GameEntity
{
    public required LaunchOptions LaunchOptions { get; set; }

    /// <summary>
    /// Game Folder Path.
    /// </summary>
    public required string MinecraftDiractory { get; set; }

    /// <summary>
    /// Game Root Path.
    /// </summary>
    public required string MinecraftRootDirectory { get; set; }

    /// <summary>
    /// The Parsed Game Json Content. Type is <see cref="Data.VersionInfo"/>.
    /// </summary>
    public required VersionInfo VersionInfo { get; set; }

    /// <summary>
    /// The loader type.
    /// </summary>
    public ModLoader Loader { get; set; } = ModLoader.None;

    /// <summary>
    /// Demonstrate if the version has been loaded (runed).
    /// </summary>
    public bool IsRunning { get; set; } = false;

    private bool? _isIndie;

    /// <summary>
    /// Wether the game is an indie game.
    /// </summary>
    public bool IsIndie
    {
        get
        {
            if (_isIndie != null)
            {
                return _isIndie.Value;
            }

            _isIndie = Path.Exists(Path.Combine(MinecraftDiractory, "saves"))
                       && Path.Exists(Path.Combine(MinecraftDiractory, "mods"));

            return _isIndie.Value;
        }

        set { _isIndie = value; }
    }

    /// <summary>
    /// The Game Jar File Path.
    /// </summary>
    public required string GameJarPath { get; set; }
}