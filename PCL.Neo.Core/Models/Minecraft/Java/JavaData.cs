using PCL.Neo.Core.Utils;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace PCL.Neo.Core.Models.Minecraft.Java;

/// <summary>
/// 每一个 Java 实体的信息类
/// </summary>
public class JavaRuntime
{
    /// <summary>
    /// 该 Java 实体的父目录，在构造时传入
    /// </summary>
    public string DirectoryPath { get; }

    /// <summary>
    /// 描述具体的 Java 信息，内部信息，不应在外部取用
    /// </summary>
    private JavaInfo _javaInfo;

    /// <summary>
    /// 私有构造函数，需要路径和信息
    /// </summary>
    /// <param name="directoryPath"></param>
    /// <param name="javaInfo"></param>
    private JavaRuntime(string directoryPath, JavaInfo javaInfo)
    {
        DirectoryPath = directoryPath;
        _javaInfo = javaInfo;
    }

    /// <summary>
    /// 具体的 Java 信息数据结构
    /// </summary>
    private record JavaInfo
    {
        public string Version { get; set; } = string.Empty;
        public int SlugVersion { get; set; }

        public ExeArchitectureUtils.ExeArchitecture Architecture { get; set; } =
            ExeArchitectureUtils.ExeArchitecture.Unknown;

        public bool IsJre { get; init; }
        public JavaCompability Compability { get; set; } = JavaCompability.Unknown;
        public required string JavaExe { get; init; }
        public required string JavaWExe { get; init; }
        public string? Implementor { get; set; }
    }

    /// <summary>
    /// 单个Java 实体的工厂函数
    /// </summary>
    /// <param name="directoryPath">Java 可执行文件的父目录</param>
    /// <param name="isUserImport">是否为用户手动导入</param>
    /// <returns>得到的 Java 运行时，如果初始化失败会返回 null，需要在外部判断</returns>
    public static async Task<JavaRuntime?> CreateJavaEntityAsync(string directoryPath, bool isUserImport = false)
    {
        Debug.WriteLine($"创建 JavaRuntime: {directoryPath}");

        var javaInfo = await JavaInfoInitAsync(directoryPath);

        if (javaInfo.Compability == JavaCompability.Error)
        {
            return null;
        }

        var javaEntity = new JavaRuntime(directoryPath, javaInfo) { IsUserImport = isUserImport };

        return javaEntity;
    }

    /// <summary>
    /// 是否为用户手动导入，手动导入的运行时在刷新时不会被刷新掉
    /// </summary>
    public bool IsUserImport { get; set; }

    /// <summary>
    /// Java版本信息
    /// </summary>
    public string Version => _javaInfo.Version;

    /// <summary>
    /// Java 数字版本
    /// </summary>
    public int SlugVersion => _javaInfo.SlugVersion;

    /// <summary>
    /// 是否为64Bit版本
    /// </summary>
    public bool Is64Bit => _javaInfo.Architecture.Is64Bit();

    /// <summary>
    /// Java 架构信息
    /// </summary>
    public ExeArchitectureUtils.ExeArchitecture Architecture => _javaInfo.Architecture;

    /// <summary>
    /// 供应商
    /// </summary>
    public JavaCompability Compability => _javaInfo.Compability;

    /// <summary>
    /// 是否为 JRE（Java Runtime Environment）
    /// </summary>
    public bool IsJre => _javaInfo.IsJre;

    /// <summary>
    /// JavaExe路径
    /// </summary>
    public string JavaExe => _javaInfo.JavaExe;

    /// <summary>
    /// Windows 特有的 javaw.exe
    /// </summary>
    public string JavaWExe => _javaInfo.JavaWExe;

    public string? Implementor => _javaInfo.Implementor;

    /// <summary>
    /// 异步地初始化 Java 信息
    /// </summary>
    /// <param name="directoryPath">需要初始化的 Java 所在的父目录</param>
    /// <returns>得到的 Java 具体信息，JavaCompability 为 Error 时为无效</returns>
    private static async Task<JavaInfo> JavaInfoInitAsync(string directoryPath)
    {
        // 首先直接设置JavaExe、JavawExe、releaseFile、javac的路径
        var javaExe = Path.Combine(directoryPath,
            SystemUtils.Os is SystemUtils.RunningOs.Windows ? "java.exe" : "java");
        string? releaseFile = null;
        var parentDir = Directory.GetParent(directoryPath);
        if (parentDir != null) releaseFile = Path.Combine(parentDir.FullName, "release");
        var javacPath = SystemUtils.Os is SystemUtils.RunningOs.Windows
            ? Path.Combine(directoryPath, "javac.exe")
            : Path.Combine(directoryPath, "javac");
        var info = new JavaInfo
        {
            JavaExe = javaExe,
            JavaWExe = SystemUtils.Os is SystemUtils.RunningOs.Windows
                ? Path.Combine(directoryPath, "javaw.exe")
                : javaExe,
            IsJre = !File.Exists(javacPath)
        };

        // 尝试读取RELEASE文件的信息
        if (releaseFile != null && File.Exists(releaseFile))
            (info.Implementor, info.Version, info.Architecture) = ReadReleaseFile(releaseFile);

        // 若版本未被设置，运行 java -version 获取版本
        if (string.IsNullOrWhiteSpace(info.Version))
        {
            string runJavaOutput;
            try
            {
                runJavaOutput = await GetRunJavaOutputAsync(javaExe).ConfigureAwait(false);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                info.Compability = JavaCompability.Error;
                return info;
            }

            info.Version = MatchVersion(runJavaOutput) ?? string.Empty;
        }

        // 设置slug Version
        var versionSplit = info.Version.Split('.');
        info.SlugVersion = int.TryParse(versionSplit[0] == "1" ? versionSplit[1] : versionSplit[0], out var slugVersion)
            ? slugVersion
            : 0;

        // 若架构未被设置，读取PE/ELF/Mach-O文件头获得架构
        if (info.Architecture == ExeArchitectureUtils.ExeArchitecture.Unknown)
            info.Architecture = ExeArchitectureUtils.GetExecutableArchitecture(javaExe);

        // 设置兼容性标识
        info.Compability = info.Architecture.GetJavaCompability();
        return info;
    }

    /// <summary>
    /// 刷新 Java 实体的信息
    /// </summary>
    public async Task<bool> RefreshInfoAsync()
    {
        _javaInfo = await JavaInfoInitAsync(DirectoryPath).ConfigureAwait(false);
        return _javaInfo.Compability != JavaCompability.Error;
    }

    /// <summary>
    /// 运行 java -version 并获取输出
    /// </summary>
    /// <returns></returns>
    private static async Task<string> GetRunJavaOutputAsync(string javaExe)
    {
        using var javaProcess = new Process();
        javaProcess.StartInfo = new ProcessStartInfo
        {
            FileName = javaExe,
            Arguments = "-version",
            UseShellExecute = false,
            CreateNoWindow = true,
            RedirectStandardError = true,
            RedirectStandardOutput = true
        };
        // -version      将产品版本输出到错误流并退出
        // --version     将产品版本输出到输出流并退出
        // 但是格式不一样
        javaProcess.Start();
        await javaProcess.WaitForExitAsync().ConfigureAwait(false);
        var output = await javaProcess.StandardError.ReadToEndAsync();
        return output;
    }

    private static string? MatchVersion(string runJavaOutput)
    {
        var regexMatch = Regex.Match(runJavaOutput, """version\s+"([\d._]+)""");
        return regexMatch.Success ? regexMatch.Groups[1].Value : null;
    }

    private static ValueTuple<string?, string, ExeArchitectureUtils.ExeArchitecture> ReadReleaseFile(string releaseFile)
    {
        if (!File.Exists(releaseFile))
        {
            throw new FileNotFoundException("Release file not found.", releaseFile);
        }

        var implementor = string.Empty;
        var version = string.Empty;
        var osArch = ExeArchitectureUtils.ExeArchitecture.Unknown;
        foreach (var line in File.ReadLines(releaseFile))
        {
            if (line.StartsWith("IMPLEMENTOR="))
                implementor = line.Split('=')[1].Trim('"');
            else if (line.StartsWith("JAVA_VERSION="))
                version = line.Split('=')[1].Trim('"');
            else if (line.StartsWith("OS_ARCH="))
            {
                var arch = line.Split('=')[1].Trim('"');
                osArch = arch switch
                {
                    "x86_64" => ExeArchitectureUtils.ExeArchitecture.X64,
                    "aarch64" => ExeArchitectureUtils.ExeArchitecture.Arm64,
                    // TODO: 增加其他架构种类
                    _ => ExeArchitectureUtils.ExeArchitecture.Unknown
                };
            }

            if (!string.IsNullOrEmpty(implementor)
                && !string.IsNullOrEmpty(version)
                && osArch is not ExeArchitectureUtils.ExeArchitecture.Unknown)
            {
                break;
            }
        }

        return (implementor, version, osArch);
    }
}
