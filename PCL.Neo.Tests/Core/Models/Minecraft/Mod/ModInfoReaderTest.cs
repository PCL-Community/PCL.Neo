using PCL.Neo.Core.Models.Minecraft.Mod;
using System;
using System.Threading.Tasks;

namespace PCL.Neo.Tests.Core.Models.Minecraft.Mod;

[TestFixture]
[TestOf(typeof(ModInfoReader))]
public class ModInfoReaderTest
{
    [Test]
    public async Task GetModInfo_ShouldGetModInfos()
    {
        const string modDir =
            @"C:\Users\WhiteCAT\Desktop\Games\PCL2\.minecraft\versions\1.20.4-Fabric 0.15.11-[轻量通用]\mods";
        var mods = await ModInfoReader.GetModInfo(modDir);

        foreach (var modInfo in mods)
        {
            Console.WriteLine(modInfo.Name);
            Console.WriteLine(modInfo.Description);
            Console.WriteLine(modInfo.Version);
            Console.WriteLine(modInfo.Icon);
            Console.WriteLine("--------");
        }
    }
}
