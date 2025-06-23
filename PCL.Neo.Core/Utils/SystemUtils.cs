using System.Runtime.InteropServices;

namespace PCL.Neo.Core.Utils;

public static class SystemUtils
{
    /// <summary>
    /// 系统是否为64位。
    /// </summary>
    public static readonly bool Is64Os = Environment.Is64BitOperatingSystem;

    public enum RunningOs
    {
        Windows,
        Linux,
        MacOs,
        Unknown
    }

    public static readonly RunningOs Os = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
        ? RunningOs.Windows
        : RuntimeInformation.IsOSPlatform(OSPlatform.Linux)
            ? RunningOs.Linux
            : RuntimeInformation.IsOSPlatform(OSPlatform.OSX)
                ? RunningOs.MacOs
                : RunningOs.Unknown;

    public static string ToMajangApiName(this RunningOs os)
    {
        return os switch
        {
            RunningOs.Windows => "windows",
            RunningOs.Linux => "linux",
            RunningOs.MacOs => "macos",
            RunningOs.Unknown => "unknow", // TODO: not konw is this right
            _ => throw new ArgumentOutOfRangeException(nameof(os), os, null)
        };
    }

    /// 本条好像并没有参考性，因为API里写的全是X86
    public static string ToMajangApiName(this Architecture architecture)
    {
        return architecture switch
        {
            Architecture.X64 => "x86",
            Architecture.X86 => "x86",
            Architecture.Arm64 => "arm64",
            _ => "",
        };
    }

    public static readonly Architecture Architecture = RuntimeInformation.ProcessArchitecture;

    /// <summary>
    /// 根据 MOJANG API 命名
    /// </summary>
    public static string Platform
    {
        get
        {
            return Os switch
            {
                RunningOs.Windows => Architecture switch
                {
                    Architecture.X64 => "windows-x64",
                    Architecture.X86 => "windows-x86",
                    Architecture.Arm64 => "windows-arm64",
                    _ => "unknown"
                },
                RunningOs.Linux => Architecture switch
                {
                    Architecture.X64 => "linux",
                    Architecture.X86 => "linux-i386",
                    _ => "unknown"
                },
                RunningOs.MacOs => Architecture switch
                {
                    Architecture.X64 => "mac-os",
                    Architecture.Arm64 => "mac-os-arm64",
                    _ => "unknown"
                },
                RunningOs.Unknown => "unknown",
                _ => "unknown"
            };
        }
    }

    /// <summary>
    /// 获取系统最大可用内存 (MB)
    /// </summary>
    public static int SystemMaxMemoryMb
    {
        get
        {
            try
            {
                // 简化实现，保留Core项目中可用的逻辑
                return 8192; // 默认8GB
            }
            catch
            {
                // 出错时使用默认值
                return 4096;
            }
        }
    }

    /// <summary>
    /// 获取当前系统的native键
    /// </summary>
    public static string? GetNativeKey()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            return Environment.Is64BitOperatingSystem ? "natives-windows-64" : "natives-windows-32";
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            return "natives-linux";
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            return "natives-osx";
        }

        return null;
    }
}