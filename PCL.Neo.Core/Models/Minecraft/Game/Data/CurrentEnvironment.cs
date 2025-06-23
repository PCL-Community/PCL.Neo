using System.Runtime.InteropServices;

namespace PCL.Neo.Core.Models.Minecraft.Game.Data
{
    public record CurrentEnvironment(
        string OsName,
        string OsVersion,
        string OsArch,
        FeatureCunstomKv ActiveFeatures)
    {
        public string OsName { get; } = OsName;
        public string OsVersion { get; } = OsVersion;
        public string OsArch { get; } = OsArch;
        public FeatureCunstomKv ActiveFeatures { get; } = ActiveFeatures;

        // Factory method to get current system environment
        public static CurrentEnvironment GetCurrent(FeatureCunstomKv activeFeatures)
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

            return new CurrentEnvironment(osName, Environment.OSVersion.Version.ToString(), osArch, activeFeatures);
        }
    }
}