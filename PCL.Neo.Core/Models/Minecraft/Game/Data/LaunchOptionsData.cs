namespace PCL.Neo.Models.Minecraft.Game.Data;

public class LaunchOptionsData
{
    public string VersionId { get; set; } = string.Empty;
    public string JavaPath { get; set; } = string.Empty;
    public string MinecraftDirectory { get; set; } = string.Empty;
    public int MaxMemoryMB { get; set; } = 2048;
    public int MinMemoryMB { get; set; } = 512;
    public string Username { get; set; } = "Player";
    public string UUID { get; set; } = "00000000-0000-0000-0000-000000000000";
    public string AccessToken { get; set; } = "0";
    public string LauncherName { get; set; } = "PCL.Neo";
    public string LauncherVersion { get; set; } = "1.0.0";
    public string GameDirectory { get; set; } = string.Empty;
    public string AssetsDirectory { get; set; } = string.Empty;
    public string AssetIndex { get; set; } = string.Empty;
    public string NativesDirectory { get; set; } = string.Empty;
    public string ClassPath { get; set; } = string.Empty;
    public int GameWidth { get; set; } = 854;
    public int GameHeight { get; set; } = 480;
    public bool FullScreen { get; set; } = false;
    public Dictionary<string, string>? ExtraJvmArgs { get; set; }
    public Dictionary<string, string>? ExtraGameArgs { get; set; }
}
