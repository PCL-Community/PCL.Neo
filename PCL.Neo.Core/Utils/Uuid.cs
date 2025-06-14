using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace PCL.Neo.Core.Utils;

public static partial class Uuid
{
    public enum UuidGenerateType
    {
        Guid,
        Standard,
        MurmurHash3
    }

    private const string OfflinePlayerPrefix = "OfflinePlayer:";
    private const int MinUsernameLength = 3;
    private const int MaxUsernameLength = 16;

    // Thread-safe MD5 instance
    private static readonly ThreadLocal<MD5> Md5Instance = new(MD5.Create);

    /// <summary>
    /// Generate UUID based on input username. If username is empty or invalid,
    /// throw <see cref="ArgumentException"/>.
    /// </summary>
    /// <param name="username">Username to generate UUID.</param>
    /// <param name="type">Type of UUID generation.</param>
    /// <returns>Generated UUID without hyphens.</returns>
    /// <exception cref="ArgumentException">
    /// If <paramref name="username"/> is invalid or empty.
    /// </exception>
    public static string GenerateUuid(string username, UuidGenerateType type)
    {
        if (!IsValidUsername(username))
        {
            throw new ArgumentException("Username is invalid.", nameof(username));
        }

        // Use StringBuilder for better string concatenation performance
        var fullNameBuilder = new StringBuilder(OfflinePlayerPrefix.Length + username.Length);
        fullNameBuilder.Append(OfflinePlayerPrefix).Append(username);
        var fullName = fullNameBuilder.ToString();

        return type switch
        {
            UuidGenerateType.Guid => GenerateGuidUuid(fullName),
            UuidGenerateType.Standard => GenerateStandardUuid(fullName),
            UuidGenerateType.MurmurHash3 => GenerateMurmurHash3Uuid(fullName),
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, "Invalid UUID generation type.")
        };
    }

    private static string GenerateGuidUuid(string input)
    {
        var bytes = Encoding.UTF8.GetBytes(input);
        var hash = Md5Instance.Value!.ComputeHash(bytes);
        return new Guid(hash).ToString("N"); // "N" format removes hyphens directly
    }

    private static string GenerateStandardUuid(string input)
    {
        var bytes = Encoding.UTF8.GetBytes(input);
        var hash = Md5Instance.Value!.ComputeHash(bytes);

        // Set version (3) and variant bits
        hash[6] = (byte)((hash[6] & 0x0F) | 0x30); // Version 3
        hash[8] = (byte)((hash[8] & 0x3F) | 0x80); // Variant 1 (RFC 4122)

        return new Guid(hash).ToString("N");
    }

    private static string GenerateMurmurHash3Uuid(string input)
    {
        var hash = MurmurHash3.Hash(input);
        return new Guid(hash).ToString("N");
    }

    [GeneratedRegex("^[a-zA-Z0-9_]+$", RegexOptions.Compiled)]
    private static partial Regex ValidUsernameRegex();

    private static bool IsValidUsername(string? username)
    {
        return !string.IsNullOrEmpty(username) &&
               username.Length is >= MinUsernameLength and <= MaxUsernameLength &&
               ValidUsernameRegex().IsMatch(username);
    }

    // Optimized MurmurHash3 implementation
    private static class MurmurHash3
    {
        private const uint Seed = 144;
        private const uint C1 = 0xcc9e2d51;
        private const uint C2 = 0x1b873593;
        private const uint FinalMix1 = 0x85ebca6b;
        private const uint FinalMix2 = 0xc2b2ae35;

        public static byte[] Hash(string str)
        {
            var bytes = Encoding.UTF8.GetBytes(str);
            const uint seed = 144;
            const uint c1 = 0xcc9e2d51;
            const uint c2 = 0x1b873593;
            var h1 = seed;
            uint k1;
            var len = bytes.Length;
            var i = 0;
            for (; i + 4 <= len; i += 4)
            {
                k1 = (uint)((bytes[i] & 0xFF) |
                            ((bytes[i + 1] & 0xFF) << 8) |
                            ((bytes[i + 2] & 0xFF) << 16) |
                            ((bytes[i + 3] & 0xFF) << 24));
                k1 *= c1;
                k1 = RotateLeft(k1, 15);
                k1 *= C2;

                h1 ^= k1;
                h1 = RotateLeft(h1, 13);
                h1 = h1 * 5 + 0xe6546b64;
            }

            k1 = 0;
            switch (len & 3)
            {
                case 3:
                    k1 ^= (uint)(bytes[i + 2] & 0xFF) << 16;
                    goto case 2;
                case 2:
                    k1 ^= (uint)(bytes[i + 1] & 0xFF) << 8;
                    goto case 1;
                case 1:
                    k1Tail ^= bytes[tailIndex];
                    k1Tail *= C1;
                    k1Tail = RotateLeft(k1Tail, 15);
                    k1Tail *= C2;
                    h1 ^= k1Tail;
                    break;
            }

            h1 ^= (uint)len;
            h1 = Fmix(h1);
            var result = new byte[16];
            BitConverter.GetBytes(h1).CopyTo(result, 0);
            BitConverter.GetBytes(h1 ^ seed).CopyTo(result, 4);
            BitConverter.GetBytes(seed ^ (h1 >> 16)).CopyTo(result, 8);
            BitConverter.GetBytes(seed ^ (h1 << 8)).CopyTo(result, 12);
            return result;
        }

        private static uint RotateLeft(uint x, int r)
        {
            return (x << r) | (x >> (32 - r));
        }

        private static uint Fmix(uint h)
        {
            h ^= h >> 16;
            h *= FinalMix1;
            h ^= h >> 13;
            h *= FinalMix2;
            h ^= h >> 16;
            return h;
        }
    }
}
