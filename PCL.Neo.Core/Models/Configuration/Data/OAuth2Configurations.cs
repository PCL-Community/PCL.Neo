using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PCL.Neo.Core.Models.Configuration.Data;

[ConfigurationInfo("OAuth2Configuration.json")]
public record OAuth2Configurations
{
    public string ClientId     { get; init; } = string.Empty;
    public string ClientSecret { get; init; } = string.Empty;
    public int    RedirectPort { get; init; } = 5050;
}