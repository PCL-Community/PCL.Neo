using NUnit.Framework.Legacy;
using System;
using System.Security.Cryptography;
using System.Text;

namespace PCL.Neo.Tests;

[TestFixture]
public class BitConvertorTest
{
    [Test]
    public void ConvertorTest()
    {
        using var md5 = MD5.Create();
        var inputBytes = Encoding.UTF8.GetBytes("OfflinePlayer:whiteact346");
        var hashBytes = md5.ComputeHash(inputBytes);

        // 设置UUID版本 (版本3 = MD5)
        hashBytes[6] = (byte)((hashBytes[6] & 0x0F) | 0x30);
        // 设置UUID变体
        hashBytes[8] = (byte)((hashBytes[8] & 0x3F) | 0x80);

        // 转换为UUID字符串格式
        var fir = BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
        var sec = Convert.ToHexStringLower(hashBytes);

        Console.Write(fir);

        ClassicAssert.AreEqual(fir, sec);
    }
}
