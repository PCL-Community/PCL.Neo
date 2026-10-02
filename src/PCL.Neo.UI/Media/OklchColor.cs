using Avalonia.Media;
using PCL.Neo.UI.Media.Primitives;

namespace PCL.Neo.UI.Media;

public readonly struct OklchColor : IEquatable<OklchColor>
{
    public double L { get; }
    public double C { get; }
    public double H { get; } 
    public double A { get; }

    public OklchColor(double l, double c, double h, double a = 1)
    {
        L = Math.Clamp(l, 0.0, 1.0);
        C = c;
        H = h;
        A = Math.Clamp(a, 0.0, 1.0);
    }
    
    public OklchColor(Color color)
    {
        var oklch = color.ToOklch();
        
        L = oklch.L;
        C = oklch.C;
        H = oklch.H;
        A = oklch.A;
    }

    public Color ToRgb()
    {
        return ToRgb(L, C, H, A);
    }

    public static Color ToRgb(double l, double c, double h, double a = 1)
    {
        var (oklabL, oklabA, oklabB) = ColorUtils.OklchToOklab(l, c, h);
        var (linearR, linearG, linearB) = ColorUtils.OklabToLinearRgb(oklabL, oklabA, oklabB);
        var (r, g, b) = ColorUtils.LinearRgbToRgb(linearR, linearG, linearB);
        
        var rgbA = (byte)Math.Round(a * 255.0);
        
        return Color.FromArgb(rgbA, r, g, b);
    }
    
    public bool Equals(OklchColor other) =>
        // ReSharper disable once CompareOfFloatsByEqualityOperator
        L == other.L &&
        // ReSharper disable once CompareOfFloatsByEqualityOperator
        C == other.C &&
        // ReSharper disable once CompareOfFloatsByEqualityOperator
        H == other.H &&
        // ReSharper disable once CompareOfFloatsByEqualityOperator
        A == other.A;

    public override bool Equals(object? obj)
    {
        if (obj is OklchColor hslColor)
        {
            return Equals(hslColor);
        }

        return false;
    }

    public override int GetHashCode() => HashCode.Combine(L, C, H, A);

    public static bool operator ==(OklchColor left, OklchColor right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(OklchColor left, OklchColor right)
    {
        return !(left == right);
    }
}