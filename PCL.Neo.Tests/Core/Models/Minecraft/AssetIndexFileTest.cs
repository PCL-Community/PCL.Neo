using PCL.Neo.Core.Models.Minecraft;
using System.IO;

namespace PCL.Neo.Tests.Core.Models.Minecraft;

public class AssetIndexFileTest
{
    [Test]
    public void Parse()
    {
        var aif = AssetIndexFile.Parse(File.ReadAllText("./pre-1.6.json"));
        //Assert.NotNull(aif);
        Assert.That(aif.Objects, Is.Not.Empty);
        foreach (var (path, info) in aif.Objects)
        {
            Assert.That(path, Is.Not.Empty);
            Assert.That(info, Is.Not.Null);
            Assert.That(info.Size, Is.Not.Zero);
            Assert.That(info.Hash, Is.Not.Empty);
        }
    }
}
