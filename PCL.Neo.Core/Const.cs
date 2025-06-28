using System.Diagnostics;
using System.Runtime.InteropServices;

namespace PCL.Neo;

public static class Const
{
    /// <summary>
    /// 平台路径分隔符。
    /// </summary>
    public static readonly char Sep = System.IO.Path.DirectorySeparatorChar;

    /// <summary>
    /// 平台换行符。
    /// </summary>
    public static readonly string CrLf = Environment.NewLine;

    /// <summary>
    /// 程序的启动路径，以 <see cref="Sep"/> 结尾。
    /// </summary>
    public static readonly string Path = Environment.CurrentDirectory + Sep;

    /// <summary>
    /// 程序本体所在位置，无程序名
    /// </summary>
    public static readonly string PathWithoutName = AppDomain.CurrentDomain.BaseDirectory;

    /// <summary>
    /// 包含程序名的完整路径。
    /// </summary>
    public static readonly string PathWithName = Process.GetCurrentProcess().MainModule!.FileName;

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

    private static RunningOs? _os;

    public static RunningOs Os
    {
        get
        {
            if (_os != null)
            {
                return _os.Value;
            }

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                _os = RunningOs.Windows;
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                _os = RunningOs.Linux;
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                _os = RunningOs.MacOs;
            }
            else
            {
                _os = RunningOs.Unknown;
            }

            return _os.Value;
        }
    }
}