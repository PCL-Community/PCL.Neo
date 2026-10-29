using Avalonia.Controls;
using Avalonia.Rendering.Composition;

namespace PCL.Neo.UI.Media.Animation;

public sealed record CompositionAnimationTarget
{
    public required Control Control { get; init; }

    public CompositionVisual? Visual { get; private set; }

    public Compositor? Compositor => Visual?.Compositor;
    
    public void Initialize()
    {
        Visual = ElementComposition.GetElementVisual(Control);
    }
}