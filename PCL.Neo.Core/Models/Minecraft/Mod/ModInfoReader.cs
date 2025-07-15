using PCL.Neo.Core.Models.Minecraft.Mod.Data;
using System.IO.Compression;
using System.Text.Json;
using Tomlyn;

namespace PCL.Neo.Core.Models.Minecraft.Mod;

public class ModInfoReader
{
    private enum ModInfoType
    {
        Unknown,
        MetaInf,
        JsonInfo
    }

    private static (ModInfoType, ZipArchiveEntry?) GetModInfoType(ZipArchive archive)
    {
        var entry = archive.GetEntry("META-INF/mods.toml");
        if (entry != null)
        {
            return (ModInfoType.MetaInf, entry);
        }

        entry = archive.GetEntry("fabric.mod.json");
        if (entry != null)
        {
            return (ModInfoType.JsonInfo, entry);
        }

        return (ModInfoType.Unknown, null);
    }

    /// <summary>
    /// Get mod information from the specified mod directory.
    /// </summary>
    /// <param name="modDir">The directory that storage mods.</param>
    /// <returns></returns>
    /// <exception cref="DirectoryNotFoundException">Throw if given directory not found.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Throw if needed file is not found.</exception>
    public static async Task<IEnumerable<ModInfo>> GetModInfo(string modDir)
    {
        if (!Directory.Exists(modDir))
        {
            throw new DirectoryNotFoundException("Mods direcotry not found.");
        }

        var mods = new List<ModInfo>();
        var modFiles = Directory.GetFiles(modDir, "*.jar");

        foreach (var modFile in modFiles)
        {
            using var zipFile = ZipFile.OpenRead(modFile);

            var (type, archiveEntry) = GetModInfoType(zipFile); // get info type and entry

            ArgumentNullException.ThrowIfNull(archiveEntry);

            using var reader = new StreamReader(archiveEntry.Open());
            var rawContent = await reader.ReadToEndAsync();

            switch (type)
            {
                case ModInfoType.JsonInfo:
                    var content = new string(rawContent.Where(it => it != '\n').ToArray());
                    var jsonContent = JsonSerializer.Deserialize<FabricModInfo>(content);
                    ArgumentNullException.ThrowIfNull(jsonContent); // ensure deserialization was successful

                    // copy mod icon
                    if (!string.IsNullOrEmpty(jsonContent.Icon))
                    {
                        jsonContent.Icon = await CopyIcon(zipFile, jsonContent.Icon);
                    }
                    else
                    {
                        jsonContent.Icon = "Unknown";
                    }

                    // convert to ModInfo
                    string modSource;
                    if (jsonContent.Contact == null)
                    {
                        modSource = string.Empty;
                    }
                    else
                    {
                        modSource = string.IsNullOrEmpty(jsonContent.Contact.Sources)
                            ? jsonContent.Contact.Homepage
                            : jsonContent.Contact.Sources;
                    }

                    var modInfo = new ModInfo
                    {
                        Version = jsonContent.Version,
                        Name = string.IsNullOrEmpty(jsonContent.Name) ? jsonContent.Id : jsonContent.Name,
                        Description = jsonContent.Description,
                        Icon = jsonContent.Icon,
                        Url = modSource
                    };
                    mods.Add(modInfo);
                    break;
                case ModInfoType.MetaInf:
                    var tomlContent = Toml.ToModel<MetaModInfo>(rawContent).Mods.First();

                    // copy mod icon
                    if (!string.IsNullOrEmpty(tomlContent.LogoFile))
                    {
                        tomlContent.LogoFile = await CopyIcon(zipFile, tomlContent.LogoFile);
                    }
                    else
                    {
                        tomlContent.LogoFile = "Unknown";
                    }

                    // convert to ModInfo
                    var metaInfo = new ModInfo
                    {
                        Name = string.IsNullOrEmpty(tomlContent.DisplayName)
                            ? tomlContent.ModId
                            : tomlContent.DisplayName,
                        Version = tomlContent.Version,
                        Icon = tomlContent.LogoFile,
                        Description = tomlContent.Description,
                        Url = tomlContent.DisplayUrl
                    };
                    mods.Add(metaInfo);
                    break;

                case ModInfoType.Unknown:
                    var unknownInfo = new ModInfo
                    {
                        Name = "Unknown",
                        Description = "Can not read mod information.",
                        Icon = "Unknown",
                    };
                    mods.Add(unknownInfo);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        return mods;
    }

    private static readonly string ModIconDir = Path.Combine(Const.AppData, "modIcons");

    private static async Task<string> CopyIcon(ZipArchive archive, string iconPath)
    {
        var iconEntry = archive.GetEntry(iconPath);
        if (iconEntry == null)
        {
            throw new ArgumentNullException(nameof(iconEntry), "Icon not found.");
        }

        var tempIconPath = Path.Combine(ModIconDir, $"{Guid.NewGuid().ToString()[..8]}.png");
        await using var iconStream = iconEntry.Open();
        await using var fileStream = File.Create(tempIconPath);

        await iconStream.CopyToAsync(fileStream);

        return tempIconPath;
    }
}
