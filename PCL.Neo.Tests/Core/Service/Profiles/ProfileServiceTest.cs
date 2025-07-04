using System;
using System.IO;
using System.Threading.Tasks;
using NUnit.Framework.Legacy;
using PCL.Neo.Core.Service.Profiles;
using PCL.Neo.Core.Service.Profiles.Data;
using PCL.Neo.Core.Models.Minecraft.Game.Data;

namespace PCL.Neo.Tests.Core.Service.Profiles
{
    [TestFixture]
    [TestOf(typeof(ProfileService))]
    public class ProfileServiceTest
    {
        private const string TempDir = @"C:\Users\WhiteCAT\Desktop\Games\PCL2\.minecraft";
        private IProfileService _service = null!;
        private const string SaveTempDir = @"C:\Users\WhiteCAT\AppData\Roaming\PCL.Neo\used_for_profiles_test";

        [SetUp]
        public void SetUp()
        {
            _service = new ProfileService();
        }

        [Test]
        public async Task LoadProfileAsync_ShouldLoadAllGames()
        {
            var profile = await _service.LoadProfileAsync(TempDir);
            Assert.That(profile, Is.Not.Null);
            Assert.That(profile.Games.Count, Is.EqualTo(29));

            await _service.SaveProfilesDefaultAsync(profile);
        }

        [Test]
        public void LoadProfileAsync_ShouldThrowOnInvalidDir()
        {
            var invalidDir = Path.Combine(TempDir, "not_exist");
            var ex = Assert.ThrowsAsync<InvalidOperationException>(async () =>
                await _service.LoadProfileAsync(invalidDir));
            StringAssert.Contains("missing required subdirectories", ex!.Message);
        }

        [Test]
        public async Task LoadTargetGameAsync_ShouldLoadGame()
        {
            var game = await _service.LoadTargetGameAsync(TempDir, "1.20.6-Fabric 0.15.11");
            Assert.That(game, Is.Not.Null);
            Assert.That(game.Name, Is.EqualTo("1.20.6-Fabric 0.15.11"));
            Assert.That(game.IsIndie, Is.True);
        }

        [Test]
        public void LoadTargetGameAsync_ShouldThrowOnMissingGame()
        {
            var ex = Assert.ThrowsAsync<DirectoryNotFoundException>(async () =>
                await _service.LoadTargetGameAsync(TempDir, "not_exist"));
            StringAssert.Contains("Game directory not found", ex!.Message);
        }

        [Test]
        public async Task SaveProfilesAsync_And_LoadProfileAsync()
        {
            var profile = new ProfileInfo { ProfileName = "TestProfile", TargetDir = TempDir, Games = [] };
            var result = await _service.SaveProfilesAsync(SaveTempDir, profile);
            Assert.That(result, Is.True);
            var filePath = Path.Combine(SaveTempDir, "TestProfile.json");
            Assert.That(File.Exists(filePath), Is.True);
        }

        [Test]
        public async Task SaveProfilesDefaultAsync_WritesToAppData()
        {
            var profile = new ProfileInfo { ProfileName = "DefaultProfile", TargetDir = TempDir, Games = [] };
            var result = await _service.SaveProfilesDefaultAsync(profile);
            Assert.That(result, Is.True);
            var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            var expected = Path.Combine(appData, "PCL.Neo", "profiles", "DefaultProfile.json");
            Assert.That(File.Exists(expected), Is.True);
        }

        [Test]
        public async Task SaveGameInfoToProfileDefaultAsync_AddsGame()
        {
            var profile = new ProfileInfo { ProfileName = "GameProfile", TargetDir = TempDir, Games = [] };
            var game = new GameInfo
            {
                Name = "test",
                RootDirectory = TempDir,
                GameDirectory = "gd",
                IsIndie = false,
                Loader = ModLoader.None
            };
            var result = await _service.SaveGameInfoToProfileDefaultAsync(profile, game);
            Assert.That(result, Is.True);
            Assert.That(profile.Games, Has.Count.EqualTo(1));
            var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            var expected = Path.Combine(appData, "PCL.Neo", "profiles", "GameProfile.json");
            Assert.That(File.Exists(expected), Is.True);
        }

        [Test]
        public async Task SaveGameInfoToProfileAsync_AddsGame()
        {
            var profile = new ProfileInfo { ProfileName = "GameProfile2", TargetDir = TempDir, Games = [] };
            var game = new GameInfo
            {
                Name = "test2",
                RootDirectory = TempDir,
                GameDirectory = "gd2",
                IsIndie = true,
                Loader = ModLoader.None
            };
            var result = await _service.SaveGameInfoToProfileAsync(profile, game, SaveTempDir);
            Assert.That(result, Is.True);
            Assert.That(profile.Games, Has.Count.EqualTo(1));
            var filePath = Path.Combine(SaveTempDir, "GameProfile2.json");
            Assert.That(File.Exists(filePath), Is.True);
        }

        [Test]
        public void DeleteGame_ShouldDeleteGame()
            // this test method is not available, because i dont want to test that... - whiteacat346
        {
            var gameInfo = new GameInfo
            {
                GameDirectory = string.Empty,
                RootDirectory = string.Empty,
                IsIndie = true,
                Loader = ModLoader.None,
                Name = "None"
            };

            var profile = new ProfileInfo { ProfileName = "TempProfile", TargetDir = TempDir, Games = [gameInfo] };

            _service.DeleteGame(gameInfo, profile);
        }
    }
}
