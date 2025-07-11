using PCL.Neo.Core.Models.Minecraft.Game.Data;
using PCL.Neo.Core.Models.Minecraft.Java;
using PCL.Neo.Core.Service.Game;
using PCL.Neo.Core.Utils;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PCL.Neo.Tests.Core.Models.Minecraft.Game
{
    [TestFixture]
    [TestOf(typeof(GameLauncherService))]
    public class GameLauncherServiceTest
    {
        [Test]
        public async Task GameArgumentsTest()
        {
            var launchOptions = new LaunchOptions
            {
                RunnerJava =
                    await JavaRuntime.CreateJavaEntityAsync(
                        @"C:\Users\WhiteCAT\Documents\Java\zulu17.58.21-ca-jdk17.0.15-win_x64\bin"),
                MaxMemoryMB = 4096,
                MinMemoryMB = 512, // 最小内存设为最大内存的1/4，但不低于512MB
                Username = "Steve",
                UUID = Uuid.GenerateUuid("Steve",
                    Uuid.UuidGenerateType.Standard),
                AccessToken = Guid.NewGuid()
                    .ToString(),
                WindowWidth = 854,
                WindowHeight = 480,
                FullScreen = false,
                IsOfflineMode = true,

                // 添加额外的JVM参数
                ExtraJvmArgs = [],

                // 添加额外的游戏参数
                ExtraGameArgs = [],

                // 环境变量
                EnvironmentVariables = new Dictionary<string, string>
                {
                    {
                        "JAVA_TOOL_OPTIONS", "-Dfile.encoding=UTF-8"
                    }
                },

                // 是否启动后关闭启动器
                CloseAfterLaunch = false,
            };

            var launcher = new GameLauncherService(new GameProfile
            {
                Options = launchOptions,
                Information = new GameInfo
                {
                    GameDirectory =
                        @"C:\Users\WhiteCAT\Desktop\Games\PCL2\.minecraft\versions\1.20.4-Fabric 0.15.11-[轻量通用]",
                    RootDirectory = @"C:\Users\WhiteCAT\Desktop\Games\PCL2\.minecraft",
                    Name = "1.20.4-Fabric 0.15.11-[轻量通用]",
                }
            });


            var result = await launcher.BuildLaunchCommandAsync();


            Console.WriteLine(string.Join('\n', result));
        }
    }
}
