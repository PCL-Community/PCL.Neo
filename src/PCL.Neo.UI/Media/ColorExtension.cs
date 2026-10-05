using Avalonia.Media;
using PCL.Neo.UI.Media.Primitives;

namespace PCL.Neo.UI.Media;

public static class ColorExtension
{
    extension(Color color)
    {
        public OklchColor ToOklch()
        {
            return ToOklch(color.R, color.G, color.B, color.A);
        }
    }
    
    private static OklchColor ToOklch(byte r, byte g, byte b, byte a)
    {
        var (linearR, linearG, linearB) = ColorUtils.RgbToLinearRgb(r, g, b);
        var (oklabL, oklabA, oklabB) = ColorUtils.LinearRgbToOklab(linearR, linearG, linearB);
        var (oklchL, oklchC, oklchH) = ColorUtils.OklabToOklch(oklabL, oklabA, oklabB);
            
        var oklchA = a / 255.0;
            
        return new OklchColor(oklchL, oklchC, oklchH, oklchA);
    }
}