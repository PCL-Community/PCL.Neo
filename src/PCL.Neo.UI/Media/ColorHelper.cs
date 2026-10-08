using Avalonia;
using Avalonia.Media;
using Avalonia.Styling;
using PCL.Neo.UI.Media.Primitives;
using PCL.Neo.UI.Styles;

namespace PCL.Neo.UI.Media;

public static class ColorHelper
{
    public static NeoColorPalette GenerateColorPalette(Color color)
    {
        var oklch = color.ToOklch();

        var currentTheme = Application.Current?.ActualThemeVariant;

        if (currentTheme == ThemeVariant.Light)
        {
            return new NeoColorPalette
            {
                Accent = color,
                AccentLight1 = Lighten(color, Lerp(0, 0.995 - oklch.L, 0.2)),
                AccentLight2 = Lighten(color, Lerp(0, 0.995 - oklch.L, 0.4)),
                AccentLight3 = Lighten(color, Lerp(0, 0.995 - oklch.L, 0.6)),
                AccentLight4 = Lighten(color, Lerp(0, 0.995 - oklch.L, 0.8)),
                AccentLight5 = Lighten(color, Lerp(0, 0.995 - oklch.L, 0.9)),
                AccentLight6 = Lighten(color, Lerp(0, 0.995 - oklch.L, 0.95)),
                AccentDark1 = Darken(color, Lerp(0, oklch.L - 0.15, 0.2)),
                AccentDark2 = Darken(color, Lerp(0, oklch.L - 0.15, 0.4)),
                AccentDark3 = Darken(color, Lerp(0, oklch.L - 0.15, 0.6)),
                AccentStroke = GenerateStroke(false),
                AccentPressed = GeneratePressed(false)
            };
        }

        if (currentTheme == ThemeVariant.Dark)
        {
            return new NeoColorPalette
            {
                Accent = color,
                AccentLight1 = Darken(color, Lerp(0, oklch.L - 0.1, 0.2)),
                AccentLight2 = Darken(color, Lerp(0, oklch.L - 0.1, 0.4)),
                AccentLight3 = Darken(color, Lerp(0, oklch.L - 0.1, 0.6)),
                AccentLight4 = Darken(color, Lerp(0, oklch.L - 0.1, 0.7)),
                AccentLight5 = Darken(color, Lerp(0, oklch.L - 0.1, 0.8)),
                AccentLight6 = Darken(color, Lerp(0, oklch.L - 0.1, 0.9)),
                AccentDark1 = Lighten(color, Lerp(0, 0.95 - oklch.L, 0.3)),
                AccentDark2 = Lighten(color, Lerp(0, 0.95 - oklch.L, 0.6)),
                AccentDark3 = Lighten(color, Lerp(0, 0.95 - oklch.L, 0.9)),
                AccentStroke = GenerateStroke(true),
                AccentPressed = GeneratePressed(true)
            };
        }

        throw new NotSupportedException($"Theme variant {currentTheme} is not supported.");

        double Lerp(double a, double b, double t) => a + (b - a) * t;

        Color GenerateStroke(bool isDark)
        {
            var strokeL = Math.Clamp(oklch.L - 0.12, 0.30, 0.40);
            var strokeC = Math.Min(oklch.C * (isDark ? 0.10 : 0.20), isDark ? 0.08 : 0.10);

            return GamutMappingToSrgb(new OklchColor(strokeL, strokeC, oklch.H, oklch.A)).ToRgb();
        }

        Color GeneratePressed(bool isDark)
        {
            var c = isDark
                ? Darken(color, Lerp(0, oklch.L - 0.1, 0.7))
                : Lighten(color, Lerp(0, 0.9 - oklch.L, 0.7));
            return new Color(190, c.R, c.G, c.B);
        }
    }

    /// <summary>
    /// 变亮颜色。
    /// </summary>
    /// <param name="color">要变亮的颜色。</param>
    /// <param name="amount">变亮的程度，范围应在 0.0 到 1.0 之间。</param>
    /// <returns>变亮后的颜色。</returns>
    public static Color Lighten(Color color, double amount)
    {
        if (amount == 0.0) return color;
        amount = Math.Clamp(amount, 0.0, 1.0);

        return LightenCore(color, amount);
    }

    /// <summary>
    /// 变暗颜色。
    /// </summary>
    /// <param name="color">要变暗的颜色。</param>
    /// <param name="amount">变暗的程度，范围应在 0.0 到 1.0 之间。</param>
    /// <returns>变暗后的颜色。</returns>
    public static Color Darken(Color color, double amount)
    {
        if (amount == 0.0) return color;
        amount = Math.Clamp(amount, 0.0, 1.0);

        return LightenCore(color, -amount);
    }

    private static Color LightenCore(Color color, double amount)
    {
        // 转换为 OKLCH 空间
        var oklch = color.ToOklch();
        // 线性相加
        var newL = Math.Clamp(oklch.L + amount, 0.0, 1.0);

        return GamutMappingToSrgb(new OklchColor(newL, oklch.C, oklch.H, oklch.A)).ToRgb();
    }

    private static OklchColor GamutMappingToSrgb(OklchColor oklchColor)
    {
        var lowC = 0.0;
        var midC = 0.0;
        var highC = oklchColor.C;
        for (var i = 0; i < 20; i++)
        {
            midC = (lowC + highC) / 2.0;

            var (linearR, linearG, linearB) = OklchToLinearRgb(oklchColor.L, midC, oklchColor.H);
            if (IsInSrgb(linearR, linearG, linearB))
            {
                lowC = midC;
            }
            else
            {
                highC = midC;
            }
        }

        return new OklchColor(oklchColor.L, midC, oklchColor.H, oklchColor.A);

        (double r, double g, double b) OklchToLinearRgb(double l, double c, double h)
        {
            var (oklabL, oklabA, oklabH) = ColorUtils.OklchToOklab(l, c, h);
            return ColorUtils.OklabToLinearRgb(oklabL, oklabA, oklabH);
        }

        bool IsInSrgb(double r, double g, double b) =>
            r is >= 0 and <= 1 &&
            g is >= 0 and <= 1 &&
            b is >= 0 and <= 1;
    }
}