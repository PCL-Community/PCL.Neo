namespace PCL.Neo.Core.Models.Minecraft.Game.Data
{
    /// <summary>
    /// 模组信息
    /// </summary>
    public record ModInfo
    {
        public required string Id          { get; init; }
        public required string Name        { get; set; }
        public          string Version     { get; set; } = string.Empty;
        public          bool   Enabled     { get; set; } = true;
        public          string FilePath    { get; set; } = string.Empty;
        public          string Description { get; set; } = string.Empty;
    }
}