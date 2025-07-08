using System.Security.Cryptography;

namespace PCL.Neo.Core.Download;

public class FileIntegrityException(string? msg = null) : Exception(msg);

public record FileIntegrity(
    long ExpectedSize = -1,
    HashAlgorithm? HashAlgorithm = null,
    string Hash = "")
{
    private byte[] HashBytes => Convert.FromHexString(Hash);

    public async Task<bool> VerifyAsync(string filepath, CancellationToken token = default)
    {
        await using var fs = new FileStream(filepath, FileMode.Open, FileAccess.Read, FileShare.Read);
        return await VerifyAsync(fs, token);
    }

    public async Task<bool> VerifyAsync(Stream stream, CancellationToken token = default)
    {
        if (!stream.CanRead)
            throw new InvalidOperationException($"{nameof(stream)} is not readable");
        if (ExpectedSize >= 0)
        {
            if (!stream.CanSeek)
            {
                throw new InvalidOperationException(
                    $"{nameof(ExpectedSize)} >= 0 but {nameof(stream)} is not seekable");
            }

            if (stream.Length != ExpectedSize)
                return false;
        }

        if (string.IsNullOrEmpty(Hash)) // skip hash check if no hash specified
            return true;

        var hasher = HashAlgorithm ?? SHA1.Create(); // default to sha1 if not specified

        stream.Seek(0, SeekOrigin.Begin);
        return (await hasher.ComputeHashAsync(stream, token)).SequenceEqual(HashBytes);
    }

    public bool Verify(string filepath)
    {
        return VerifyAsync(filepath).Result;
    }

    public bool Verify(Stream stream)
    {
        return VerifyAsync(stream).Result;
    }

    public bool Verify(byte[] data)
    {
        if (ExpectedSize >= 0 && data.Length != ExpectedSize)
            return false;
        if (string.IsNullOrEmpty(Hash)) // skip hash check if no hash specified
            return true;
        var hasher = HashAlgorithm ?? SHA1.Create();
        return hasher.ComputeHash(data).SequenceEqual(HashBytes);
    }
}
