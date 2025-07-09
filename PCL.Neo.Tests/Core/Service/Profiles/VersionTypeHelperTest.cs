using PCL.Neo.Core.Models.Minecraft.Game.Data;
using PCL.Neo.Core.Service.Profiles;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace PCL.Neo.Tests.Core.Service.Profiles
{
    [TestFixture]
    [TestOf(typeof(VersionTypeHelper))]
    public class VersionTypeHelperTest
    {
        [Test]
        public async Task GetVerisonType_ShouldFabric()
        {
            const string gamePath =
                @"C:\Users\WhiteCAT\Desktop\Games\PCL2\.minecraft\versions\1.20.4-Fabric 0.15.11-[轻量通用]\";
            const string gameName = "1.20.4-Fabric 0.15.11-[轻量通用]";
            const GameType targetType = GameType.Fabric;
            var gameType = await VersionTypeHelper.GetGameType(gamePath, gameName);


            Assert.That(gameType, Is.EqualTo(targetType));
            Console.WriteLine(gameType);
        }

        [Test]
        public async Task GetVersionType_ShouldFool()
        {
            const string gamePath =
                @"C:\Users\WhiteCAT\Desktop\Games\PCL2\.minecraft\versions\25w14craftmine";
            const string gameName = "25w14craftmine";
            const GameType targetType = GameType.Fool | GameType.Snapshot | GameType.Vanilla;
            var gameType = await VersionTypeHelper.GetGameType(gamePath, gameName);
            Assert.That(gameType, Is.EqualTo(targetType));
            Console.WriteLine(gameType);
        }

        [Test]
        public async Task GetVersionType_ShouldSnapshot()
        {
            const string gamePath =
                @"C:\Users\WhiteCAT\Desktop\Games\PCL2\.minecraft\versions\1.0.0-rc2-2";
            const string gameName = "1.0.0-rc2-2";
            const GameType targetType = GameType.Snapshot | GameType.Vanilla;

            var gameType = await VersionTypeHelper.GetGameType(gamePath, gameName);
            Assert.That(gameType, Is.EqualTo(targetType));
            Console.WriteLine(gameType);
        }
    }
}