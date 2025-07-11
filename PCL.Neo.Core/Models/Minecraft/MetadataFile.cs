using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace PCL.Neo.Core.Models.Minecraft;

public class MetadataFile
{
    private JsonObject _rawMetadata = new();

    #region Model Classes

    public class Rule
    {
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public enum ActionEnum
        {
            Unknown,
            [JsonStringEnumMemberName("allow")] Allow,
            [JsonStringEnumMemberName("disallow")] Disallow
        }

        [JsonPropertyName("action")] public ActionEnum Action { get; set; } = ActionEnum.Allow;
        [JsonPropertyName("features")] public Dictionary<string, bool>? Features { get; set; } = null;
        [JsonPropertyName("os")] public OsModel? Os { get; set; } = null;

        public class OsModel
        {
            [JsonConverter(typeof(JsonStringEnumConverter))]
            public enum ArchEnum
            {
                Unknown,
                [JsonStringEnumMemberName("x64")] X64,
                [JsonStringEnumMemberName("x86")] X86
            }

            [JsonConverter(typeof(JsonStringEnumConverter))]
            public enum NameEnum
            {
                Unknown,
                [JsonStringEnumMemberName("windows")] Windows,
                [JsonStringEnumMemberName("linux")] Linux,
                [JsonStringEnumMemberName("osx")] Osx
            }

            [JsonPropertyName("arch")] public ArchEnum? Arch { get; set; } = null;
            [JsonPropertyName("name")] public NameEnum? Name { get; set; } = null;
            [JsonPropertyName("version")] public string? Version { get; set; } = null; // regex
        }
    }

    public class ConditionalArg
    {
        [JsonPropertyName("rules")] public List<Rule>? Rules { get; set; }
        [JsonPropertyName("value")] public List<string> Value { get; set; }
    }

    public class ArgumentsModel
    {
        public List<ConditionalArg> Game { get; set; } = [];
        public List<ConditionalArg> Jvm { get; set; } = [];
    }

    public class JavaVersionModel
    {
        [JsonPropertyName("component")] public string Component { get; set; } = string.Empty;
        [JsonPropertyName("majorVersion")] public int MajorVersion { get; set; } = 0;
    }

    public class RemoteFileModel
    {
        [JsonPropertyName("id")] public string Id { get; set; } = string.Empty;
        [JsonPropertyName("path")] public string? Path { get; set; } = null;
        [JsonPropertyName("sha1")] public string Sha1 { get; set; } = string.Empty;
        [JsonPropertyName("size")] public int Size { get; set; }
        [JsonPropertyName("url")] public string Url { get; set; } = string.Empty;
    }

    public class AssetIndexModel : RemoteFileModel
    {
        [JsonPropertyName("totalSize")] public int TotalSize { get; set; } = 0;
    }

    public class LibraryModel
    {
        [JsonPropertyName("downloads")] public DownloadsModel Downloads { get; set; } = new();
        [JsonPropertyName("extract")] public ExtractModel? Extract { get; set; } = null;
        [JsonPropertyName("name")] public string Name { get; set; } = string.Empty;
        [JsonPropertyName("natives")] public Dictionary<string, string>? Natives { get; set; } = null;
        [JsonPropertyName("rules")] public List<Rule>? Rules { get; set; } = null;

        public class DownloadsModel
        {
            [JsonPropertyName("artifact")] public RemoteFileModel? Artifact { get; set; } = null;

            [JsonPropertyName("classifiers")]
            public Dictionary<string, RemoteFileModel>? Classifiers { get; set; } = null;
        }

        public class ExtractModel
        {
            [JsonPropertyName("exclude")] public List<string> Exclude { get; set; } = [];
        }
    }

    public class LoggingModel
    {
        [JsonPropertyName("argument")] public string Argument { get; set; } = string.Empty;
        [JsonPropertyName("file")] public RemoteFileModel File { get; set; } = new();
        [JsonPropertyName("type")] public string Type { get; set; } = string.Empty;
    }

    #endregion

    #region Metadata Fields

    public ArgumentsModel Arguments { get; set; } = new();
    public AssetIndexModel AssetIndex { get; set; } = new();
    public string Assets { get; set; } = string.Empty;
    public int? ComplianceLevel { get; set; } // field missing in 1.6.4.json
    public Dictionary<string, RemoteFileModel> Downloads { get; set; } = [];
    public string Id { get; set; } = string.Empty;
    public JavaVersionModel? JavaVersion { get; set; } = new(); // field missing in 1.6.1.json
    public List<LibraryModel> Libraries { get; set; } = [];
    public Dictionary<string, LoggingModel>? Logging { get; set; }
    public string MainClass { get; set; } = string.Empty;
    public int MinimumLauncherVersion { get; set; }
    public string ReleaseTime { get; set; } = string.Empty;
    public string Time { get; set; } = string.Empty;
    public ReleaseTypeEnum Type { get; set; }

    #endregion

    #region Parse Methods

    // For simplicity, we assume the metadata files are always valid
// ReSharper disable PossibleNullReferenceException
// ReSharper disable AssignNullToNotNullAttribute
#nullable disable
#pragma warning disable IL2026
    public static MetadataFile Parse(string json) =>
        Parse(JsonNode.Parse(json)!.AsObject());

    public static MetadataFile Parse(JsonNode json) =>
        Parse(json.AsObject());

    public static MetadataFile Parse(JsonObject json)
    {
        var mf = new MetadataFile { _rawMetadata = json };

        #region Arguments

        if (mf._rawMetadata.ContainsKey("arguments"))
        {
            ParseArguments(mf.Arguments.Game, "game");
            ParseArguments(mf.Arguments.Jvm, "jvm");

            // TODO: convert this to json converter
            void ParseArguments(List<ConditionalArg> toBeFilled, string propertyName)
            {
                toBeFilled.Clear();
                foreach (var param in mf._rawMetadata["arguments"][propertyName].AsArray())
                {
                    if (param.GetValueKind() == JsonValueKind.String)
                        toBeFilled.Add(new ConditionalArg { Value = [param.GetValue<string>()] });
                    else if (param.GetValueKind() == JsonValueKind.Object)
                    {
                        var rules = param["rules"].Deserialize<List<Rule>>();
                        List<string> value = null;
                        if (param["value"].GetValueKind() == JsonValueKind.String)
                            value = [param["value"].GetValue<string>()];
                        else if (param["value"].GetValueKind() == JsonValueKind.Array)
                            value = param["value"].Deserialize<List<string>>();
                        toBeFilled.Add(new ConditionalArg { Rules = rules, Value = value });
                    }
                }
            }
        }
        else if (mf._rawMetadata.ContainsKey("minecraftArguments"))
        {
            var argStr = mf._rawMetadata["minecraftArguments"].GetValue<string>();
            mf.Arguments.Game = argStr
                .Split(' ')
                .Select(x => new ConditionalArg { Value = [x] })
                .ToList();
        }
        else
            throw new Exception("Unknown Metadata File version");

        #endregion

        #region Logging

        if (mf._rawMetadata.ContainsKey("logging"))
            mf.Logging = mf._rawMetadata["logging"].Deserialize<Dictionary<string, LoggingModel>>();

        #endregion

        #region Common Fields

        mf.AssetIndex = mf._rawMetadata["assetIndex"].Deserialize<AssetIndexModel>();
        mf.Assets = mf._rawMetadata["assets"].GetValue<string>();

        mf.ComplianceLevel = mf._rawMetadata["complianceLevel"]?.GetValue<int>(); // field missing in 1.6.4.json

        mf.Downloads = mf._rawMetadata["downloads"].Deserialize<Dictionary<string, RemoteFileModel>>();

        mf.Id = mf._rawMetadata["id"].GetValue<string>();
        mf.JavaVersion = mf._rawMetadata["javaVersion"].Deserialize<JavaVersionModel>();

        mf.Libraries = mf._rawMetadata["libraries"].Deserialize<List<LibraryModel>>();

        mf.MainClass = mf._rawMetadata["mainClass"].GetValue<string>();
        mf.MinimumLauncherVersion = mf._rawMetadata["minimumLauncherVersion"].GetValue<int>();
        mf.ReleaseTime = mf._rawMetadata["releaseTime"].GetValue<string>();
        mf.Time = mf._rawMetadata["time"].GetValue<string>();
        mf.Type = mf._rawMetadata["type"].Deserialize<ReleaseTypeEnum>();

        #endregion

        return mf;
    }
#pragma warning restore IL2026
#nullable restore
// ReSharper restore AssignNullToNotNullAttribute
// ReSharper restore PossibleNullReferenceException

    #endregion
}