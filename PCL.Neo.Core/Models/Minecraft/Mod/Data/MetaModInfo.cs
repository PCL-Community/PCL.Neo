namespace PCL.Neo.Core.Models.Minecraft.Mod.Data;

internal class MetaModInfo
{
    public record ModInfo
    {
        /// <summary>
        /// 模组ID，映射到 "modId"。
        /// </summary>
        public string ModId { get; set; } = string.Empty;

        /// <summary>
        /// 模组版本，映射到 "version"。
        /// </summary>
        public string Version { get; set; } = string.Empty;

        /// <summary>
        /// 显示名称，映射到 "displayName"。
        /// </summary>
        public string DisplayName { get; set; } = string.Empty;

        /// <summary>
        /// 主页URL，映射到 "displayURL"。
        /// </summary>
        public string DisplayUrl { get; set; } = string.Empty;

        /// <summary>
        /// Logo文件名，映射到 "logoFile"。
        /// </summary>
        public string LogoFile { get; set; } = string.Empty;

        /// <summary>
        /// 制作人员/致谢名单，映射到 "credits"。
        /// </summary>
        public string Credits { get; set; } = string.Empty;

        /// <summary>
        /// 模组描述，映射到 "description"。
        /// </summary>
        public string Description { get; set; } = string.Empty;
    }

    /// <summary>
    /// 映射 TOML 中的 [[mods]] 数组。
    /// Tomlyn 会自动将 TOML 中的 "mods" 键映射到这个名为 "Mods" 的属性。
    /// </summary>
    public List<ModInfo> Mods { get; set; } = [];
}
