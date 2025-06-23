using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace PCL.Neo.Core.Utils;

public static partial class Uuid // TODO: implement different way of genereate uuid
{
    public enum UuidGenerateType
    {
        Guid,
        Standard,
        MurmurHash3
    }

    /// <summary>
    /// Generate UUID base on input username. If username is empty or invalid,
    /// throw <see cref="ArgumentException"/>.
    /// </summary>
    /// <param name="username">Username to generate UUID.</param>
    /// <param name="type">Type of UUID generation.</param>
    /// <returns>Generated UUID.</returns>
    /// <exception cref="ArgumentException">
    /// If <paramref name="username"/> is invalid or empty.
    /// </exception>
    public static string GenerateUuid(string username, UuidGenerateType type)
    {
        if (string.IsNullOrEmpty(username) ||
            !IsValidUsername(username))
        {
            throw new ArgumentException("Username is invalid.");
        }

        var fullName = $"OfflinePlayer:{username}";

        var uuid = type switch
        {
            UuidGenerateType.Guid => new Guid(MD5.HashData(Encoding.UTF8.GetBytes(fullName))).ToString(),
            UuidGenerateType.Standard => StadardVer(fullName),
            UuidGenerateType.MurmurHash3 => new Guid(MurmurHash3.Hash(fullName)).ToString(),
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
        };

        uuid = uuid.Replace("-", string.Empty);

        return uuid;
    }

    private static string StadardVer(string name)
    {
        var hash = MD5.HashData(Encoding.UTF8.GetBytes(name));

        hash[6] = (byte)((hash[6] & 0x0F) | 0x30); // Version 3
        hash[8] = (byte)((hash[8] & 0x3F) | 0x80); // Variant 1 (RFC 4122)

        return new Uuids.Uuid(hash).ToString();
    }

    [GeneratedRegex("^[a-zA-Z0-9_]+$")]
    private static partial Regex ValidUsernameRegex();

    public static bool IsValidUsername(string username)
    {
        return !string.IsNullOrEmpty(username) &&
               username.Length is >= 3 and <= 16 &&
               ValidUsernameRegex().IsMatch(username);
    }

    // MurmurHash3算法实现

    private static class MurmurHash3
    {
        public static byte[] Hash(string str)
        {
            var bytes = Encoding.UTF8.GetBytes(str);
            const uint seed = 144;
            const uint c1 = 0xcc9e2d51;
            const uint c2 = 0x1b873593;
            uint h1 = seed;
            uint k1;
            int len = bytes.Length;
            int i = 0;
            for (; i + 4 <= len; i += 4)
            {
                k1 = (uint)((bytes[i] & 0xFF) |
                    ((bytes[i + 1] & 0xFF) << 8) |
                    ((bytes[i + 2] & 0xFF) << 16) |
                    ((bytes[i + 3] & 0xFF) << 24));
                k1 *= c1;
                k1 = RotateLeft(k1, 15);
                k1 *= c2;
                h1 ^= k1;
                h1 = RotateLeft(h1, 13);
                h1 = h1 * 5 + 0xe6546b64;
            }
            k1 = 0;
            switch (len & 3)
            {
                case 3: k1 ^= (uint)(bytes[i + 2] & 0xFF) << 16; goto case 2;
                case 2: k1 ^= (uint)(bytes[i + 1] & 0xFF) << 8; goto case 1;
                case 1:
                    k1 ^= (uint)(bytes[i] & 0xFF);
                    k1 *= c1;
                    k1 = RotateLeft(k1, 15);
                    k1 *= c2;
                    h1 ^= k1;
                    break;
            }
            h1 ^= (uint)len;
            h1 = Fmix(h1);
            byte[] result = new byte[16];
            BitConverter.GetBytes(h1).CopyTo(result, 0);
            BitConverter.GetBytes(h1 ^ seed).CopyTo(result, 4);
            BitConverter.GetBytes(seed ^ (h1 >> 16)).CopyTo(result, 8);
            BitConverter.GetBytes(seed ^ (h1 << 8)).CopyTo(result, 12);
            return result;
        }
        private static uint RotateLeft(uint x, int r) => (x << r) | (x >> (32 - r));
        private static uint Fmix(uint h)
        {
            h ^= h >> 16;
            h *= 0x85ebca6b;
            h ^= h >> 13;
            h *= 0xc2b2ae35;
            h ^= h >> 16;
            return h;
        }
    }
}