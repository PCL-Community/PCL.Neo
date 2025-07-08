using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace PCL.Neo.Core.Service.Accounts.Storage;

public record OfflineAccount : BaseAccount
{
    // 离线账户特有属性
    public required string AccessToken { get; init; }

    public OfflineAccount() : base(UserTypeConstants.Offline)
    {
    }

    // 创建新的离线账户
    public static OfflineAccount Create(string username)
    {
        return new OfflineAccount
        {
            UserName = username,
            Uuid = GenerateOfflineUUID(username),
            AccessToken = Guid.NewGuid().ToString(),
            Skins = new List<Skin>(),
            Capes = new List<Cape>()
        };
    }

    // 生成离线UUID
    public static string GenerateOfflineUUID(string username)
    {
        // 离线模式使用用户名的MD5作为UUID
        using var md5 = MD5.Create();
        var inputBytes = Encoding.UTF8.GetBytes($"OfflinePlayer:{username}");
        var hashBytes = md5.ComputeHash(inputBytes);

        // 设置UUID版本 (版本3 = MD5)
        hashBytes[6] = (byte)((hashBytes[6] & 0x0F) | 0x30);
        // 设置UUID变体
        hashBytes[8] = (byte)((hashBytes[8] & 0x3F) | 0x80);

        // 转换为UUID字符串格式
        return BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
    }

    // 验证用户名是否合法
    public static bool IsValidUsername(string username)
    {
        // Minecraft用户名规则：3-16个字符，只允许字母、数字和下划线
        return !string.IsNullOrEmpty(username) &&
               username.Length >= 3 &&
               username.Length <= 16 &&
               Regex.IsMatch(username, "^[a-zA-Z0-9_]+$");
    }

    // 离线账户永不过期
    public override bool IsExpired()
    {
        return false;
    }
}
