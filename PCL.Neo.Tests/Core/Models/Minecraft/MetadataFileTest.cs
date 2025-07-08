using PCL.Neo.Core.Models.Minecraft;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json.Nodes;

namespace PCL.Neo.Tests.Core.Models.Minecraft;

public class MetadataFileTest
{
    [Test]
    public void Parse()
    {
        foreach (var metadataFilePath in Directory.EnumerateFiles("./MCMetadataFiles"))
        {
            var jsonObj = JsonNode.Parse(File.ReadAllText(metadataFilePath))!.AsObject();
            var meta = MetadataFile.Parse(jsonObj);
            Assert.That(meta.Arguments.Game, Is.Not.Empty);
            if (jsonObj.ContainsKey("arguments"))
            {
                Assert.That(meta.Arguments.Game.Count, Is.EqualTo(jsonObj["arguments"]!["game"]!.AsArray().Count));
            }

            Assert.Multiple(() =>
            {
                Assert.That(meta.Assets, Is.Not.Empty);
                Assert.That(meta.AssetIndex.Id, Is.Not.Empty);
                Assert.That(meta.AssetIndex.Path, Is.Null);
                Assert.That(meta.AssetIndex.Sha1, Is.Not.Empty);
                Assert.That(meta.AssetIndex.Size, Is.Not.Zero);
                Assert.That(meta.AssetIndex.TotalSize, Is.Not.Zero);
            });
            Assert.That(meta.Downloads, Is.Not.Empty);
            foreach (var (id, file) in meta.Downloads)
            {
                Assert.Multiple(() =>
                {
                    Assert.That(id, Is.Not.Empty);
                    Assert.That(file.Path, Is.Null);
                    Assert.That(file.Sha1, Is.Not.Empty);
                    Assert.That(file.Size, Is.Not.Zero);
                    Assert.That(file.Url, Is.Not.Empty);
                });
            }

            Assert.That(meta.Id, Is.Not.Empty);
            Assert.Multiple(() =>
            {
                if (meta.JavaVersion is null)
                    return;
                Assert.That(meta.JavaVersion.Component, Is.Not.Empty);
                Assert.That(meta.JavaVersion.MajorVersion, Is.Not.Zero);
            });
            Assert.That(meta.Libraries.Count, Is.EqualTo(jsonObj["libraries"]!.AsArray().Count));
            Assert.Multiple(() =>
            {
                if (meta.Logging is null)
                    return;
                Assert.That(meta.Logging, Is.Not.Empty);
                foreach (var (id, logging) in meta.Logging)
                {
                    Assert.That(id, Is.Not.Empty);
                    Assert.That(logging.Argument, Is.Not.Empty);
                    Assert.That(logging.File, Is.Not.Null);
                    Assert.Multiple(() =>
                    {
                        Assert.That(logging.File.Id, Is.Not.Empty);
                        Assert.That(logging.File.Path, Is.Null);
                        Assert.That(logging.File.Sha1, Is.Not.Empty);
                        Assert.That(logging.File.Size, Is.Not.Zero);
                        Assert.That(logging.File.Url, Is.Not.Empty);
                    });
                    Assert.That(logging.Type, Is.Not.Empty);
                }
            });
            Assert.That(meta.MainClass, Is.Not.Empty);
            Assert.That(meta.MinimumLauncherVersion, Is.Not.Zero);
            Assert.That(meta.ReleaseTime, Is.Not.Empty);
            Assert.That(meta.Time, Is.Not.Empty);
            Assert.That(meta.Type, Is.Not.EqualTo(ReleaseTypeEnum.Unknown));
        }
    }

    [Test]
    public void ArgumentsParsing()
    {
        object[] testGameArgs =
        [
            "--username",
            "${auth_player_name}",
            "--version",
            "${version_name}",
            "--gameDir",
            "${game_directory}",
            "--assetsDir",
            "${assets_root}",
            "--assetIndex",
            "${assets_index_name}",
            "--uuid",
            "${auth_uuid}",
            "--accessToken",
            "${auth_access_token}",
            "--clientId",
            "${clientid}",
            "--xuid",
            "${auth_xuid}",
            "--userType",
            "${user_type}",
            "--versionType",
            "${version_type}",
            new MetadataFile.ConditionalArg
            {
                Rules =
                [
                    new MetadataFile.Rule
                    {
                        Action = MetadataFile.Rule.ActionEnum.Allow,
                        Features = new Dictionary<string, bool> { ["is_demo_user"] = true }
                    }
                ],
                Value = ["--demo"]
            },
            new MetadataFile.ConditionalArg
            {
                Rules =
                [
                    new MetadataFile.Rule
                    {
                        Action = MetadataFile.Rule.ActionEnum.Allow,
                        Features = new Dictionary<string, bool> { ["has_custom_resolution"] = true }
                    }
                ],
                Value =
                [
                    "--width",
                    "${resolution_width}",
                    "--height",
                    "${resolution_height}"
                ]
            },
            new MetadataFile.ConditionalArg
            {
                Rules =
                [
                    new MetadataFile.Rule
                    {
                        Action = MetadataFile.Rule.ActionEnum.Allow,
                        Features = new Dictionary<string, bool> { ["has_quick_plays_support"] = true }
                    }
                ],
                Value =
                [
                    "--quickPlayPath",
                    "${quickPlayPath}"
                ]
            },
            new MetadataFile.ConditionalArg
            {
                Rules =
                [
                    new MetadataFile.Rule
                    {
                        Action = MetadataFile.Rule.ActionEnum.Allow,
                        Features = new Dictionary<string, bool> { ["is_quick_play_singleplayer"] = true }
                    }
                ],
                Value =
                [
                    "--quickPlaySingleplayer",
                    "${quickPlaySingleplayer}"
                ]
            },
            new MetadataFile.ConditionalArg
            {
                Rules =
                [
                    new MetadataFile.Rule
                    {
                        Action = MetadataFile.Rule.ActionEnum.Allow,
                        Features = new Dictionary<string, bool> { ["is_quick_play_multiplayer"] = true }
                    }
                ],
                Value =
                [
                    "--quickPlayMultiplayer",
                    "${quickPlayMultiplayer}"
                ]
            },
            new MetadataFile.ConditionalArg
            {
                Rules =
                [
                    new MetadataFile.Rule
                    {
                        Action = MetadataFile.Rule.ActionEnum.Allow,
                        Features = new Dictionary<string, bool> { ["is_quick_play_realms"] = true }
                    }
                ],
                Value =
                [
                    "--quickPlayRealms",
                    "${quickPlayRealms}"
                ]
            }
        ];

        var jsonObj = JsonNode.Parse(File.ReadAllText("./MCMetadataFiles/1.21.5.json"))!.AsObject();
        var meta = MetadataFile.Parse(jsonObj);
        Assert.That(meta.Arguments.Game.Count, Is.EqualTo(testGameArgs.Length));
        for (var i = 0; i < meta.Arguments.Game.Count; i++)
        {
            if (testGameArgs[i] is string)
            {
                Assert.That(meta.Arguments.Game[i].Value.Count, Is.EqualTo(1));
                Assert.That(meta.Arguments.Game[i].Value[0], Is.EqualTo(testGameArgs[i]));
            }
            else
            {
                var arg = meta.Arguments.Game[i];
                var testArg = (MetadataFile.ConditionalArg)testGameArgs[i];

                Assert.That(arg.Value.SequenceEqual(testArg.Value), Is.True);
                Assert.That(
                    (arg.Rules is null && testArg.Rules is null) ||
                    (arg.Rules is not null && testArg.Rules is not null));
                if (arg.Rules is not null && testArg.Rules is not null)
                {
                    Assert.That(arg.Rules.Count, Is.EqualTo(testArg.Rules.Count));
                    foreach (var (rule, testRule) in arg.Rules.Zip(testArg.Rules))
                    {
                        Assert.That(rule.Action, Is.EqualTo(testRule.Action));
                        Assert.That((rule.Features is null && testRule.Features is null) ||
                                    (rule.Features is not null && testRule.Features is not null));
                        if (rule.Features is not null && testRule.Features is not null)
                            Assert.That(rule.Features.SequenceEqual(testRule.Features));
                        Assert.That((rule.Os is null && testRule.Os is null) ||
                                    (rule.Os is not null && testRule.Os is not null));
                        if (rule.Os is not null && testRule.Os is not null)
                        {
                            Assert.That(rule.Os.Arch, Is.EqualTo(testRule.Os.Arch));
                            Assert.That(rule.Os.Name, Is.EqualTo(testRule.Os.Name));
                            Assert.That(rule.Os.Version, Is.EqualTo(testRule.Os.Version));
                        }
                    }
                }
            }
        }
    }
}
