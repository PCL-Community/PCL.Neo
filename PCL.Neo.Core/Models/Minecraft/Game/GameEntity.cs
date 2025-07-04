using PCL.Neo.Core.Models.Minecraft.Game.Data;
using System.Diagnostics;

namespace PCL.Neo.Core.Models.Minecraft.Game;

public class GameEntity(GameProfile profile) : IDisposable
{
    private readonly Lazy<IGameLauncher> _launcherLazy = new(() => new GameLauncher(profile));
    private Process? _gameProcess;
    private bool _disposed;

    // 惰性创建启动器
    public GameProfile Profile { get; } = profile ?? throw new ArgumentNullException(nameof(profile));

    private IGameLauncher Launcher => _launcherLazy.Value;

    public async Task<bool> StartGameAsync()
    {
        try
        {
            var command = await Launcher.BuildLaunchCommandAsync();
            _gameProcess = await Launcher.LaunchAsync(command);
            return true;
        }
        catch (Exception ex)
        {
            // 记录异常或重新抛出 TODO: log this exception
            throw new InvalidOperationException("Failed to start game", ex);
        }
    }

    public void Dispose()
    {
        if (_disposed) return;

        _gameProcess?.Dispose();
        _disposed = true;
    }
}