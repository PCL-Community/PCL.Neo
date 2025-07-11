using PCL.Neo.Core.Utils;
using System.Collections.Immutable;

namespace PCL.Neo.Core.Models.Minecraft.Java;

/// <summary>
/// Fetches Java installations on Windows.
/// </summary>
public static class Windows
{
    private static readonly string[] _windowJavaSearchTerms = ["java",
        "jdk",
        "jbr",
        "bin",
        "env",
        "环境",
        "run",
        "软件",
        "jre",
        "bin",
        "mc",
        "software",
        "cache",
        "temp",
        "corretto",
        "roaming",
        "users",
        "craft",
        "program",
        "世界",
        "net",
        "游戏",
        "oracle",
        "game",
        "file",
        "data",
        "jvm",
        "服务",
        "server",
        "客户",
        "client",
        "整合",
        "应用",
        "运行",
        "前置",
        "mojang",
        "官启",
        "新建文件夹",
        "eclipse",
        "microsoft",
        "hotspot",
        "idea",
        "android",
    ];

    public static async Task<ImmutableArray<JavaRuntime>> SearchJavaAsync()
    {
        List<string> paths = [];
        // 获得环境变量
        string? environmentVariable = Environment.GetEnvironmentVariable("Path");
        if (environmentVariable != null)
        {
            string[] array = environmentVariable.Split(Path.PathSeparator);
            foreach (string path in array)
            {
                string temp = Path.Combine(path, "javaw.exe");
                if (File.Exists(temp))
                {
                    paths.Add(temp);
                }
            }
        }

        // 扫描硬盘
        var drives = DriveInfo.GetDrives();
        foreach (var drive in drives)
        {
            if (!drive.IsReady)
            {
                continue;
            }

            FetchJavaw(new DirectoryInfo(drive.Name), ref paths);
        }

        FetchJavaw(new DirectoryInfo(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)), ref paths);

        // FetchJavaw(new DirectoryInfo(AppDomain.CurrentDomain.SetupInformation.ApplicationBase)), ref paths);

        paths.Sort((string x, string s) => x.CompareTo(s));

        var validPaths = paths.Where(x => !string.IsNullOrWhiteSpace(x) && !x.Contains("javapath_target_") && File.Exists(x))
            .Select(Path.GetDirectoryName).Distinct();
        return [.. (await Task.WhenAll(validPaths.Select(static validPath => JavaRuntime.CreateJavaEntityAsync(validPath!))))
        .Where(r => r is { Compability: not JavaCompability.Error })!];
    }

    private static void FetchJavaw(DirectoryInfo directory, ref List<string> results)
    {
        if (directory.Exists)
        {
            var javaw = Path.Combine(directory.FullName, "javaw.exe");
            if (File.Exists(javaw)) results.Add(javaw);
            try
            {
                foreach (DirectoryInfo item in directory.EnumerateDirectories())
                {
                    if (!item.Attributes.HasFlag(FileAttributes.ReparsePoint))
                    {
                        string name = item.Name.ToLower();
                        if (item.Parent!.Name.ToLower() == "users" || _windowJavaSearchTerms.Any(name.Contains))
                        {
                            FetchJavaw(item, ref results);
                        }
                    }
                }
            }
            catch (UnauthorizedAccessException)
            {
                // 忽略权限错误
            }
        }
    }
}