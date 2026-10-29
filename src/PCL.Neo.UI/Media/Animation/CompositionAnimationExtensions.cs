using Avalonia.Rendering.Composition.Animations;

namespace PCL.Neo.UI.Media.Animation;

public static class CompositionAnimationExtensions
{
    extension(CompositionAnimationGroup group)
    {
        public void AddRange(IEnumerable<CompositionAnimation> animations)
        {
            foreach (var animation in animations)
            {
                group.Add(animation);
            }
        }
    }
}