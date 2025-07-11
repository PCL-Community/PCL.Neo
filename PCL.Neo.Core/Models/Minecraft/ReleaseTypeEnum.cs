using System.Text.Json.Serialization;

namespace PCL.Neo.Core.Models.Minecraft;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ReleaseTypeEnum
{
    Unknown,
    [JsonStringEnumMemberName("release")] Release,
    [JsonStringEnumMemberName("snapshot")] Snapshot,
    [JsonStringEnumMemberName("old_alpha")] OldAlpha,
    [JsonStringEnumMemberName("old_beta")] OldBeta
}