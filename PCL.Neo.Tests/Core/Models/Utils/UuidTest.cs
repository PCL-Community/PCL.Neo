using PCL.Neo.Core.Utils;
using System;

namespace PCL.Neo.Tests.Core.Models.Utils
{
    [TestFixture]
    [TestOf(typeof(Uuid))]
    public class UuidTest
    {
        [Test]
        public void UuidGenerateTest()
        {
            const string name = "WhiteCat";
            var uuid1 = Uuid.GenerateUuid(name, Uuid.UuidGenerateType.Guid);
            var uuid2 = Uuid.GenerateUuid(name, Uuid.UuidGenerateType.Standard);
            var uuid3 = Uuid.GenerateUuid(name, Uuid.UuidGenerateType.MurmurHash3);

            Console.WriteLine(uuid1);
            Console.WriteLine(uuid2);
            Console.WriteLine(uuid3);
        }
    }
}