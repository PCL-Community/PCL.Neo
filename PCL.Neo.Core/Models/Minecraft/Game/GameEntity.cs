using PCL.Neo.Core.Models.Minecraft.Game.Data;
using PCL.Neo.Core.Service.Game;
using PCL.Neo.Core.Utils.Logger;
using System.Diagnostics;

namespace PCL.Neo.Core.Models.Minecraft.Game;

public class GameEntity(GameInfo inforamtion, LaunchOptions options) : IDisposable
{
    private readonly Lazy<IGameLauncherService>
        _launcherLazy = new(() => new GameLauncherService(inforamtion, options));

    private Process? _gameProcess;
    private bool _disposed;

    // 惰性创建启动器
    public GameInfo Infomation { get; } = inforamtion;
    public LaunchOptions Options { get; } = options;

    private IGameLauncherService Launcher => _launcherLazy.Value;

    public async Task<bool> StartGameAsync()
    {
        try
        {
            var command = await Launcher.BuildLaunchCommandAsync();
            _gameProcess = await Launcher.LaunchAsync(command);
        }
        catch (Exception innerException)
        {
            // 记录异常或重新抛出 TODO: log this exception
            var e = new InvalidOperationException("Failed to start game", innerException);
            NewLogger.Logger.LogError("Failed to start game.", e);
            throw e;
        }

#if DEBUG
        var logPath = Path.Combine(Const.AppData, "logs", "game_.log");
        var logger = new McLogFileLogger(logPath, _gameProcess);
        logger.Start();
#endif

        return true;
    }

    public bool StopGame()
    {
        _gameProcess?.Kill();
        return true;
    }

    public void Dispose()
    {
        if (_disposed) return;

        _gameProcess?.Dispose();
        _disposed = true;
    }
}
