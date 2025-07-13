namespace PCL.Neo.Core.Utils;

internal static class DirectoryUtil
{
    /// <summary>
    /// 为路径加上引号（如果包含空格）
    /// </summary>
    public static string QuotePath(string path)
    {
        // 统一路径分隔符为当前系统的分隔符
        path = path.Replace('\\', Path.DirectorySeparatorChar).Replace('/', Path.DirectorySeparatorChar);

        // 如果路径包含空格，则加上引号
        return path.Contains(' ') ? $"\"{path}\"" : path;
    }

    public static string ForceQuotePath(string path)
    {
        return $"\"{path}\"";
    }

    /// <summary>
    /// 确保目录存在
    /// </summary>
    public static void EnsureDirectoryExists(string path)
    {
        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }
    }
}
