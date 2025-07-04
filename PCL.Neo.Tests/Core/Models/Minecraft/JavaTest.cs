using PCL.Neo.Core.Models.Minecraft.Java;
using System;
using System.Threading.Tasks;

namespace PCL.Neo.Tests.Core.Models.Minecraft;

public class JavaTest
{
    [Test]
    public async Task Test()
    {
        JavaManager javaInstance = new();
        await javaInstance.JavaListInitAsync();
        var runtimes = javaInstance.DefaultJavaRuntimes;
        Console.WriteLine("runtimes:" + runtimes.Java8?.Version + ' ' + runtimes.Java17?.Version + ' ' +
                          runtimes.Java21?.Version);
    }
}
