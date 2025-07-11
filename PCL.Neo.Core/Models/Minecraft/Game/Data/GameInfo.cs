namespace PCL.Neo.Core.Models.Minecraft.Game.Data;

public record GameInfo
{
    /// <summary>
    /// Game ID.
    /// </summary>
    public required string Name { get; set; }

    /// <summary>
    /// .minecraft folder path.
    /// </summary>
    public required string GameDirectory { get; set; }

    /// <summary>
    /// Game version path.
    /// </summary>
    public required string RootDirectory { get; set; }

    /// <summary>
    /// The loader type.
    /// </summary>
    public GameType Loader { get; set; } = GameType.Unknown;

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

            _isIndie = Path.Exists(Path.Combine(GameDirectory, "saves"))
                       && Path.Exists(Path.Combine(GameDirectory, "mods"));

            return _isIndie.Value;
        }

        set { _isIndie = value; }
    }
}