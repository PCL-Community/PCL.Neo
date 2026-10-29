using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using PCL.Neo.UI.Controls.Primitives;

namespace PCL.Neo.UI.Controls;

[PseudoClasses(":loading", ":failed", ":success")]
public class NeoLoading : TemplatedControl
{
    public static readonly StyledProperty<NeoLoadingState> StateProperty =
        AvaloniaProperty.Register<NeoLoading, NeoLoadingState>(
            nameof(State));

    public NeoLoadingState State
    {
        get => GetValue(StateProperty);
        set => SetValue(StateProperty, value);
    }

    public static readonly StyledProperty<string> TextProperty = AvaloniaProperty.Register<NeoLoading, string>(
        nameof(Text));

    public string Text
    {
        get => GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    public NeoLoading()
    {
        StateProperty.Changed.AddClassHandler<NeoLoading>(OnStateChanged);
    }

    private static void OnStateChanged(NeoLoading sender, AvaloniaPropertyChangedEventArgs e)
    {
        switch (e.GetNewValue<NeoLoadingState>())
        {
            case NeoLoadingState.Idle:
                SetPseudoClass(sender, false, false, false);
                break;
            case NeoLoadingState.Loading:
                SetPseudoClass(sender, true, false, false);
                break;
            case NeoLoadingState.Failed:
                SetPseudoClass(sender, false, true, false);
                break;
            case NeoLoadingState.Success:
                SetPseudoClass(sender, false, false, true);
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
    
    private static void SetPseudoClass(NeoLoading control, bool loading, bool failed, bool success)
    {
        control.PseudoClasses.Set(":loading", loading);
        control.PseudoClasses.Set(":failed", failed);
        control.PseudoClasses.Set(":success", success);
    }
}