using PCL.Neo.Core.Download;
using PCL.Neo.Core.Models.Minecraft.Java;
using PCL.Neo.Core.Models.Minecraft.Mod;
using System;
using System.IO;
using System.Threading.Tasks;

namespace PCL.Neo.Tests.Core.Models.FileHelper;

public class FileTest
{
    [Test]
    public async Task Download()
    {
        await DownloadReceipt.FastDownloadAsync(
            "https://piston-meta.mojang.com/v1/products/java-runtime/2ec0cc96c44e5a76b9c8b7c39df7210883d12871/all.json",
            Path.Combine(Path.GetTempPath(), "all.json"));
        Console.WriteLine(Path.GetTempPath());
    }

    [Test]
    public async Task Fetch()
    {
        await JavaManager.FetchJavaOnline("mac-os-arm64", "/Users/amagicpear/Downloads/PCL2Test",
            JavaManager.MojangJavaVersion.Δ,
            new Progress<(int, int)>(value =>
            {
                Console.WriteLine($"下载进度：已下载{value.Item1}/总文件数{value.Item2}");
            }));
        // await Helpers.FileExtension.FetchJavaOnline("windows-x64",@"C:\Users\AMagicPear\Downloads\PCL2Test");
    }

    [Test]
    public void ModPackTest()
    {
        ModPack.InstallPackModrinth("/Users/amagicpear/Downloads/1.20.4-Fabric 0.15.3/modpack.mrpack",
            "/Users/amagicpear/Downloads/TestModPack");
    }

    [Test]
    public void MojangVersionTest()
    {
        Console.WriteLine(JavaManager.MojangJavaVersion.Δ.Value);
    }

    [Test]
    public async Task SelectFileTest()
    {
        // await Helpers.FileExtension.SelectFile("Test");
    }
}
