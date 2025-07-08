using System.Runtime.InteropServices;

namespace PCL.Neo.Core.Utils;

/// <summary>
/// 描述某个 Java 环境与当前系统是否兼容
/// </summary>
public enum JavaCompability
{
    /// <summary>
    /// 未知，例如尚未初始化
    /// </summary>
    Unknown,

    /// <summary>
    /// 能够原生运行
    /// </summary>
    Yes,

    /// <summary>
    /// 不兼容
    /// </summary>
    No,

    /// <summary>
    /// 对于 Win11 on Arm 或者 M 芯片 macOS 可以转译运行
    /// </summary>
    UnderTranslation,

    /// <summary>
    /// 初始化失败或者不是 Java 可执行文件
    /// </summary>
    Error
}

public static class ExeArchitectureUtils
{
    public enum ExeArchitecture
    {
        X86,
        X64,
        Arm64,
        FatFile,
        Unknown
    }

    public static ExeArchitecture GetExecutableArchitecture(string path)
    {
        if (!File.Exists(path))
            throw new FileNotFoundException("Executable file not found.", path);
        // Console.WriteLine("--------------------");
        // Console.WriteLine("PE: " + ReadPeHeader(path));
        // Console.WriteLine("ELF: " + ReadElfHeader(path));
        // Console.WriteLine("Mach-O: " + ReadMachOHeader(path));
        return SystemUtils.Os switch
        {
            SystemUtils.RunningOs.Windows => ReadPeHeader(path),
            SystemUtils.RunningOs.Linux => ReadElfHeader(path),
            SystemUtils.RunningOs.MacOs => ReadMachOHeader(path),
            _ => ExeArchitecture.Unknown
        };
    }

    private static ExeArchitecture ReadPeHeader(string filePath)
    {
        using var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read);
        using var reader = new BinaryReader(fs);

        // 检查是否为有效的 PE 文件
        fs.Seek(0x3C, SeekOrigin.Begin); // PE header 偏移地址
        var peHeaderOffset = reader.ReadInt32();
        fs.Seek(peHeaderOffset, SeekOrigin.Begin);

        var peSignature = reader.ReadUInt32();
        if (peSignature != 0x00004550) // "PE\0\0"
            return ExeArchitecture.Unknown;

        // 读取机器类型
        var machine = reader.ReadUInt16();
        return machine switch
        {
            0x014C => ExeArchitecture.X86, // IMAGE_FILE_MACHINE_I386
            0x8664 => ExeArchitecture.X64, // IMAGE_FILE_MACHINE_AMD64
            0xAA64 => ExeArchitecture.Arm64, // IMAGE_FILE_MACHINE_ARM64
            _ => ExeArchitecture.Unknown
        };
    }

    private static ExeArchitecture ReadElfHeader(string filePath)
    {
        using var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read);
        using var reader = new BinaryReader(fs);

        var elfMagic = reader.ReadBytes(4);
        if (elfMagic[0] != 0x7F || elfMagic[1] != 'E' || elfMagic[2] != 'L' || elfMagic[3] != 'F')
            return ExeArchitecture.Unknown;

        fs.Seek(4, SeekOrigin.Begin); // Skip EI_MAG
        var eiClass = reader.ReadByte(); // 1=32-bit, 2=64-bit

        // Skip data encoding (ei_data), version (ei_version), etc.
        fs.Seek(0x12, SeekOrigin.Begin); // e_machine offset for ELF32/ELF64
        var machine = reader.ReadUInt16();

        return machine switch
        {
            0x0003 => ExeArchitecture.X86, // EM_386
            0x003E => ExeArchitecture.X64, // EM_X86_64
            0x00B7 => ExeArchitecture.Arm64, // EM_AARCH64
            _ => ExeArchitecture.Unknown
        };
    }

    private static ExeArchitecture ReadMachOHeader(string filePath)
    {
        using var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read);
        using var reader = new BinaryReader(fs);

        var magic = reader.ReadUInt32();

        // 判断是否为 Fat File
        if (magic == 0xBEBAFECA) // FAT_MAGIC or FAT_MAGIC_64
        {
            return ExeArchitecture.FatFile;
        }

        var is64Bit = false;
        switch (magic)
        {
            case 0xFEEDFACE: // MH_MAGIC
            case 0xCEFAEDFE: // MH_CIGAM (reverse)
                break;
            case 0xFEEDFACF: // MH_MAGIC_64
            case 0xCFFAEDFE: // MH_CIGAM_64 (reverse)
                is64Bit = true;
                break;
            default:
                return ExeArchitecture.Unknown;
        }

        fs.Seek(is64Bit ? 4 : 0, SeekOrigin.Begin); // Skip magic
        var cputype = reader.ReadUInt32();

        return cputype switch
        {
            0x7 => ExeArchitecture.X86, // CPU_TYPE_I386
            0x1000007 => ExeArchitecture.X64, // CPU_TYPE_X86_64
            0x100000C => ExeArchitecture.Arm64, // CPU_TYPE_ARM64
            _ => ExeArchitecture.Unknown
        };
    }

    public static bool Is64Bit(this ExeArchitecture arch)
    {
        return arch is ExeArchitecture.X64 or ExeArchitecture.Arm64 or ExeArchitecture.FatFile;
    }

    public static JavaCompability GetJavaCompability(this ExeArchitecture arch)
    {
        if (arch.ToString() == SystemUtils.Architecture.ToString())
        {
            return JavaCompability.Yes;
        }

        return SystemUtils.Os switch
        {
            SystemUtils.RunningOs.Windows when SystemUtils.Architecture is Architecture.X64 or Architecture.Arm64 =>
                JavaCompability.UnderTranslation,
            SystemUtils.RunningOs.MacOs when SystemUtils.Architecture is Architecture.Arm64 &&
                                             arch is ExeArchitecture.X64 => JavaCompability.UnderTranslation,
            SystemUtils.RunningOs.MacOs when arch is ExeArchitecture.FatFile => JavaCompability.Yes,
            SystemUtils.RunningOs.Linux => JavaCompability.No,
            SystemUtils.RunningOs.Unknown => JavaCompability.Unknown,
            _ => JavaCompability.Unknown
        };
    }
}
