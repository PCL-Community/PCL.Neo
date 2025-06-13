namespace PCL.Neo.ViewModels.Game;

public record GameEntry
{
    /// <summary>
    /// The Game Version information.
    /// </summary>
    public GameVersionNum? GameVersion { get; init; }

    /// <summary>
    /// String typed game version. eg: 25w19a、1.21.5-rc2、25w14craftmine.
    /// </summary>
    public required string GameVersionString { get; init; }

    /// <summary>
    /// Game Name that is used to display in the UI.
    /// </summary>
    public required string Name { get; set; }

    /// <summary>
    /// Game Description that is used to display in the UI.
    /// </summary>
    public string Description { get; set; }

    /// <summary>
    /// Game Icon that is used to display in the UI.
    /// </summary>
    public Icons Icon { get; set; }

    /// <summary>
    /// Demonstrate the Game Version Type.
    /// Content is <see cref="VersionCardType"/>.
    /// </summary>
    public VersionCardType Type { get; set; }

    /// <summary>
    /// Demonstrater is the game started by user. Used to display in the UI.
    /// </summary>
    public bool IsStared { get; set; } = false;
}