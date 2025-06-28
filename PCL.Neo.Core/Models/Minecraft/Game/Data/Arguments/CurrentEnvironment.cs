using System.Runtime.InteropServices;

namespace PCL.Neo.Core.Models.Minecraft.Game.Data.Arguments
{
    public record CurrentEnvironment(
        string OsName,
        string OsVersion,
        string OsArch)
    {
        public string OsName { get; } = OsName;
        public string OsVersion { get; } = OsVersion;
        public string OsArch { get; } = OsArch;

        // Factory method to get current system environment
        public static CurrentEnvironment GetCurrent()
        {
            string osName = Const.Os switch
            {
                Const.RunningOs.Windows => "windows",
                Const.RunningOs.Linux => "linux",
                Const.RunningOs.MacOs => "osx",
                Const.RunningOs.Unknown => Environment.OSVersion.Platform.ToString().ToLowerInvariant(),
                _ => throw new ArgumentOutOfRangeException(nameof(Const.Os))
            };

            string osArch = RuntimeInformation.ProcessArchitecture switch
            {
                Architecture.X86 => "x86",
                Architecture.X64 => "x64",
                Architecture.Arm => "arm",
                Architecture.Arm64 => "arm64",
                _ => RuntimeInformation.ProcessArchitecture.ToString().ToLowerInvariant()
            };

            return new CurrentEnvironment(osName, Environment.OSVersion.Version.ToString(), osArch);
        }
    }
}