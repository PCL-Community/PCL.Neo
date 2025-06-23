using System;
using Avalonia;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Avalonia.Styling;
using PCL.Neo.Models;
using PCL.Neo.Views;

namespace PCL.Neo.Helpers;

public class ThemeHelper
{
    private readonly MainWindow _mainWindow;

    public static MyColor Color1 { get; set; } = new(52, 61, 74);
    public static MyColor Color2 { get; set; } = new(11, 91, 203);
    public static MyColor Color3 { get; set; } = new(19, 112, 243);
    public static MyColor Color4 { get; set; } = new(72, 144, 245);
    public static MyColor Color5 { get; set; } = new(150, 192, 249);
    public static MyColor Color6 { get; set; } = new(213, 230, 253);
    public static MyColor Color7 { get; set; } = new(222, 236, 253);
    public static MyColor Color8 { get; set; } = new(234, 242, 254);
    public static MyColor ColorBg0 { get; set; } = new(150, 192, 249);
    public static MyColor ColorBg1 { get; set; } = new(190, Color7);
    public static MyColor ColorGray1 { get; set; } = new(64, 64, 64);
    public static MyColor ColorGray2 { get; set; } = new(115, 115, 115);
    public static MyColor ColorGray3 { get; set; } = new(140, 140, 140);
    public static MyColor ColorGray4 { get; set; } = new(166, 166, 166);
    public static MyColor ColorGray5 { get; set; } = new(204, 204, 204);
    public static MyColor ColorGray6 { get; set; } = new(235, 235, 235);
    public static MyColor ColorGray7 { get; set; } = new(240, 240, 240);
    public static MyColor ColorGray8 { get; set; } = new(245, 245, 245);
    public static MyColor ColorSemiTransparent { get; set; } = new(1, Color8);

    private int _colorHue = 50, _colorSat = 65, _colorLightAdjust = 10, _colorHueTopbarDelta = 0;

    public ThemeHelper(MainWindow mainWindow)
    {
        _mainWindow = mainWindow;

        Application.Current!.ActualThemeVariantChanged += (sender, _) =>
        {
            var themeVariant = ((IThemeVariantHost)sender!).ActualThemeVariant;
            Refresh(themeVariant);
        };
    }
    public void Refresh(ThemeVariant themeVariant)
    {
        // 主题色
        Color1 = MyColor.FromHsl2(_colorHue, _colorSat * 0.2, 25 + _colorLightAdjust * 0.3);
        Color2 = MyColor.FromHsl2(_colorHue, _colorSat, 45 + _colorLightAdjust);
        Color3 = MyColor.FromHsl2(_colorHue, _colorSat, 55 + _colorLightAdjust);
        Color4 = MyColor.FromHsl2(_colorHue, _colorSat, 65 + _colorLightAdjust);
        Color5 = MyColor.FromHsl2(_colorHue, _colorSat, 80 + _colorLightAdjust * 0.4);
        Color6 = MyColor.FromHsl2(_colorHue, _colorSat, 91 + _colorLightAdjust * 0.1);
        Color7 = MyColor.FromHsl2(_colorHue, _colorSat, 95);
        Color7 = MyColor.FromHsl2(_colorHue, _colorSat, 97);
        ColorBg0 = Color4 * 0.4 + Color5 * 0.4 + ColorGray4 * 0.2;
        ColorBg1 = new MyColor(190, Color7);
        ColorSemiTransparent = new MyColor(1, Color8);

        if (Application.Current is not null)
        {
            Application.Current.Resources["ColorBrush1"] = new SolidColorBrush(Color1);
            Application.Current.Resources["ColorBrush2"] = new SolidColorBrush(Color2);
            Application.Current.Resources["ColorBrush3"] = new SolidColorBrush(Color3);
            Application.Current.Resources["ColorBrush4"] = new SolidColorBrush(Color4);
            Application.Current.Resources["ColorBrush5"] = new SolidColorBrush(Color5);
            Application.Current.Resources["ColorBrush6"] = new SolidColorBrush(Color6);
            Application.Current.Resources["ColorBrush7"] = new SolidColorBrush(Color7);
            Application.Current.Resources["ColorBrush8"] = new SolidColorBrush(Color8);
            Application.Current.Resources["ColorBrushBg0"] = new SolidColorBrush(ColorBg0);
            Application.Current.Resources["ColorBrushBg1"] = new SolidColorBrush(ColorBg1);

            Application.Current.Resources["ColorObject1"] = (Color)Color1;
            Application.Current.Resources["ColorObject2"] = (Color)Color2;
            Application.Current.Resources["ColorObject3"] = (Color)Color3;
            Application.Current.Resources["ColorObject4"] = (Color)Color4;
            Application.Current.Resources["ColorObject5"] = (Color)Color5;
            Application.Current.Resources["ColorObject6"] = (Color)Color6;
            Application.Current.Resources["ColorObject7"] = (Color)Color7;
            Application.Current.Resources["ColorObject8"] = (Color)Color8;
            Application.Current.Resources["ColorObjectBg0"] = (Color)ColorBg0;
            Application.Current.Resources["ColorObjectBg1"] = (Color)ColorBg1;
        }

        // 标题栏
        var brushTitle = new LinearGradientBrush
        {
            EndPoint = new RelativePoint(1, 0, RelativeUnit.Relative),
            StartPoint = new RelativePoint(0, 0, RelativeUnit.Relative)
        };

        brushTitle.GradientStops.Add(new GradientStop
        {
            Offset = 0,
            Color = MyColor.FromHsl2(_colorHue - _colorHueTopbarDelta, _colorSat, 48 + _colorLightAdjust)
        });
        brushTitle.GradientStops.Add(new GradientStop
        {
            Offset = 0.5,
            Color = MyColor.FromHsl2(_colorHue, _colorSat, 54 + _colorLightAdjust)
        });
        brushTitle.GradientStops.Add(new GradientStop
        {
            Offset = 1,
            Color = MyColor.FromHsl2(_colorHue + _colorHueTopbarDelta, _colorSat, 48 + _colorLightAdjust)
        });

        _mainWindow.NavBackgroundBorder.Background = brushTitle;

        _mainWindow.ImgTitle.Source = new Bitmap(AssetLoader.Open(new Uri("avares://PCL.Neo/Assets/Themes/8.png")));

        double lightAdjust = 1;
        if (themeVariant == ThemeVariant.Light)
        {
            lightAdjust = 1;
        }
        else if (themeVariant == ThemeVariant.Dark)
        {
            lightAdjust = 0.1;
        }

        // 背景
        var brushBackground = new LinearGradientBrush
        {
            EndPoint = new RelativePoint(0.1, 1, RelativeUnit.Relative),
            StartPoint = new RelativePoint(0.9, 0, RelativeUnit.Relative)
        };

        brushBackground.GradientStops.Add(new GradientStop
        {
            Offset = -0.1,
            Color = MyColor.FromHsl2(_colorHue - 20, Math.Min(60, _colorSat) * 0.5, 80 * lightAdjust)
        });
        brushBackground.GradientStops.Add(new GradientStop
        {
            Offset = 0.4,
            Color = MyColor.FromHsl2(_colorHue, _colorSat * 0.9, 90 * lightAdjust)
        });
        brushBackground.GradientStops.Add(new GradientStop
        {
            Offset = 1.1,
            Color = MyColor.FromHsl2(_colorHue + 20, Math.Min(60, _colorSat) * 0.5, 80 * lightAdjust)
        });

        _mainWindow.MainBorder.Background = brushBackground;
    }
}