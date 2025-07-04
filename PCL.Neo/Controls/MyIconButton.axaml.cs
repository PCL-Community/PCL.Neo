using Avalonia;
using Avalonia.Animation.Easings;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Shapes;
using Avalonia.Input;
using Avalonia.Media;
using PCL.Neo.Animations;
using PCL.Neo.Helpers;
using PCL.Neo.Models;
using PCL.Neo.Utils;
using System;

namespace PCL.Neo.Controls;

[PseudoClasses(":color", ":white", ":black", ":red", ":custom")]
public class MyIconButton : Button
{
    private Path? _pathIcon;
    private Border? _panBack;
    private readonly AnimationHelper _animation = new();

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        _pathIcon = e.NameScope.Find<Path>("PathIcon")!;
        _panBack = e.NameScope.Find<Border>("PanBack")!;

        Loaded += (_, _) => RefreshColor();

        // 初始化
        _pathIcon.Data = Logo;
        _pathIcon.RenderTransform = new ScaleTransform { ScaleX = LogoScale, ScaleY = LogoScale };

        SetPseudoClass();
    }

    protected override async void OnPointerPressed(PointerPressedEventArgs e)
    {
        base.OnPointerPressed(e);
        if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
        {
            _animation.CancelAndClear();
            _animation.Animations.AddRange(
            [
                new ScaleTransformScaleXAnimation(_panBack!, TimeSpan.FromMilliseconds(400), 0.8d,
                    new QuarticEaseOut()),
                new ScaleTransformScaleYAnimation(_panBack!, TimeSpan.FromMilliseconds(400), 0.8d, new QuarticEaseOut())
            ]);
            await _animation.RunAsync();
        }
    }

    protected override async void OnPointerReleased(PointerReleasedEventArgs e)
    {
        base.OnPointerReleased(e);
        if (e.InitialPressMouseButton == MouseButton.Left)
        {
            _animation.CancelAndClear();
            _animation.Animations.AddRange(
            [
                new ScaleTransformScaleXAnimation(_panBack!, TimeSpan.FromMilliseconds(250), 0.8d, 1d,
                    new BackEaseOut()),
                new ScaleTransformScaleYAnimation(_panBack!, TimeSpan.FromMilliseconds(250), 0.8d, 1d,
                    new BackEaseOut())
            ]);
            await _animation.RunAsync();
        }
    }

    public int Uuid = CoreUtils.GetUuid();

    public static readonly StyledProperty<Geometry> LogoProperty = AvaloniaProperty.Register<MyIconButton, Geometry>(
        nameof(Logo));

    public Geometry Logo
    {
        get => GetValue(LogoProperty);
        set
        {
            SetValue(LogoProperty, value);
            if (_pathIcon != null)
            {
                _pathIcon.Data = value;
            }
        }
    }

    public static readonly StyledProperty<double> LogoScaleProperty = AvaloniaProperty.Register<MyIconButton, double>(
        nameof(LogoScale),
        1);

    public double LogoScale
    {
        get => GetValue(LogoScaleProperty);
        set
        {
            SetValue(LogoScaleProperty, value);
            if (_pathIcon != null)
            {
                _pathIcon.RenderTransform = new ScaleTransform { ScaleX = value, ScaleY = value };
            }
        }
    }

    public enum IconThemes
    {
        Color,
        White,
        Black,
        Red,
        Custom
    }

    public static readonly StyledProperty<IconThemes> IconThemeProperty =
        AvaloniaProperty.Register<MyIconButton, IconThemes>(
            nameof(IconTheme));

    public IconThemes IconTheme
    {
        get => GetValue(IconThemeProperty);
        set
        {
            SetValue(IconThemeProperty, value);
            SetPseudoClass();
        }
    }

    public static new readonly StyledProperty<IBrush> ForegroundProperty =
        AvaloniaProperty.Register<MyIconButton, IBrush>(
            nameof(Foreground));

    public new IBrush Foreground
    {
        get => GetValue(ForegroundProperty);
        set => SetValue(ForegroundProperty, value);
    }

    public static readonly StyledProperty<IBrush> ForegroundInnerProperty =
        AvaloniaProperty.Register<MyIconButton, IBrush>(
            nameof(ForegroundInner));

    public IBrush ForegroundInner
    {
        get => GetValue(ForegroundInnerProperty);
        set => SetValue(ForegroundInnerProperty, value);
    }

    public static new readonly StyledProperty<IBrush> BackgroundProperty =
        AvaloniaProperty.Register<MyIconButton, IBrush>(
            nameof(Background));

    public new IBrush Background
    {
        get => GetValue(BackgroundProperty);
        set => SetValue(BackgroundProperty, value);
    }

    public static readonly StyledProperty<string> EventTypeProperty = AvaloniaProperty.Register<MyIconButton, string>(
        nameof(EventType));

    public string EventType
    {
        get => GetValue(EventTypeProperty);
        set => SetValue(EventTypeProperty, value);
    }

    public static readonly StyledProperty<string> EventDataProperty = AvaloniaProperty.Register<MyIconButton, string>(
        nameof(EventData));

    public string EventData
    {
        get => GetValue(EventDataProperty);
        set => SetValue(EventDataProperty, value);
    }

    /// <summary>
    /// 初始化颜色。
    /// </summary>
    private void RefreshColor()
    {
        if (_pathIcon is null || _panBack is null) return;
        switch (IconTheme)
        {
            case IconThemes.Color:
                _pathIcon.Fill = (SolidColorBrush?)Application.Current!.Resources["ColorBrush5"];
                break;
            case IconThemes.White:
                _pathIcon.Fill = (SolidColorBrush)new MyColor(234, 242, 254);
                break;
            case IconThemes.Red:
                _pathIcon.Fill = (SolidColorBrush)new MyColor(160, 255, 76, 76);
                break;
            case IconThemes.Black:
                _pathIcon.Fill = (SolidColorBrush)new MyColor(160, 0, 0, 0);
                break;
            case IconThemes.Custom:
                _pathIcon.Fill = (SolidColorBrush)new MyColor(160, (SolidColorBrush)Foreground);
                break;
        }

        _panBack.Background = (SolidColorBrush)new MyColor(0, 255, 255, 255);
    }

    private void SetPseudoClass()
    {
        switch (IconTheme)
        {
            case IconThemes.Color:
                PseudoClasses.Set(":color", true);
                break;
            case IconThemes.White:
                PseudoClasses.Set(":white", true);
                break;
            case IconThemes.Black:
                PseudoClasses.Set(":black", true);
                break;
            case IconThemes.Red:
                PseudoClasses.Set(":red", true);
                break;
            case IconThemes.Custom:
                PseudoClasses.Set(":custom", true);
                break;
        }
    }
}
