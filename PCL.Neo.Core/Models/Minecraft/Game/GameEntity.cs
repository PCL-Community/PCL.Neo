using PCL.Neo.Core.Models.Minecraft.Game.Data;
using PCL.Neo.Core.Service.Game;
using PCL.Neo.Core.Utils.Logger;
using System.Diagnostics;

namespace PCL.Neo.Core.Models.Minecraft.Game;

public class GameEntity(GameProfile profile) : IDisposable
{
    private readonly Lazy<IGameLauncherService> _launcherLazy = new(() => new GameLauncherService(profile));
    private Process? _gameProcess;
    private bool _disposed;

    // 惰性创建启动器
    public GameProfile Profile { get; } = profile ?? throw new ArgumentNullException(nameof(profile));

    private IGameLauncherService Launcher => _launcherLazy.Value;

    public async Task<bool> StartGameAsync()
    {
        try
        {
            var command = await Launcher.BuildLaunchCommandAsync();
            _gameProcess = await Launcher.LaunchAsync(command);
        }
        catch (Exception ex)
        {
            // 记录异常或重新抛出 TODO: log this exception
            throw new InvalidOperationException("Failed to start game", ex);
        }

#if DEBUG
        var logger = new McLogFileLogger("launch_args.txt", _gameProcess);
#endif

        return true;
    }

    public void Dispose()
    {
        if (_disposed) return;

        _gameProcess?.Dispose();
        _disposed = true;
    }
}
