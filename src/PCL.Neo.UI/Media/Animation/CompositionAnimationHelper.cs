using Avalonia.Rendering.Composition.Animations;

namespace PCL.Neo.UI.Media.Animation;

public static class CompositionAnimationHelper
{
    public static Task StartAnimationAsync(CompositionAnimationTarget target, string propertyName,
        CompositionAnimation animation, CancellationToken cancellationToken = default)
    {
        if (cancellationToken.IsCancellationRequested)
        {
            return Task.CompletedTask;
        }

        if (animation is not KeyFrameAnimation keyFrameAnimation)
        {
            throw new InvalidOperationException("CompositionAnimationHelper 只支持 KeyFrameAnimation。");
        }

        var totalDelay = keyFrameAnimation.Duration + keyFrameAnimation.DelayTime;
        target.Initialize();
        var visual = target.Visual!;
            
        using var registration = cancellationToken.Register(() =>
        {
            visual.StopAnimation(propertyName);
        });
            
        visual.StartAnimation(propertyName, animation);
            
        return Task.Delay(totalDelay, cancellationToken);
    }
    
    public static Task StartAnimationGroupAsync(CompositionAnimationTarget target, NeoCompositionAnimationGroup group,
        CancellationToken cancellationToken = default)
    {
        if (cancellationToken.IsCancellationRequested)
        {
            return Task.CompletedTask;
        }

        var delay = TimeSpan.Zero;
        foreach (var ani in group)
        {
            if (ani is KeyFrameAnimation keyFrameAnimation)
            {
                var totalDelay = keyFrameAnimation.Duration + keyFrameAnimation.DelayTime;
                delay = delay >= totalDelay ? delay : totalDelay;
            }
            else
            {
                throw new InvalidOperationException("CompositionAnimationHelper 只支持 KeyFrameAnimation。");
            }
        }
        target.Initialize();
        var visual = target.Visual!;
        var animationGroup = group.Build(target.Compositor!);
        
        using var registration = cancellationToken.Register(() =>
        {
            visual.StopAnimationGroup(animationGroup);
        });
        
        visual.StartAnimationGroup(animationGroup);
        
        return Task.Delay(delay, cancellationToken);
    }
}