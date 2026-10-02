using Avalonia.Media;

namespace PCL.Neo.UI.Styles;

public record NeoColorPalette
{
    public Color Accent { get; set; }
    public Color AccentDark1 { get; set; }
    public Color AccentDark2 { get; set; }
    public Color AccentDark3 { get; set; }
    public Color AccentLight1 { get; set; }
    public Color AccentLight2 { get; set; }
    public Color AccentLight3 { get; set; }
    public Color AccentStroke { get; set; }
}