namespace PCL.Neo.Core.Models.Profile
{
    /// <summary>
    /// 资源包信息
    /// </summary>
    public record ResourcePackInfo
    {
        public required string Id          { get; init; }
        public required string Name        { get; set; }
        public          string Version     { get; set; } = string.Empty;
        public          string Format      { get; set; } = string.Empty;
        public          string FilePath    { get; set; } = string.Empty;
        public          string Description { get; set; } = string.Empty;
    }
}