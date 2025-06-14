using System.Diagnostics;

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
    public VersionInfo JsonContent { get; set; }


    /// <summary>
    /// If <see cref="Type"/> is <see cref="VersionCardType"/>.Moddable, Loader will have value that is used to display in the UI.
    /// </summary>
    public ModLoader Loader { get; set; }


    /// <summary>
    /// Demonstrater is the game started by user. Used to display in the UI.
    /// </summary>
    public bool IsStared { get; set; } = false;

    /// <summary>
    /// Demonstrate is the version has been loader (runed).
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
    public required string JarPath { get; set; }
}

public class GameEntity
{
    public required GameEntityInfo Entity { get; set; }
    public Process GameProcess { get; set; } = new();

    public GameEntity(GameEntityInfo entityInfo)
    {
        Entity = entityInfo;
        GameProcess.StartInfo = new ProcessStartInfo { FileName = Entity.JarPath };
    }
}
