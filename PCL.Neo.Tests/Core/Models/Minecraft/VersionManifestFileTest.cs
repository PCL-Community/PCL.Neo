using PCL.Neo.Core.Models.Minecraft;
using System.IO;

namespace PCL.Neo.Tests.Core.Models.Minecraft;

public class VersionManifestFileTest
{
    [Test]
    public void Parse()
    {
        var vmf = VersionManifestFile.Parse(File.ReadAllText("./version_manifest.json"));
        //Assert.IsNotNull(vmf);
        Assert.That(vmf.Latest, Is.Not.Empty);
        Assert.That(vmf.Latest.ContainsKey(ReleaseTypeEnum.Release));
        Assert.That(vmf.Latest.ContainsKey(ReleaseTypeEnum.Snapshot));
        Assert.That(vmf.Latest[ReleaseTypeEnum.Release], Is.Not.Empty);
        Assert.That(vmf.Latest[ReleaseTypeEnum.Snapshot], Is.Not.Empty);

        Assert.That(vmf.Versions, Is.Not.Empty);
        foreach (var v in vmf.Versions)
        {
            Assert.That(v.Id, Is.Not.Empty);
            Assert.That(v.Type, Is.Not.EqualTo(ReleaseTypeEnum.Unknown));
            Assert.That(v.Url, Is.Not.Empty);
            Assert.That(v.Time, Is.Not.Empty);
            Assert.That(v.ReleaseTime, Is.Not.Empty);
        }
    }
}
