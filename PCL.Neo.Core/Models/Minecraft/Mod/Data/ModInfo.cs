namespace PCL.Neo.Core.Models.Minecraft.Mod.Data;

public record ModInfo : IDisposable
{
    public string Name { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public string Version { get; set; } = string.Empty;

    public string Icon { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;

    private bool _disposed;

    /// <inheritdoc />
    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        if (File.Exists(Icon))
        {
            File.Delete(Icon);
        }

        _disposed = true;
    }
}
