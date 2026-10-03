using Avalonia.Markup.Xaml;
using Avalonia.Media;
using PCL.Neo.UI.Media;
using PCL.Neo.UI.Styles;

namespace PCL.Neo.UI;

public class NeoTheme : Avalonia.Styling.Styles
{
    public event EventHandler<RefreshedEventArgs>? Refreshed;
    
    public static NeoTheme? Current { get; private set; }

    public Color AccentColor
    {
        get => _accentColor;
        set
        {
            if (_accentColor == value)
                return;

            _accentColor = value;

            Refresh(true);
        }
    }
    private Color _accentColor = Color.Parse("#fa9545");

    public NeoColorPalette ColorPalette
    {
        get => _colorPalette;
        set
        {
            if (_colorPalette == value)
                return;

            _colorPalette = value;
            
            Refresh(false);
        }
    }

    private NeoColorPalette _colorPalette = null!;

    public NeoTheme(IServiceProvider? sp = null)
    {
        AvaloniaXamlLoader.Load(sp, this);
        Refresh(true);
        Current = this;
    }

    private void Refresh(bool accentChanged)
    {
        if (accentChanged)
        {
            _colorPalette = ColorHelper.GenerateColorPalette(AccentColor);
        }
        else
        {
            _accentColor = ColorPalette.Accent;
        }
        
        Refreshed?.Invoke(this, new RefreshedEventArgs(ColorPalette));
    }
}