using PCL.Neo.Core.Utils;

namespace PCL.Neo.Core.Download;

public class DownloadReceipt
{
    public event Action<DownloadReceipt>? OnBegin;
    public event Action<DownloadReceipt>? OnSuccess;
    public event Action<DownloadReceipt, Exception>? OnError;
    public event Action<DownloadReceipt, long>? OnDeltaSizeChanged;

    public string SourceUrl { get; init; } = string.Empty;
    public string DestinationPath { get; init; } = string.Empty;
    public FileIntegrity? Integrity { get; init; }
    public int MaxRetries { get; init; } = 3;


    public int Attempts { get; private set; }
    public long Size { get; private set; }
    public long TotalSize { get; private set; }

    public bool IsCompleted { get; private set; }
    public Exception? Error { get; private set; }
    public IProgress<double>? DownloadProgress { get; set; }

    public Task DownloadInNewTask(HttpClient? client = null, CancellationToken token = default)
    {
        try
        {
            return Task.Run(async () => await DownloadAsync(client, false, token), token);
        }
        catch (OperationCanceledException) {}

        return Task.FromCanceled(token);
    }

    public async Task DownloadAsync(HttpClient? client = null, bool throwException = true,
        CancellationToken token = default)
    {
        if (token.IsCancellationRequested)
            return;

        IsCompleted = false;
        Error = null;
        client ??= Shared.HttpClient;
        try
        {
            OnBegin?.Invoke(this);
            while (!token.IsCancellationRequested)
            {
                try
                {
                    var res = await client.GetAsync(SourceUrl, HttpCompletionOption.ResponseHeadersRead, token);
                    res.EnsureSuccessStatusCode();

                    Size = 0;
                    TotalSize = res.Content.Headers.ContentLength ?? 0;

                    // preparing parent directory
                    var parentDir = Path.GetDirectoryName(DestinationPath);
                    if (!string.IsNullOrEmpty(parentDir))
                        Directory.CreateDirectory(parentDir);

                    // copying file content
                    await using var ns = await res.Content.ReadAsStreamAsync(token);
                    await using var fs = new FileStream(
                        DestinationPath + ".tmp", // to ensure only properly downloaded file exists
                        FileMode.Create,
                        FileAccess.ReadWrite, FileShare.None);
                    await ns.CopyToAsync(
                        fs,
                        81920,
                        new SynchronousProgress<long>(x =>
                        {
                            var origSize = Size;
                            Size = x;
                            OnDeltaSizeChanged?.Invoke(this, x - origSize);
                            DownloadProgress?.Report((double)Size / TotalSize);
                        }),
                        token);

                    if (Integrity is not null && !await Integrity.VerifyAsync(fs, token))
                        throw new FileIntegrityException("Failed to verify integrity");

                    IsCompleted = true;
                    break; // downloaded successfully, break download loop
                }
                catch (Exception ex) when (ex is not OperationCanceledException && Attempts < MaxRetries)
                {
                    // reset common properties
                    Size = 0;
                    TotalSize = 0;

                    // 異議あり！ ...
                    const int baseDelayMs = 500;
                    var delay = baseDelayMs * (1 << Attempts++);
                    // TODO: remove this testing log
                    Console.WriteLine(
                        $"[{SourceUrl}] Attempt {Attempts} failed: {ex.Message}. Retry after {delay} ms...");
                    await Task.Delay(delay, token);
                }
            }

            if (IsCompleted)
            {
                File.Move(DestinationPath + ".tmp", DestinationPath);
                OnSuccess?.Invoke(this);
            }
        }
        catch (OperationCanceledException) {}
        catch (Exception ex)
        {
            // 寄
            Error = ex;
            OnError?.Invoke(this, ex);
            if (throwException)
                throw;
        }
        finally
        {
            // clean up
            if (File.Exists(DestinationPath + ".tmp"))
                File.Delete(DestinationPath + ".tmp");
        }
    }

    public static async Task FastDownloadAsync(string sourceUrl, string destPath, string? sha1 = null,
        CancellationToken token = default)
    {
        await new DownloadReceipt
        {
            SourceUrl = sourceUrl,
            DestinationPath = destPath,
            Integrity = sha1 is null ? null : new FileIntegrity { Hash = sha1 }
        }.DownloadAsync(token: token);
    }

    public static async Task<FileStream> FastDownloadAsStreamAsync(string sourceUrl, string destPath,
        string? sha1 = null, CancellationToken token = default)
    {
        await FastDownloadAsync(sourceUrl, destPath, sha1, token);
        return new FileStream(destPath, FileMode.Open, FileAccess.Read, FileShare.Read);
    }
}
