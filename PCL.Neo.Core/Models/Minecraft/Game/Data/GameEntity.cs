namespace PCL.Neo.Core.Models.Minecraft.Game.Data;

public record GameEntity
{
    /// <summary>
    /// Game Folder Path.
    /// </summary>
    public required string GamePath { get; set; }

    /// <summary>
    /// Game Root Path.
    /// </summary>
    public required string RootPath { get; set; }


    /// <summary>
    /// The origin Game Json Content. Type is <see langword="string"/>.
    /// </summary>
    public required string JsonOrigContent { get; set; }

    /// <summary>
    /// The Parsed Game Json Content. Type is <see cref="VersionInfo"/>.
    /// </summary>
    public VersionInfo JsonContent { get; set; }


    /// <summary>
    /// If <see cref="Type"/> is <see cref="VersionCardType"/>.Moddable, Loader will have value that is used to display in the UI.
    /// </summary>
    public ModLoader Loader { get; set; }


    /// <summary>
    /// Demonstrate if the version has been loaded (runed).
    /// </summary>
    public bool IsLoaded { get; set; } = false;

    private bool? _isIndie;

    public bool IsIndie
    {
        get
        {
            if (_isIndie != null)
            {
                return _isIndie.Value;
            }

            _isIndie = Path.Exists(Path.Combine(GamePath, "saves"))
                       && Path.Exists(Path.Combine(GamePath, "mods"));

            return _isIndie.Value;
        }
    }

    /// <summary>
    /// THe Game Jar File Path.
    /// </summary>
    public required string JarPath { get; set; }
}