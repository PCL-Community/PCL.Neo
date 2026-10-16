using Avalonia;
using Avalonia.Controls.Primitives;
using PCL.Neo.UI.Controls.Primitives;

namespace PCL.Neo.UI.Controls;

public sealed class NeoLoading : TemplatedControl
{
    public static readonly StyledProperty<NeoLoadingState> StateProperty =
        AvaloniaProperty.Register<NeoLoading, NeoLoadingState>(
            nameof(State));

    public NeoLoadingState State
    {
        get => GetValue(StateProperty);
        set => SetValue(StateProperty, value);
    }
}