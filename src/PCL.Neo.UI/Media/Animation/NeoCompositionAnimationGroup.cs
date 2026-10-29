using Avalonia.Rendering.Composition;
using Avalonia.Rendering.Composition.Animations;

namespace PCL.Neo.UI.Media.Animation;

public sealed class NeoCompositionAnimationGroup : List<CompositionAnimation>
{
    public CompositionAnimationGroup Build(Compositor compositor)
    {
        var group = compositor.CreateAnimationGroup();
        group.AddRange(this);
        return group;
    }
}