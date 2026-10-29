using Avalonia;
using Avalonia.Animation.Easings;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Media;
using Avalonia.Rendering.Composition.Animations;
using PCL.Neo.UI.Controls;
using PCL.Neo.UI.Controls.Primitives;
using PCL.Neo.UI.Media.Animation;
using PCL.Neo.UI.Media.Animation.Easings;
using Path = Avalonia.Controls.Shapes.Path;

namespace PCL.Neo.UI.Styles.Controls.Primitives;

public sealed class NeoLoadingAnimationHandler
{
    public static readonly AttachedProperty<bool> AnimationProperty =
        AvaloniaProperty.RegisterAttached<NeoLoadingAnimationHandler, NeoLoading, bool>("Animation");

    public static void SetAnimation(NeoLoading obj, bool value) => obj.SetValue(AnimationProperty, value);
    public static bool GetAnimation(NeoLoading obj) => obj.GetValue(AnimationProperty);

    public static readonly AttachedProperty<NeoLoadingAnimationContext?> AnimationContextProperty =
        AvaloniaProperty.RegisterAttached<NeoLoadingAnimationHandler, NeoLoading, NeoLoadingAnimationContext?>("AnimationContext");

    public static void SetAnimationContext(NeoLoading obj, NeoLoadingAnimationContext? value) => obj.SetValue(AnimationContextProperty, value);
    public static NeoLoadingAnimationContext? GetAnimationContext(NeoLoading obj) => obj.GetValue(AnimationContextProperty);

    static NeoLoadingAnimationHandler()
    {
        AnimationProperty.Changed.AddClassHandler<NeoLoading>(OnAnimationChanged);
    }

    private static void OnAnimationChanged(NeoLoading sender, AvaloniaPropertyChangedEventArgs e)
    {
        if (e.NewValue is true)
        {
            sender.DetachedFromVisualTree += OnDetachedFromVisualTree;
            sender.PropertyChanged += OnPropertyChanged;
            sender.TemplateApplied += OnTemplateApplied;
        }
        else
        {
            sender.DetachedFromVisualTree -= OnDetachedFromVisualTree;
            sender.PropertyChanged -= OnPropertyChanged;
            sender.TemplateApplied -= OnTemplateApplied;
            
            StopAnimation(sender);
            Reset(sender);
        }
    }

    private static void OnTemplateApplied(object? sender, TemplateAppliedEventArgs e)
    {
        var control = (NeoLoading)sender!;
        
        var context = control.GetValue(AnimationContextProperty);
        if (context is null)
        {
            context = new NeoLoadingAnimationContext
            {
                PickaxePathTarget = new CompositionAnimationTarget
                {
                    Control = e.NameScope.Find<Path>("PickaxePath")!
                },
                LeftPiecePathTarget = new CompositionAnimationTarget
                {
                    Control = e.NameScope.Find<Path>("LeftPiecePath")!
                },
                RightPiecePathTarget = new CompositionAnimationTarget
                {
                    Control = e.NameScope.Find<Path>("RightPiecePath")!
                },
                IconPathTarget = new CompositionAnimationTarget
                {
                    Control = e.NameScope.Find<Path>("IconPath")!
                }
            };
                
            control.SetValue(AnimationContextProperty, context);
        }
        
        context.Initialize();
        
        Reset(control);
        OnStateChanged(control, NeoLoadingState.Idle, control.State);
    }

    private static void OnDetachedFromVisualTree(object? sender, VisualTreeAttachmentEventArgs e)
    {
        var control = (NeoLoading)sender!;
        
        StopAnimation(control);
        Reset(control);
    }

    private static void OnPropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
    {
        var control = (NeoLoading)sender!;

        if (e.Property != NeoLoading.StateProperty)
            return;
        
        if (!GetAnimation(control) || !control.IsLoaded)
            return;
        
        var oldState = e.GetOldValue<NeoLoadingState>();
        var newState = e.GetNewValue<NeoLoadingState>();
        
        OnStateChanged(control, oldState, newState);
    }
    
    private static void OnStateChanged(NeoLoading control, NeoLoadingState oldState, NeoLoadingState newState)
    {
        var context = GetAnimationContext(control);
        
        if (context is null || oldState == newState)
            return;

        _ = TransitionAsync(control, context, oldState, newState);
    }

    private static Task TransitionAsync(
        NeoLoading control,
        NeoLoadingAnimationContext context,
        NeoLoadingState oldState,
        NeoLoadingState newState)
    {
        SetIcon(context, newState);
        
        return (oldState, newState) switch
        {
            (NeoLoadingState.Idle, NeoLoadingState.Loading)
                => RunLoadingLoopAsync(control, context),
            (NeoLoadingState.Idle, NeoLoadingState.Failed)
                => StartShowIconAnimationAsync(context),
            (NeoLoadingState.Idle, NeoLoadingState.Success)
                => StartShowIconAnimationAsync(context),
            (NeoLoadingState.Loading, NeoLoadingState.Failed)
                => StartShowIconAnimationAsync(context),
            (NeoLoadingState.Loading, NeoLoadingState.Success)
                => StartShowIconAnimationAsync(context),
            (NeoLoadingState.Failed, NeoLoadingState.Idle)
                => StartHideIconAnimationAsync(context),
            (NeoLoadingState.Failed, NeoLoadingState.Loading)
                => Task.WhenAll(StartHideIconAnimationAsync(context),
                    RunLoadingLoopAsync(control, context)),
            (NeoLoadingState.Failed, NeoLoadingState.Success)
                => Task.CompletedTask,
            (NeoLoadingState.Success, NeoLoadingState.Idle)
                => StartHideIconAnimationAsync(context),
            (NeoLoadingState.Success, NeoLoadingState.Loading)
                => Task.WhenAll(StartHideIconAnimationAsync(context),
                    RunLoadingLoopAsync(control, context)),
            _ => Task.CompletedTask
        };
    }
    
    private static void SetIcon(NeoLoadingAnimationContext context, NeoLoadingState state)
    {
        switch (state)
        {
            case NeoLoadingState.Failed:
                SetIconCore("NeoLoadingIconFailedPathData");
                break;

            case NeoLoadingState.Success:
                SetIconCore("NeoLoadingIconSuccessPathData");
                break;
        }

        void SetIconCore(string key)
        {
            var app = Application.Current!;

            if (!app.TryGetResource(key, out var resource))
                throw new InvalidOperationException($"资源未找到： {key}");

            ((Path)context.IconPathTarget.Control).Data = (StreamGeometry)resource!;
        }
    }

    private static async Task RunLoadingLoopAsync(NeoLoading control, NeoLoadingAnimationContext context)
    {
        if (context.LoadingLoopRunning)
        {
            return;
        }
        
        context.LoadingLoopRunning = true;
        
        while (control.State == NeoLoadingState.Loading)
        {
            await StartLoadingAnimationAsync(context);
        }

        context.LoadingLoopRunning = false;
    }
    
    private static async Task StartLoadingAnimationAsync(NeoLoadingAnimationContext context)
    {
        var ani1 = context.PickaxePathTarget.Compositor!.CreateScalarKeyFrameAnimation();
        var easing1 = new BackEaseWithPowerIn(EasePower.Weak);
        ani1.InsertKeyFrame(0, 1f, easing1);
        ani1.InsertKeyFrame(1, -0.20f, easing1);
        ani1.Duration = TimeSpan.FromMilliseconds(350);
        ani1.DelayTime = TimeSpan.FromMilliseconds(250);
        ani1.DelayBehavior = AnimationDelayBehavior.SetInitialValueBeforeDelay;
        await CompositionAnimationHelper.StartAnimationAsync(context.PickaxePathTarget, "RotationAngle", ani1);

        context.PickaxeFallen = true;
        
        var ani2 = context.PickaxePathTarget.Compositor!.CreateScalarKeyFrameAnimation();
        var easing2 = new NeoLoadingEase();
        ani2.InsertKeyFrame(0, -0.20f, easing2);
        ani2.InsertKeyFrame(1, 1f, easing2);
        ani2.Duration = TimeSpan.FromMilliseconds(900);
        
        context.PickaxePathTarget.Visual!.CenterPoint = new Vector3D(30, 30, 0);

        var ani3 = context.LeftPiecePathTarget.Compositor!.CreateVector3DKeyFrameAnimation();
        var ani4 = context.RightPiecePathTarget.Compositor!.CreateVector3DKeyFrameAnimation();
        var easing3 = new CubicEaseOut();
        ani3.InsertKeyFrame(0, new Vector3D(7, 42, 0), easing3);
        ani3.InsertKeyFrame(1, new Vector3D(2, 36, 0), easing3);
        ani3.Duration = TimeSpan.FromMilliseconds(180);
        ani3.Target = "Offset";
        ani4.InsertKeyFrame(0, new Vector3D(14, 41, 0), easing3);
        ani4.InsertKeyFrame(1, new Vector3D(19, 35, 0), easing3);
        ani4.Duration = TimeSpan.FromMilliseconds(180);
        ani4.Target = "Offset";
        
        var ani5 = context.LeftPiecePathTarget.Compositor!.CreateScalarKeyFrameAnimation();
        var ani6 = context.RightPiecePathTarget.Compositor!.CreateScalarKeyFrameAnimation();
        ani5.InsertKeyFrame(0, 1);
        ani5.InsertKeyFrame(1, 0);
        ani5.Duration = TimeSpan.FromMilliseconds(100);
        ani5.DelayTime = TimeSpan.FromMilliseconds(50);
        ani5.Target = "Opacity";
        ani6.InsertKeyFrame(0, 1);
        ani6.InsertKeyFrame(1, 0);
        ani6.Duration = TimeSpan.FromMilliseconds(100);
        ani6.DelayTime = TimeSpan.FromMilliseconds(50);
        ani6.Target = "Opacity";

        await Task.WhenAll(
            CompositionAnimationHelper.StartAnimationAsync(context.PickaxePathTarget, "RotationAngle", ani2),
            CompositionAnimationHelper.StartAnimationGroupAsync(context.LeftPiecePathTarget, [ani3, ani5]),
            CompositionAnimationHelper.StartAnimationGroupAsync(context.RightPiecePathTarget, [ani4, ani6])
        );

        context.PickaxeFallen = false;
    }
    
    private static Task StartShowIconAnimationAsync(NeoLoadingAnimationContext context)
    {
        var delay = TimeSpan.FromMilliseconds(context.PickaxeFallen ? 300 : 700);
        
        var ani1 = context.IconPathTarget.Compositor!.CreateVector3DKeyFrameAnimation();
        var easing = new BackEaseWithPowerOut(EasePower.Middle);
        ani1.InsertKeyFrame(0, new Vector3D(0.5, 0.5, 1), easing);
        ani1.InsertKeyFrame(1, new Vector3D(1, 1, 1), easing);
        ani1.Duration = TimeSpan.FromMilliseconds(400);
        ani1.DelayTime = delay;
        ani1.Target = "Scale";
        
        var ani2 = context.IconPathTarget.Compositor!.CreateScalarKeyFrameAnimation();
        ani2.InsertKeyFrame(0, 0);
        ani2.InsertKeyFrame(1, 1);
        ani2.Duration = TimeSpan.FromMilliseconds(100);
        ani2.DelayTime = delay;
        ani2.Target = "Opacity";

        return CompositionAnimationHelper.StartAnimationGroupAsync(context.IconPathTarget, [ani1, ani2]);
    }
    
    private static Task StartHideIconAnimationAsync(NeoLoadingAnimationContext context)
    {
        var ani1 = context.IconPathTarget.Compositor!.CreateVector3DKeyFrameAnimation();
        ani1.InsertKeyFrame(0, new Vector3D(1, 1, 1));
        ani1.InsertKeyFrame(1, new Vector3D(0.5, 0.5, 1));
        ani1.Duration = TimeSpan.FromMilliseconds(200);
        ani1.Target = "Scale";
        
        var ani2 = context.IconPathTarget.Compositor!.CreateScalarKeyFrameAnimation();
        ani2.InsertKeyFrame(0, 1);
        ani2.InsertKeyFrame(1, 0);
        ani2.Duration = TimeSpan.FromMilliseconds(100);
        ani2.Target = "Opacity";

        return CompositionAnimationHelper.StartAnimationGroupAsync(context.IconPathTarget, [ani1, ani2]);
    }
    
    private static void StopAnimation(NeoLoading control)
    {
        var context = control.GetValue(AnimationContextProperty);
        if (context is null)
            return;

        context.PickaxePathTarget.Visual?.StopAnimation("RotationAngle");
        context.LeftPiecePathTarget.Visual?.StopAnimation("Offset");
        context.LeftPiecePathTarget.Visual?.StopAnimation("Opacity");
        context.RightPiecePathTarget.Visual?.StopAnimation("Offset");
        context.RightPiecePathTarget.Visual?.StopAnimation("Opacity");
        context.IconPathTarget.Visual?.StopAnimation("Scale");
        context.IconPathTarget.Visual?.StopAnimation("Opacity");
    }

    private static void Reset(NeoLoading control)
    {
        var context = GetAnimationContext(control)!;
        context.PickaxePathTarget.Visual!.RotationAngle = 1f;
        context.PickaxePathTarget.Visual.CenterPoint = new Vector3D(30, 30, 0);
        context.IconPathTarget.Visual!.Scale = new Vector3D(0.5, 0.5, 1);
        context.IconPathTarget.Visual.CenterPoint = new Vector3D(8.5, 8.5, 0);
        context.IconPathTarget.Visual.Opacity = 0;
    }

    public record NeoLoadingAnimationContext
    {
        public required CompositionAnimationTarget PickaxePathTarget { get; init; }
        public required CompositionAnimationTarget LeftPiecePathTarget { get; init; }
        public required CompositionAnimationTarget RightPiecePathTarget { get; init; }
        public required CompositionAnimationTarget IconPathTarget { get; init; }
        public bool PickaxeFallen { get; set; }
        public bool LoadingLoopRunning { get; set; }
        
        public void Initialize()
        {
            PickaxePathTarget.Initialize();
            LeftPiecePathTarget.Initialize();
            RightPiecePathTarget.Initialize();
            IconPathTarget.Initialize();
        }
    }

    public class NeoLoadingEase : Easing
    {
        public override double Ease(double progress)
        {
            var u = 1d - progress;

            return 1d - (
                2d * u * u * u
                + Math.Pow(u, 1.25d) *
                Math.Cos(2.5d * Math.PI * Math.Pow(progress, 1.5d))
            ) / 3d;
        }
    }
}