using Avalonia;
using Avalonia.Animation.Easings;
using Avalonia.Controls;
using Avalonia.Controls.Documents;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Metadata;
using PCL.Neo.Animations;
using PCL.Neo.Helpers;
using PCL.Neo.Utils;
using System;

namespace PCL.Neo.Controls;

[PseudoClasses(":normal", ":highlight", ":red")]
public class MyButton : Button
{
    private Border? _panFore;
    private AnimationHelper _animation;

    public MyButton()
    {
        _animation = new AnimationHelper();
        Inlines = new InlineCollection();
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        _panFore = e.NameScope.Find<Border>("PanFore")!;

        if (Inlines!.Count == 0 && string.IsNullOrEmpty(Text))
        {
            Text = "Button";
        }

        SetPseudoClasses();
    }

    protected override async void OnPointerPressed(PointerPressedEventArgs e)
    {
        base.OnPointerPressed(e);

        if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
        {
            _animation.CancelAndClear();
            _animation.Animations.AddRange([
                new ScaleTransformScaleXAnimation(this, TimeSpan.FromMilliseconds(80), 0.955, new CubicEaseOut()),
                new ScaleTransformScaleYAnimation(this, TimeSpan.FromMilliseconds(80), 0.955, new CubicEaseOut())
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
            _animation.Animations.AddRange([
                new ScaleTransformScaleXAnimation(this, TimeSpan.FromMilliseconds(300), 0.955, 1, new QuinticEaseOut()),
                new ScaleTransformScaleYAnimation(this, TimeSpan.FromMilliseconds(300), 0.955, 1, new QuinticEaseOut())
            ]);
            await _animation.RunAsync();
        }
    }

    public int Uuid = CoreUtils.GetUuid();

    public static readonly StyledProperty<InlineCollection?> InlinesProperty =
        AvaloniaProperty.Register<MyButton, InlineCollection?>(
            nameof(Inlines), new InlineCollection());

    [Content]
    public InlineCollection? Inlines
    {
        get => GetValue(InlinesProperty);
        set => SetValue(InlinesProperty, value);
    }

    public static readonly StyledProperty<string> TextProperty = AvaloniaProperty.Register<MyButton, string>(
        nameof(Text));

    public string Text
    {
        get => GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    public static readonly StyledProperty<Thickness> TextPaddingProperty =
        AvaloniaProperty.Register<MyButton, Thickness>(
            nameof(TextPadding));

    public Thickness TextPadding
    {
        get => GetValue(TextPaddingProperty);
        set => SetValue(TextPaddingProperty, value);
    }

    public enum ColorState
    {
        Normal,
        Highlight,
        Red
    }

    public static readonly StyledProperty<ColorState> ColorTypeProperty =
        AvaloniaProperty.Register<MyButton, ColorState>(
            nameof(ColorType));

    public ColorState ColorType
    {
        get => GetValue(ColorTypeProperty);
        set => SetValue(ColorTypeProperty, value);
    }

    public static readonly StyledProperty<Transform> RealRenderTransformProperty =
        AvaloniaProperty.Register<MyButton, Transform>(
            nameof(RealRenderTransform));

    public Transform RealRenderTransform
    {
        get => GetValue(RealRenderTransformProperty);
        set => SetValue(RealRenderTransformProperty, value);
    }

    public static readonly StyledProperty<string> EventTypeProperty = AvaloniaProperty.Register<MyButton, string>(
        nameof(EventType));

    public string EventType
    {
        get => GetValue(EventTypeProperty);
        set => SetValue(EventTypeProperty, value);
    }

    public static readonly StyledProperty<string> EventDataProperty = AvaloniaProperty.Register<MyButton, string>(
        nameof(EventData));

    public string EventData
    {
        get => GetValue(EventDataProperty);
        set => SetValue(EventDataProperty, value);
    }

    [Obsolete]
    private void RefreshColor()
    {
        if (_panFore is null) return;
        if (IsEnabled)
        {
            switch (ColorType)
            {
                case ColorState.Normal:
                    _panFore.BorderBrush = (IBrush?)Application.Current!.Resources["ColorBrush1"];
                    break;
                case ColorState.Highlight:
                    _panFore.BorderBrush = (IBrush?)Application.Current!.Resources["ColorBrush2"];
                    break;
                case ColorState.Red:
                    _panFore.BorderBrush = (IBrush?)Application.Current!.Resources["ColorBrushRedDark"];
                    break;
            }
        }
        else
        {
            _panFore.BorderBrush = (SolidColorBrush)ThemeHelper.ColorGray4;
        }

        _panFore.Background = (IBrush?)Application.Current!.Resources["ColorBrushHalfWhite"];
    }

    private void SetPseudoClasses()
    {
        switch (ColorType)
        {
            case ColorState.Normal:
                PseudoClasses.Set(":normal", true);
                break;
            case ColorState.Highlight:
                PseudoClasses.Set(":highlight", true);
                break;
            case ColorState.Red:
                PseudoClasses.Set(":red", true);
                break;
        }
    }
}
