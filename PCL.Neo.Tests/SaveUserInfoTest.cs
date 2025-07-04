using PCL.Neo.Core.Service.Accounts.Storage;
using PCL.Neo.Models.User;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

namespace PCL.Neo.Tests;

[TestFixture]
public class SaveUserInfoTest
{
    [Test]
    public async Task SaveUsers()
    {
        const string savePath = "testPath.json";

        // 创建测试账户
        var msaAccount = new MsaAccount
        {
            Uuid = Guid.NewGuid().ToString(),
            UserName = "MicrosoftPlayer123",
            OAuthToken = new OAuthTokenData("msa_access_token", "msa_refresh_token",
                DateTimeOffset.UtcNow.AddHours(1)),
            McAccessToken = "minecraft_access_token_for_msa",
            Skins =
            [
                new Skin("skin1_id", new Uri("http://textures.minecraft.net/texture/skin1"), "classic",
                    "key1", AccountState.Active)
            ],
            Capes = [],
            UserProperties = "{ \"property\": \"value\" }"
        };

        var yggdrasilAccount = new YggdrasilAccount
        {
            Uuid = Guid.NewGuid().ToString(),
            UserName = "YggdrasilUser789",
            McAccessToken = "yggdrasil_access_token",
            ClientToken = "yggdrasil_client_token",
            ServerUrl = "https://authserver.example.com",
            Skins = [],
            Capes =
            [
                new Cape("cape_ygg_id",
                    AccountState.Active,
                    new Uri("http://yggdrasil.example.com/cape1"),
                    "MyYggCape")
            ]
        };

        // 正确创建UserInfo对象
        List<UserInfo> users =
        [
            new(msaAccount),
            new(yggdrasilAccount)
        ];

        var json = JsonSerializer.Serialize(users);
        await File.WriteAllTextAsync(savePath, json);

        var content = await File.ReadAllTextAsync(savePath);
        var desr = JsonSerializer.Deserialize<List<UserInfo>>(content);
        foreach (var userInfo in desr)
        {
            Console.WriteLine(
                $"\nLoaded Account: {userInfo.Account.UserName} (UUID: {userInfo.Account.Uuid}, Type: {userInfo.Account.StorageType})");
            switch (userInfo.Account)
            {
                case MsaAccount msa:
                    Console.WriteLine($"  MSA Token Expires: {msa.OAuthToken.ExpiresAt}");
                    break;
                case YggdrasilAccount ygg:
                    Console.WriteLine($"  Yggdrasil MC Access Token: {ygg.McAccessToken}");
                    break;
                case OfflineAccount off:
                    Console.WriteLine("  This is an offline account.");
                    break;
            }
        }
    }
}
