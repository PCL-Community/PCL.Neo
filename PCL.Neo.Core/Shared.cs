using PCL.Neo.Core.Models.Configuration;
using PCL.Neo.Core.Models.Configuration.Data;

namespace PCL.Neo.Core;

// you should put every shared resources here!
internal static class Shared
{
    public static readonly HttpClient HttpClient = new();

    public static readonly Lazy<ConfigurationAccessor<AppSettingsData>> AppSettings =
        new(() =>
        {
            var accessor = ConfigurationManager.Instance.GetAccessor<AppSettingsData>();
            return accessor;
        }, true);
}
