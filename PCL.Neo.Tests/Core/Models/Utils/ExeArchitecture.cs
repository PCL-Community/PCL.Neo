using PCL.Neo.Core.Utils;

namespace PCL.Neo.Tests.Core.Models.Utils;

public class ExeArchitecture
{
    [Test]
    public void TestArchitecture()
    {
        var path = "/Users/amagicpear/Downloads/file_zip_win32_x64.exe";
        ExeArchitectureUtils.GetExecutableArchitecture(path);
    }
}
