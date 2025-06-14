using PCL.Neo.Core.Models.Minecraft.Game.Data;
using System.Diagnostics;

namespace PCL.Neo.Core.Models.Minecraft.Game;

public class GameEntity : IDisposable
{
    public required GameProfile Profile { get; init; }
    private IGameLauncher Launcher { get; } = new GameLauncher();
    private Process GameProcess { get; set; }

    public GameEntity()
    {
    }

    public async Task<bool> StartGame()
    {
        GameProcess = await Launcher.LaunchAsync(Profile);
        return !GameProcess.HasExited;
    }

    /// <inheritdoc />
    public void Dispose()
    {
        if (!GameProcess.HasExited)
        {
            GameProcess.Kill();
        }

        GameProcess.Dispose();
    }
}