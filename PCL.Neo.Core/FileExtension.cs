using PCL.Neo.Core.Utils;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;

namespace PCL.Neo.Core;

/// <summary>
/// 一些文件操作和下载请求之类的
/// </summary>
public static class FileExtension
{
    /// <summary>
    /// 校验文件流与SHA-1是否匹配
    /// </summary>
    /// <param name="fileStream">文件流</param>
    /// <param name="sha1">SHA-1</param>
    /// <returns>是否匹配</returns>
    public static async Task<bool> CheckSha1(this FileStream fileStream, string sha1)
    {
        using var sha1Provider = System.Security.Cryptography.SHA1.Create();
        fileStream.Position = 0; // 重置文件流位置

        var computedHash       = await sha1Provider.ComputeHashAsync(fileStream).ConfigureAwait(false);
        var computedHashString = Convert.ToHexStringLower(computedHash);

        return string.Equals(computedHashString, sha1, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// 在 Unix 系统中给予可执行文件运行权限
    /// </summary>
    /// <param name="path">文件路径</param>
    [SupportedOSPlatform(nameof(OSPlatform.OSX))]
    [SupportedOSPlatform(nameof(OSPlatform.Linux))]
    public static void SetFileExecutableUnix(this string path)
    {
        // check is system windows
        if (SystemUtils.Os is SystemUtils.RunningOs.Windows)
        {
            return;
        }

        try
        {
            var currentMode = File.GetUnixFileMode(path);
            var newMode = currentMode
                          | UnixFileMode.UserExecute
                          | UnixFileMode.GroupExecute
                          | UnixFileMode.OtherExecute;
            File.SetUnixFileMode(path, newMode);
        }
        catch (Exception e)
        {
            Console.WriteLine($"无法设置可执行权限：{e.Message}");
            throw;
        }
    }

    /// <summary>
    /// 从某个地方抄来的很像 C 语言风格的解压 LZMA 压缩算法的函数
    /// </summary>
    /// <param name="inStream">被压缩的文件流</param>
    /// <param name="outputFile">输出文件路径</param>
    /// <returns>解压后的文件流</returns>
    public static FileStream? DecompressLzma(this FileStream inStream, string outputFile)
    {
        inStream.Position = 0;

        var outStream        = new FileStream(outputFile, FileMode.Create, FileAccess.ReadWrite);
        var decodeProperties = new byte[5];
        var debugPos         = inStream.Read(decodeProperties, 0, 5);

        Debug.Assert(debugPos == 5);

        SevenZip.Compression.LZMA.Decoder decoder = new();

        decoder.SetDecoderProperties(decodeProperties);
        long outSize = 0;
        for (int i = 0; i < 8; i++)
        {
            int v = inStream.ReadByte();
            if (v < 0)
            {
                Console.WriteLine("read outSize error.");
                return null;
            }

            outSize |= (long)(byte)v << (8 * i);
        }

        long compressedSize = inStream.Length - inStream.Position;

        decoder.Code(inStream, outStream, compressedSize, outSize, null);
        inStream.Close();

        return outStream;
    }
}