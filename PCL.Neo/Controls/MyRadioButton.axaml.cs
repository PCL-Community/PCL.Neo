using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Shapes;
using Avalonia.Media;
using PCL.Neo.Helpers;
using PCL.Neo.Models;
using PCL.Neo.Utils;
using System;

namespace PCL.Neo.Controls;

[PseudoClasses(":white", ":highlight")]
public class MyRadioButton : RadioButton
{
    private Path? _shapeLogo;
    private TextBlock? _labText;
    private Border? _panBack;

    private bool _isMouseDown = false;

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        _shapeLogo = e.NameScope.Find<Path>("ShapeLogo")!;
        _labText = e.NameScope.Find<TextBlock>("LabText")!;
        _panBack = e.NameScope.Find<Border>("PanBack")!;

        Loaded += (_, _) => RefreshColor();

        _shapeLogo.Data = Logo;
        _shapeLogo.RenderTransform = new ScaleTransform { ScaleX = LogoScale, ScaleY = LogoScale };
        _labText.Text = Text;

        SetPseudoClass();
    }

    public int Uuid = CoreUtils.GetUuid();

    public static readonly StyledProperty<Geometry> LogoProperty = AvaloniaProperty.Register<MyRadioButton, Geometry>(
        nameof(Logo));

    public Geometry Logo
    {
        get => GetValue(LogoProperty);
        set
        {
            SetValue(LogoProperty, value);
            if (_shapeLogo != null)
            {
                _shapeLogo.Data = value;
            }
        }
    }

    public static readonly StyledProperty<double> LogoScaleProperty = AvaloniaProperty.Register<MyRadioButton, double>(
        nameof(LogoScale),
        1);

    public double LogoScale
    {
        get => GetValue(LogoScaleProperty);
        set
        {
            SetValue(LogoScaleProperty, value);
            if (_shapeLogo != null)
            {
                _shapeLogo.RenderTransform = new ScaleTransform { ScaleX = value, ScaleY = value };
            }
        }
    }

    public static readonly StyledProperty<string> TextProperty = AvaloniaProperty.Register<MyRadioButton, string>(
        nameof(Text),
        string.Empty);

    public string Text
    {
        get => GetValue(TextProperty);
        set
        {
            SetValue(TextProperty, value);
            if (_labText != null)
            {
                _labText.Text = value;
            }
        }
    }

    public enum ColorState
    {
        White,
        HighLight
    }

    public static readonly StyledProperty<ColorState> ColorTypeProperty =
        AvaloniaProperty.Register<MyRadioButton, ColorState>(nameof(ColorType));

    public ColorState ColorType
    {
        get => GetValue(ColorTypeProperty);
        set
        {
            SetValue(ColorTypeProperty, value);
            SetPseudoClass();
        }
    }

    [Obsolete]
    private void SetCheck()
    {
        if (Parent is Panel parent)
        {
            foreach (var child in parent.Children)
            {
                if (child is MyRadioButton radioButton && radioButton != this)
                {
                    radioButton.IsChecked = false;
                }
            }
        }
    }

    private void SetPseudoClass()
    {
        switch (ColorType)
        {
            case ColorState.White:
                PseudoClasses.Set(":white", true);
                break;
            case ColorState.HighLight:
                PseudoClasses.Set(":highlight", true);
                break;
        }
    }

    private void RefreshColor()
    {
        if (_shapeLogo is null || _labText is null) return;
        switch (ColorType)
        {
            case ColorState.White:
                if (IsChecked!.Value)
                {
                    _panBack!.Background = (SolidColorBrush)new MyColor(255, 255, 255);
                    _shapeLogo.Fill = (IBrush?)Application.Current!.Resources["ColorBrush3"];
                    _labText.Foreground = (IBrush?)Application.Current!.Resources["ColorBrush3"];
                }
                else
                {
                    _panBack!.Background = (SolidColorBrush)ThemeHelper.ColorSemiTransparent;
                    _shapeLogo.Fill = (SolidColorBrush)new MyColor(255, 255, 255);
                    _labText.Foreground = (SolidColorBrush)new MyColor(255, 255, 255);
                }

                break;
            case ColorState.HighLight:
                if (IsChecked!.Value)
                {
                    _panBack!.Background = (IBrush?)Application.Current!.Resources["ColorBrush3"];
                    _shapeLogo.Fill = (SolidColorBrush)new MyColor(255, 255, 255);
                    _labText.Foreground = (SolidColorBrush)new MyColor(255, 255, 255);
                }
                else
                {
                    _panBack!.Background = (SolidColorBrush)ThemeHelper.ColorSemiTransparent;
                    _shapeLogo.Fill = (IBrush?)Application.Current!.Resources["ColorBrush3"];
                    _labText.Foreground = (IBrush?)Application.Current!.Resources["ColorBrush3"];
                }

                break;
        }
    }
}
