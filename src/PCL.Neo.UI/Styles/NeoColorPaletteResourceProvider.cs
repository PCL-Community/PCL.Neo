using Avalonia.Controls;
using Avalonia.Styling;

namespace PCL.Neo.UI.Styles;

public class NeoColorPaletteResourceProvider : ResourceProvider
{
    public const string AccentKey = "NeoSystemAccentColor";
    public const string AccentDark1Key = "NeoSystemAccentColorDark1";
    public const string AccentDark2Key = "NeoSystemAccentColorDark2";
    public const string AccentDark3Key = "NeoSystemAccentColorDark3";
    public const string AccentLight1Key = "NeoSystemAccentColorLight1";
    public const string AccentLight2Key = "NeoSystemAccentColorLight2";
    public const string AccentLight3Key = "NeoSystemAccentColorLight3";
    public const string AccentLight4Key = "NeoSystemAccentColorLight4";
    public const string AccentLight5Key = "NeoSystemAccentColorLight5";
    public const string AccentLight6Key = "NeoSystemAccentColorLight6";
    public const string AccentStrokeKey = "NeoSystemAccentColorStroke";
    public const string AccentPressedKey = "NeoSystemAccentColorPressed";
    
    private NeoColorPalette _palette = null!;
    private NeoTheme? _theme;
    
    public override bool HasResources => true;
    
    public override bool TryGetResource(object key, ThemeVariant? theme, out object? value)
    {
        if (key is string strKey)
        {
            value = strKey switch
            {
                AccentKey => _palette.Accent,
                AccentDark1Key => _palette.AccentDark1,
                AccentDark2Key => _palette.AccentDark2,
                AccentDark3Key => _palette.AccentDark3,
                AccentLight1Key => _palette.AccentLight1,
                AccentLight2Key => _palette.AccentLight2,
                AccentLight3Key => _palette.AccentLight3,
                AccentLight4Key => _palette.AccentLight4,
                AccentLight5Key => _palette.AccentLight5,
                AccentLight6Key => _palette.AccentLight6,
                AccentStrokeKey => _palette.AccentStroke,
                AccentPressedKey => _palette.AccentPressed,
                _ => null
            };
        }
        else
        {
            value = null;
        }

        return value != null;
    }
    
    protected override void OnAddOwner(IResourceHost owner)
    {
        base.OnAddOwner(owner);

        _theme = NeoTheme.Current ?? throw new InvalidOperationException("NeoTheme.Current is null");
        _theme.Refreshed += CurrentOnRefreshed;
        _palette = _theme.ColorPalette;
    }

    protected override void OnRemoveOwner(IResourceHost owner)
    {
        if (_theme is not null)
        {
            _theme.Refreshed -= CurrentOnRefreshed;
            _theme = null;
        }
        
        base.OnRemoveOwner(owner);
    }
    
    private void CurrentOnRefreshed(object? sender, RefreshedEventArgs e)
    {
        _palette = e.Palette;
        Owner?.NotifyHostedResourcesChanged(ResourcesChangedEventArgs.Create());
    }
}