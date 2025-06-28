using System.Diagnostics;
using System.Text;

namespace PCL.Neo.Core.Utils.Logger;

public sealed class McLogFileLogger : IDisposable
{
    private readonly StreamWriter _writer;
    private readonly Process _process;
    private readonly string _logDir;
    private readonly CancellationTokenSource _cancellationTokenSource = new();

    private bool _disposed = false;

    public McLogFileLogger(string targetDir, Process process)
    {
        if (Directory.Exists(targetDir) == false)
        {
            Directory.CreateDirectory(targetDir);
        }

        var logFilePath = Path.Combine(targetDir, $"game_{DateTimeOffset.Now:yyyy-MM-dd_HHmmss}.log");

        _logDir = targetDir;

        _writer = new StreamWriter(logFilePath, false, Encoding.UTF8) { AutoFlush = true };

        _process = process;
    }

    private async Task AppendContent(string msg)
    {
        await _writer.WriteLineAsync(msg);
        await _writer.FlushAsync();
    }

    public void Start()
    {
        ReadStdOut();
        ReadStdErr();
        _ = GameWatchDog();
    }

    public void Export(string targetFilePath)
    {
        // pre check
        if (!Directory.Exists(_logDir))
        {
            const string msg = "Log directory not found.";
            var ex = new InvalidOperationException(msg);
            NewLogger.Logger.LogError(msg, ex);

            throw ex;
        }

        var logFiles = Directory.GetFiles(_logDir, "game_*.log", SearchOption.TopDirectoryOnly);

        if (logFiles.Length == 0)
        {
            const string msg = "Log files not found.";
            var ex = new FileNotFoundException(msg);
            NewLogger.Logger.LogError(msg, ex);

            throw ex;
        }

        // get latest log file
        var logFile = logFiles.OrderByDescending(File.GetCreationTime).FirstOrDefault();

        ArgumentException.ThrowIfNullOrEmpty(logFile, nameof(logFile));

        // create log file if not exists
        if (File.Exists(targetFilePath) == false)
        {
            using (File.Create(targetFilePath)) { }
        }

        // copy content
        File.Copy(logFile, targetFilePath, true);
    }

    private async Task GameWatchDog()
    {
        await _process.WaitForExitAsync(_cancellationTokenSource.Token);
        Cancel();
    }

    private void ReadStdOut()
    {
        Task.Run(async () =>
        {
            try
            {
                while (await _process.StandardOutput.ReadLineAsync() is { } line &&
                       _cancellationTokenSource.IsCancellationRequested == false)
                {
                    await AppendContent(line);
                }

                Dispose();
            }
            catch (Exception ex)
            {
                NewLogger.Logger.LogError($"Error reading standard output.", ex);
            }
        });
    }

    private void ReadStdErr()
    {
        Task.Run(async () =>
        {
            try
            {
                while (await _process.StandardError.ReadLineAsync() is { } line &&
                       _cancellationTokenSource.IsCancellationRequested == false)
                {
                    await AppendContent(line);
                }

                Dispose();
            }
            catch (Exception ex)
            {
                NewLogger.Logger.LogError("Error reading standard error.", ex);
            }
        });
    }

    public void Cancel()
    {
        _cancellationTokenSource.Cancel();
    }

    /// <inheritdoc />
    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _writer.Flush();
        _writer.Close();
        _writer.Dispose();
        _cancellationTokenSource.Dispose();

        _disposed = true;
    }
}