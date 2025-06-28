namespace PCL.Neo.Core.Models.Configuration;

[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
public sealed class ConfigurationInfoAttribute(string filePath) : Attribute
{
    /// <summary>
    /// 配置项的Json文件名
    /// </summary>
    public string FilePath { get; init; } = filePath;
}