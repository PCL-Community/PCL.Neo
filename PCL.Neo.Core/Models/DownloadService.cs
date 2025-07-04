namespace PCL.Neo.Core.Models;

/// <summary>
/// 下载任务管理器
/// 之所以不static是因为后续可能需要接入进度显示和下载统计
/// </summary>
public class DownloadService
{
    ///// <summary>
    ///// 一个static的HttpClient，以便在任何地方调用
    ///// </summary>
    //public static HttpClient HttpClient { get; } = new();

    /// <summary>
    /// 从某个 URL 下载并保存文件
    /// </summary>
    /// <param name="uri">下载地址</param>
    /// <param name="localFilePath">本地文件地址</param>
    /// <param name="sha1">文件sha1值</param>
    /// <param name="passStreamDown">是否向外传递下载的文件流</param>
    /// <param name="maxRetries">最大重试次数</param>
    /// <param name="cancellationToken">用于取消</param>
    /// <returns>向外传递的文件流</returns>
    public async Task<FileStream?> DownloadFileAsync(
        Uri uri,
        string localFilePath,
        string? sha1 = null,
        bool passStreamDown = false,
        int maxRetries = 3,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(uri);

        Console.WriteLine($"Downloading {localFilePath}...");

        var attempt = 0;
        const int baseDelayMs = 500;

        while (true)
        {
            try
            {
                using var response =
                    await Shared.HttpClient.GetAsync(uri, HttpCompletionOption.ResponseHeadersRead,
                        cancellationToken).ConfigureAwait(false);
                response.EnsureSuccessStatusCode();

                var fileStream = new FileStream(localFilePath, FileMode.Create, FileAccess.ReadWrite, FileShare.None);
                try
                {
                    await response.Content.CopyToAsync(fileStream, cancellationToken);
                    response.EnsureSuccessStatusCode();

                    if (!string.IsNullOrEmpty(sha1))
                    {
                        fileStream.Position = 0;
                        var isSha1Match = await fileStream.CheckSha1(sha1).ConfigureAwait(false);
                        if (!isSha1Match)
                        {
                            throw new IOException($"SHA-1 mismatch for file: {localFilePath}");
                        }
                    }

                    if (passStreamDown)
                    {
                        fileStream.Position = 0;
                        return fileStream;
                    }

                    fileStream.Close();
                    return null;
                }
                catch
                {
                    await fileStream.DisposeAsync().ConfigureAwait(false);
                    throw;
                }
            }
            catch (Exception ex) when (attempt < maxRetries)
            {
                attempt++;
                var delay = baseDelayMs * (1 << (attempt - 1)); // 500, 1000, 2000...

                Console.WriteLine($"Attempt {attempt} failed: {ex.Message}. Retrying in {delay} ms...");

                await Task.Delay(delay, cancellationToken);
            }
        }
    }

    /// <summary>
    /// 整合函数：下载并解压，然后删去原压缩文件
    /// </summary>
    public async Task DownloadAndDeCompressFileAsync(
        Uri uri,
        string localFilePath,
        string sha1Raw,
        string sha1Lzma,
        CancellationToken cancellationToken = default)
    {
        var stream = await DownloadFileAsync(uri, localFilePath + ".lzma", sha1Lzma, true,
            cancellationToken: cancellationToken).ConfigureAwait(false);
        if (stream != null)
        {
            var outStream = stream.DecompressLzma(localFilePath);
            if (outStream == null)
            {
                Console.WriteLine("outStream 为空");
                return;
            }

            var match = await outStream.CheckSha1(sha1Raw).ConfigureAwait(false);
            if (!match)
            {
                Console.WriteLine("解压后的文件SHA-1与源提供的不匹配");
                return;
            }

            stream.Close();
            outStream.Close();
        }

        File.Delete(localFilePath + ".lzma");
    }
}
