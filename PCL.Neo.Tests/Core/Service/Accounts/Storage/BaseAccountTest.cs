using PCL.Neo.Core.Service.Accounts.Storage;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace PCL.Neo.Tests.Core.Service.Accounts.Storage;

[TestFixture]
[TestOf(typeof(BaseAccount))]
public class BaseAccountTest
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true // For pretty printing
        // Converters can be added here if needed, but polymorphic attributes should handle this.
    };

    public static void SaveAccounts(List<BaseAccount> accounts, string filePath)
    {
        var json = JsonSerializer.Serialize(accounts, JsonOptions);
        File.WriteAllText(filePath, json);
        Console.WriteLine($"Accounts saved to {filePath}");
    }

    public static List<BaseAccount> LoadAccounts(string filePath)
    {
        if (!File.Exists(filePath))
        {
            return [];
        }

        var json = File.ReadAllText(filePath);
        var accounts = JsonSerializer.Deserialize<List<BaseAccount>>(json, JsonOptions);
        Console.WriteLine($"Accounts loaded from {filePath}");
        return accounts ?? [];
    }

    [Test]
    public void MainTest()
    {
        var accounts = new List<BaseAccount>();

        // Create an MSA Account
        var msaAccount = new MsaAccount
        {
            Uuid = Guid.NewGuid().ToString(),
            UserName = "MicrosoftPlayer123",
            OAuthToken =
                new OAuthTokenData("msa_access_token", "msa_refresh_token", DateTimeOffset.UtcNow.AddHours(1)),
            McAccessToken = "minecraft_access_token_for_msa",
            Skins =
            [
                new Skin("skin1_id", new Uri("http://textures.minecraft.net/texture/skin1"), "classic",
                    "key1", AccountState.Active)
            ],
            Capes = [],
            UserProperties = "{ \"property\": \"value\" }"
        };
        accounts.Add(msaAccount);

        // Create a Yggdrasil Account
        var yggdrasilAccount = new YggdrasilAccount
        {
            Uuid = Guid.NewGuid()
                .ToString(),
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
        accounts.Add(yggdrasilAccount);

        // Create an Offline Account
        var offlineAccount = new OfflineAccount
        {
            Uuid = Guid.NewGuid().ToString(), // Often generated locally for offline
            UserName = "OfflinePlayer",
            AccessToken = Guid.NewGuid().ToString(),
            Skins = [],
            Capes = []
        };
        accounts.Add(offlineAccount);

        // Save and Load
        const string filePath = "accounts.json";
        SaveAccounts(accounts, filePath);

        var loadedAccounts = LoadAccounts(filePath);

        foreach (var acc in loadedAccounts)
        {
            Console.WriteLine($"\nLoaded Account: {acc.UserName} (UUID: {acc.Uuid}, Type: {acc.StorageType})");
            switch (acc)
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

        // Example of serializing a single account
        var singleMsaJson = JsonSerializer.Serialize<BaseAccount>(msaAccount, JsonOptions);
        Console.WriteLine("\nSingle MSA account JSON:\n" + singleMsaJson);
        BaseAccount deserializedSingleMsa = JsonSerializer.Deserialize<MsaAccount>(singleMsaJson, JsonOptions);
        if (deserializedSingleMsa is MsaAccount msaAgain)
        {
            Console.WriteLine($"Successfully deserialized single MSA account: {msaAgain.UserName}");
        }
    }
}
