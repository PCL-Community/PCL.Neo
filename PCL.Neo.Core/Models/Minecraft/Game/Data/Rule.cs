using PCL.Neo.Core.Utils;
using System.Text.Json.Serialization;

namespace PCL.Neo.Core.Models.Minecraft.Game.Data;

public partial class Rule
{
    [JsonPropertyName("action")] public string Action { get; set; } = string.Empty;

    [JsonPropertyName("os")] public OsRule? Os { get; set; }

    public bool Allow => Action == "allow";
}

public class OsRule
{
    [JsonPropertyName("name")] public string? Name { get; set; }

    /// 本条好像并没有参考性，因为API里写的全是X86
    [JsonPropertyName("arch")]
    public string? Arch { get; set; }
}

public class ArgRule
{
    [JsonPropertyName("allow")] public string Action { get; set; } = string.Empty;

    [JsonPropertyName("features")] public Dictionary<string, bool>? Features { get; set; }

    [JsonPropertyName("os")] public OsRule? Os { get; set; }
}

public partial class Rule
{
    private bool IsOsRuleAllow
    {
        get
        {
            if (Os is null) return true;
            bool isCurrentOs = this.Os.Name == SystemUtils.Os.ToMajangApiName();
            return (isCurrentOs && Allow) || (!isCurrentOs && !Allow);
        }
    }

    private bool IsArchRuleAllow
    {
        get
        {
            if (Os?.Arch is null) return true;
            bool isCurrentArch = Os.Arch == SystemUtils.Architecture.ToMajangApiName();
            return (isCurrentArch && Allow) || (!isCurrentArch && !Allow);
        }
    }

    private bool IsGameFeatureAllow => true; // TODO: 设置具体值 | need more information - whitecat346
    private bool GameArgumentsFilter => IsGameFeatureAllow || IsOsRuleAllow || IsArchRuleAllow;
    private bool JvmArgumentsFilter => IsOsRuleAllow || IsArchRuleAllow;
}