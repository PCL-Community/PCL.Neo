using Avalonia;
using Avalonia.Animation;
using Avalonia.Animation.Easings;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Styling;
using PCL.Neo.Animations.Easings;
using System;
using System.Threading.Tasks;

namespace PCL.Neo.Animations;

public static class AnimationExtensions
{
    /// <summary>
    /// 获取指定类型的缓动函数
    /// </summary>
    public static Easing GetEasing(this EasingType easingType)
    {
        return easingType switch
        {
            EasingType.Linear => new LinearEasing(),
            EasingType.QuadraticEaseIn => new QuadraticEaseIn(),
            EasingType.QuadraticEaseOut => new QuadraticEaseOut(),
            EasingType.QuadraticEaseInOut => new QuadraticEaseInOut(),
            EasingType.CubicEaseIn => new CubicEaseIn(),
            EasingType.CubicEaseOut => new CubicEaseOut(),
            EasingType.CubicEaseInOut => new CubicEaseInOut(),
            EasingType.QuarticEaseIn => new QuarticEaseIn(),
            EasingType.QuarticEaseOut => new QuarticEaseOut(),
            EasingType.QuarticEaseInOut => new QuarticEaseInOut(),
            EasingType.QuinticEaseIn => new QuinticEaseIn(),
            EasingType.QuinticEaseOut => new QuinticEaseOut(),
            EasingType.QuinticEaseInOut => new QuinticEaseInOut(),
            EasingType.SineEaseIn => new SineEaseIn(),
            EasingType.SineEaseOut => new SineEaseOut(),
            EasingType.SineEaseInOut => new SineEaseInOut(),
            EasingType.CircularEaseIn => new CircularEaseIn(),
            EasingType.CircularEaseOut => new CircularEaseOut(),
            EasingType.CircularEaseInOut => new CircularEaseInOut(),
            EasingType.ExponentialEaseIn => new ExponentialEaseIn(),
            EasingType.ExponentialEaseOut => new ExponentialEaseOut(),
            EasingType.ExponentialEaseInOut => new ExponentialEaseInOut(),
            EasingType.ElasticEaseIn => new ElasticEaseIn(),
            EasingType.ElasticEaseOut => new ElasticEaseOut(),
            EasingType.ElasticEaseInOut => new ElasticEaseInOut(),
            EasingType.BackEaseIn => new BackEaseIn(),
            EasingType.BackEaseOut => new BackEaseOut(),
            EasingType.BackEaseInOut => new BackEaseInOut(),
            EasingType.BounceEaseIn => new BounceEaseIn(),
            EasingType.BounceEaseOut => new BounceEaseOut(),
            EasingType.BounceEaseInOut => new BounceEaseInOut(),
            _ => new LinearEasing()
        };
    }

    /// <summary>
    /// 从右侧滑入动画
    /// </summary>
    public static async Task SlideInFromRightAsync(this Control control, int duration = 300,
        EasingType easingType = EasingType.CubicEaseOut)
    {
        if (control == null) return;

        // 保存原始状态
        var originalOpacity = control.Opacity;
        var originalTransform = control.RenderTransform;

        // 设置初始状态
        control.Opacity = 0;
        control.RenderTransform = new TranslateTransform(100, 0);

        // 等待UI更新
        await Task.Delay(10);

        // 创建不透明度动画
        var opacityAnimation = new Animation
        {
            Duration = TimeSpan.FromMilliseconds(duration),
            FillMode = FillMode.Forward,
            Easing = easingType.GetEasing()
        };

        // 使用正确的Setter语法
        var opacityFrame1 = new KeyFrame();
        opacityFrame1.Cue = new Cue(0d);
        opacityFrame1.Setters.Add(new Setter(Visual.OpacityProperty, 0d));
        opacityAnimation.Children.Add(opacityFrame1);

        var opacityFrame2 = new KeyFrame();
        opacityFrame2.Cue = new Cue(1d);
        opacityFrame2.Setters.Add(new Setter(Visual.OpacityProperty, originalOpacity));
        opacityAnimation.Children.Add(opacityFrame2);

        // 创建位移动画
        var translateAnimation = new Animation
        {
            Duration = TimeSpan.FromMilliseconds(duration),
            FillMode = FillMode.Forward,
            Easing = easingType.GetEasing()
        };

        var translateFrame1 = new KeyFrame();
        translateFrame1.Cue = new Cue(0d);
        translateFrame1.Setters.Add(new Setter(Visual.RenderTransformProperty, new TranslateTransform(100, 0)));
        translateAnimation.Children.Add(translateFrame1);

        var translateFrame2 = new KeyFrame();
        translateFrame2.Cue = new Cue(1d);
        translateFrame2.Setters.Add(new Setter(Visual.RenderTransformProperty,
            originalTransform ?? new TranslateTransform(0, 0)));
        translateAnimation.Children.Add(translateFrame2);

        // 执行动画
        await opacityAnimation.RunAsync(control);
        await translateAnimation.RunAsync(control);
    }

    /// <summary>
    /// 从左侧滑入动画
    /// </summary>
    public static async Task SlideInFromLeftAsync(this Control control, int duration = 300,
        EasingType easingType = EasingType.CubicEaseOut)
    {
        if (control == null) return;

        // 保存原始状态
        var originalOpacity = control.Opacity;
        var originalTransform = control.RenderTransform;

        // 设置初始状态
        control.Opacity = 0;
        control.RenderTransform = new TranslateTransform(-100, 0);

        // 等待UI更新
        await Task.Delay(10);

        // 创建不透明度动画
        var opacityAnimation = new Animation
        {
            Duration = TimeSpan.FromMilliseconds(duration),
            FillMode = FillMode.Forward,
            Easing = easingType.GetEasing()
        };

        var opacityFrame1 = new KeyFrame();
        opacityFrame1.Cue = new Cue(0d);
        opacityFrame1.Setters.Add(new Setter(Visual.OpacityProperty, 0d));
        opacityAnimation.Children.Add(opacityFrame1);

        var opacityFrame2 = new KeyFrame();
        opacityFrame2.Cue = new Cue(1d);
        opacityFrame2.Setters.Add(new Setter(Visual.OpacityProperty, originalOpacity));
        opacityAnimation.Children.Add(opacityFrame2);

        // 创建位移动画
        var translateAnimation = new Animation
        {
            Duration = TimeSpan.FromMilliseconds(duration),
            FillMode = FillMode.Forward,
            Easing = easingType.GetEasing()
        };

        var translateFrame1 = new KeyFrame();
        translateFrame1.Cue = new Cue(0d);
        translateFrame1.Setters.Add(new Setter(Visual.RenderTransformProperty, new TranslateTransform(-100, 0)));
        translateAnimation.Children.Add(translateFrame1);

        var translateFrame2 = new KeyFrame();
        translateFrame2.Cue = new Cue(1d);
        translateFrame2.Setters.Add(new Setter(Visual.RenderTransformProperty,
            originalTransform ?? new TranslateTransform(0, 0)));
        translateAnimation.Children.Add(translateFrame2);

        // 执行动画
        await opacityAnimation.RunAsync(control);
        await translateAnimation.RunAsync(control);
    }

    /// <summary>
    /// 淡入动画
    /// </summary>
    public static async Task FadeInAsync(this Control control, int duration = 300,
        EasingType easingType = EasingType.CubicEaseOut)
    {
        if (control == null) return;

        // 保存原始状态
        var originalOpacity = control.Opacity;

        // 设置初始状态
        control.Opacity = 0;

        // 等待UI更新
        await Task.Delay(10);

        // 创建不透明度动画
        var opacityAnimation = new Animation
        {
            Duration = TimeSpan.FromMilliseconds(duration),
            FillMode = FillMode.Forward,
            Easing = easingType.GetEasing()
        };

        var opacityFrame1 = new KeyFrame();
        opacityFrame1.Cue = new Cue(0d);
        opacityFrame1.Setters.Add(new Setter(Visual.OpacityProperty, 0d));
        opacityAnimation.Children.Add(opacityFrame1);

        var opacityFrame2 = new KeyFrame();
        opacityFrame2.Cue = new Cue(1d);
        opacityFrame2.Setters.Add(new Setter(Visual.OpacityProperty, originalOpacity));
        opacityAnimation.Children.Add(opacityFrame2);

        // 执行动画
        await opacityAnimation.RunAsync(control);
    }

    /// <summary>
    /// 淡出动画
    /// </summary>
    public static async Task FadeOutAsync(this Control control, int duration = 300,
        EasingType easingType = EasingType.CubicEaseOut)
    {
        if (control == null) return;

        // 保存原始状态
        var originalOpacity = control.Opacity;

        // 等待UI更新
        await Task.Delay(10);

        // 创建不透明度动画
        var opacityAnimation = new Animation
        {
            Duration = TimeSpan.FromMilliseconds(duration),
            FillMode = FillMode.Forward,
            Easing = easingType.GetEasing()
        };

        var opacityFrame1 = new KeyFrame();
        opacityFrame1.Cue = new Cue(0d);
        opacityFrame1.Setters.Add(new Setter(Visual.OpacityProperty, originalOpacity));
        opacityAnimation.Children.Add(opacityFrame1);

        var opacityFrame2 = new KeyFrame();
        opacityFrame2.Cue = new Cue(1d);
        opacityFrame2.Setters.Add(new Setter(Visual.OpacityProperty, 0d));
        opacityAnimation.Children.Add(opacityFrame2);

        // 执行动画
        await opacityAnimation.RunAsync(control);

        // 设置最终状态
        control.Opacity = 0;
    }
}
