using Avalonia;
using Avalonia.Animation.Easings;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Rendering.Composition;
using Avalonia.Rendering.Composition.Animations;
using CommunityToolkit.Mvvm.Messaging;
using PCL.Neo.Animations.Easings;
using PCL.Neo.Helpers;
using PCL.Neo.Messages;
using PCL.Neo.Services;
using System;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using CubicEaseOut = Avalonia.Animation.Easings.CubicEaseOut;

namespace PCL.Neo.Views;

public partial class MainWindow : Window
{
    private CompositionVisual? _leftNavigationControlVisual;
    private CompositionVisual? _rightNavigationControlVisual;
    private Compositor? _compositor;

    public MainWindow()
    {
        InitializeComponent();

        NavBackgroundBorder.PointerPressed += (i, e) =>
        {
            if (e.GetCurrentPoint(i as Control).Properties.IsLeftButtonPressed)
            {
                BeginMoveDrag(e);
            }
        };
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            SetupSide("Left", StandardCursorType.LeftSide, WindowEdge.West);
            SetupSide("Right", StandardCursorType.RightSide, WindowEdge.East);
            SetupSide("Top", StandardCursorType.TopSide, WindowEdge.North);
            SetupSide("Bottom", StandardCursorType.BottomSide, WindowEdge.South);
            SetupSide("TopLeft", StandardCursorType.TopLeftCorner, WindowEdge.NorthWest);
            SetupSide("TopRight", StandardCursorType.TopRightCorner, WindowEdge.NorthEast);
            SetupSide("BottomLeft", StandardCursorType.BottomLeftCorner, WindowEdge.SouthWest);
            SetupSide("BottomRight", StandardCursorType.BottomRightCorner, WindowEdge.SouthEast);
        }

        new ThemeHelper(this).Refresh(Application.Current!.ActualThemeVariant);

        BtnTitleClose.Click += async (_, _) =>
        {
            AnimationOut();
            await Task.Delay(180);
            Close();
        };

        BtnTitleMin.Click += (_, _) => WindowState = WindowState.Minimized;

        LeftNavigationControl.Loaded += (_, _) =>
        {
            LeftNavigationControlBorder.Width = LeftNavigationControl!.Bounds.Width;

            LeftNavigationControl!.SizeChanged += (_, args) =>
            {
                if (args.WidthChanged)
                    LeftNavigationControlBorder.Width = args.NewSize.Width;
            };

            // 获取导航控件的CompositionVisual，用于动画
            _leftNavigationControlVisual = ElementComposition.GetElementVisual(LeftNavigationControl);
            _rightNavigationControlVisual = ElementComposition.GetElementVisual(RightNavigationControl);

            if (_leftNavigationControlVisual != null)
            {
                _compositor = _leftNavigationControlVisual.Compositor;
            }
        };

        GridRoot.Opacity = 0; // 在此处初始化透明度，不然将闪现
        Loaded += (_, _) =>
        {
            // 订阅导航事件
            WeakReferenceMessenger.Default.Register<NavigationMessage, Guid>(
                this, NavigationMessage.Channels.Navigated,
                (_, m) => OnNavigated(m));

            AnimationIn();
        };
        // 取消订阅导航事件
        Unloaded += (_, _) => WeakReferenceMessenger.Default.Unregister<NavigationMessage>(this);
    }

    private void OnNavigated(NavigationMessage e)
    {
        if (_compositor == null || _leftNavigationControlVisual == null || _rightNavigationControlVisual == null)
            return;

        // 导航动画效果
        if (e is { NavigationType: NavigationType.Forward, IsMainViewModelChanged: true })
        {
            // 前进动画
            PlayForwardNavigationAnimation();
        }

        if (e is { NavigationType: NavigationType.Backward, IsMainViewModelChanged: true })
        {
            // 后退动画
            PlayBackwardNavigationAnimation();
        }
    }

    private void PlayForwardNavigationAnimation()
    {
        if (_compositor == null || _leftNavigationControlVisual == null || _rightNavigationControlVisual == null)
            return;

        // 左侧面板动画 - 从左滑入
        var leftOffsetAnimation = _compositor.CreateVector3KeyFrameAnimation();
        leftOffsetAnimation.Duration = TimeSpan.FromMilliseconds(300);
        leftOffsetAnimation.InsertKeyFrame(0f, new Vector3(-50f, 0f, 0f), new CubicEaseOut());
        leftOffsetAnimation.InsertKeyFrame(1f, new Vector3(0f, 0f, 0f), new CubicEaseOut());
        leftOffsetAnimation.Target = "Offset";

        var leftOpacityAnimation = _compositor.CreateScalarKeyFrameAnimation();
        leftOpacityAnimation.Duration = TimeSpan.FromMilliseconds(300);
        leftOpacityAnimation.InsertKeyFrame(0f, 0f, new CubicEaseOut());
        leftOpacityAnimation.InsertKeyFrame(1f, 1f, new CubicEaseOut());
        leftOpacityAnimation.Target = "Opacity";

        // 右侧面板动画 - 从右滑入
        var rightOffsetAnimation = _compositor.CreateVector3KeyFrameAnimation();
        rightOffsetAnimation.Duration = TimeSpan.FromMilliseconds(300);
        rightOffsetAnimation.InsertKeyFrame(0f, new Vector3(50f, 0f, 0f), new CubicEaseOut());
        rightOffsetAnimation.InsertKeyFrame(1f, new Vector3(0f, 0f, 0f), new CubicEaseOut());
        rightOffsetAnimation.Target = "Offset";

        var rightOpacityAnimation = _compositor.CreateScalarKeyFrameAnimation();
        rightOpacityAnimation.Duration = TimeSpan.FromMilliseconds(300);
        rightOpacityAnimation.InsertKeyFrame(0f, 0f, new CubicEaseOut());
        rightOpacityAnimation.InsertKeyFrame(1f, 1f, new CubicEaseOut());
        rightOpacityAnimation.Target = "Opacity";

        // 应用动画
        var leftAnimationGroup = _compositor.CreateAnimationGroup();
        leftAnimationGroup.Add(leftOffsetAnimation);
        leftAnimationGroup.Add(leftOpacityAnimation);

        var rightAnimationGroup = _compositor.CreateAnimationGroup();
        rightAnimationGroup.Add(rightOffsetAnimation);
        rightAnimationGroup.Add(rightOpacityAnimation);

        _leftNavigationControlVisual.StartAnimationGroup(leftAnimationGroup);
        _rightNavigationControlVisual.StartAnimationGroup(rightAnimationGroup);
    }

    private void PlayBackwardNavigationAnimation()
    {
        if (_compositor == null || _leftNavigationControlVisual == null || _rightNavigationControlVisual == null)
            return;

        // 左侧面板动画 - 从右滑入
        var leftOffsetAnimation = _compositor.CreateVector3KeyFrameAnimation();
        leftOffsetAnimation.Duration = TimeSpan.FromMilliseconds(300);
        leftOffsetAnimation.InsertKeyFrame(0f, new Vector3(50f, 0f, 0f), new CubicEaseOut());
        leftOffsetAnimation.InsertKeyFrame(1f, new Vector3(0f, 0f, 0f), new CubicEaseOut());
        leftOffsetAnimation.Target = "Offset";

        var leftOpacityAnimation = _compositor.CreateScalarKeyFrameAnimation();
        leftOpacityAnimation.Duration = TimeSpan.FromMilliseconds(300);
        leftOpacityAnimation.InsertKeyFrame(0f, 0f, new CubicEaseOut());
        leftOpacityAnimation.InsertKeyFrame(1f, 1f, new CubicEaseOut());
        leftOpacityAnimation.Target = "Opacity";

        // 右侧面板动画 - 从左滑入
        var rightOffsetAnimation = _compositor.CreateVector3KeyFrameAnimation();
        rightOffsetAnimation.Duration = TimeSpan.FromMilliseconds(300);
        rightOffsetAnimation.InsertKeyFrame(0f, new Vector3(-50f, 0f, 0f), new CubicEaseOut());
        rightOffsetAnimation.InsertKeyFrame(1f, new Vector3(0f, 0f, 0f), new CubicEaseOut());
        rightOffsetAnimation.Target = "Offset";

        var rightOpacityAnimation = _compositor.CreateScalarKeyFrameAnimation();
        rightOpacityAnimation.Duration = TimeSpan.FromMilliseconds(300);
        rightOpacityAnimation.InsertKeyFrame(0f, 0f, new CubicEaseOut());
        rightOpacityAnimation.InsertKeyFrame(1f, 1f, new CubicEaseOut());
        rightOpacityAnimation.Target = "Opacity";

        // 应用动画
        var leftAnimationGroup = _compositor.CreateAnimationGroup();
        leftAnimationGroup.Add(leftOffsetAnimation);
        leftAnimationGroup.Add(leftOpacityAnimation);

        var rightAnimationGroup = _compositor.CreateAnimationGroup();
        rightAnimationGroup.Add(rightOffsetAnimation);
        rightAnimationGroup.Add(rightOpacityAnimation);

        _leftNavigationControlVisual.StartAnimationGroup(leftAnimationGroup);
        _rightNavigationControlVisual.StartAnimationGroup(rightAnimationGroup);
    }

    private void SetupSide(string name, StandardCursorType cursor, WindowEdge edge)
    {
        var ctl = this.Get<Control>(name);
        ctl.Cursor = new Cursor(cursor);
        ctl.PointerPressed += (i, e) =>
        {
            if (e.GetCurrentPoint(i as Control).Properties.IsLeftButtonPressed)
            {
                BeginResizeDrag(edge, e);
            }
        };
    }

    /// <summary>
    /// 进入窗口的动画。
    /// </summary>
    private async void AnimationIn()
    {
        // var animation = new AnimationHelper(
        // [
        //     new OpacityAnimation(this, TimeSpan.FromMilliseconds(250), 0d, 1d),
        //     new TranslateTransformYAnimation(this, TimeSpan.FromMilliseconds(600), 60d, 0d, new MyBackEaseOut(EasePower.Weak)),
        //     new RotateTransformAngleAnimation(this, TimeSpan.FromMilliseconds(500), -4d, 0d, new MyBackEaseOut(EasePower.Weak))
        // ]);
        // await animation.RunAsync();

        // AnimationHelper 性能太差，换用 CompositionAnimation
        await Task.Delay(100);

        var mainWindowCompositionVisual = ElementComposition.GetElementVisual(GridRoot)!;
        var compositor = mainWindowCompositionVisual.Compositor;

        var opacityFrameAnimation = compositor.CreateScalarKeyFrameAnimation();
        opacityFrameAnimation.Duration = TimeSpan.FromMilliseconds(250);
        opacityFrameAnimation.InsertKeyFrame(0f, 0f, new CubicEaseOut());
        opacityFrameAnimation.InsertKeyFrame(1f, 1f, new CubicEaseOut());
        opacityFrameAnimation.Target = "Opacity";

        var rotateTransformAngleAnimation = compositor.CreateScalarKeyFrameAnimation();
        rotateTransformAngleAnimation.Duration = TimeSpan.FromMilliseconds(500);
        rotateTransformAngleAnimation.InsertKeyFrame(0f, -0.06f, new MyBackEaseOut(EasePower.Weak));
        rotateTransformAngleAnimation.InsertKeyFrame(1f, 0f, new MyBackEaseOut(EasePower.Weak));
        rotateTransformAngleAnimation.Target = "RotationAngle";

        var translateTransformYAnimation = compositor.CreateVector3KeyFrameAnimation();
        translateTransformYAnimation.Duration = TimeSpan.FromMilliseconds(600);
        translateTransformYAnimation.InsertKeyFrame(0f, new Vector3(0f, 60f, 0f), new MyBackEaseOut(EasePower.Weak));
        translateTransformYAnimation.InsertKeyFrame(1f, new Vector3(0f, 0f, 0f), new MyBackEaseOut(EasePower.Weak));
        translateTransformYAnimation.Target = "Offset";

        var animationGroup = compositor.CreateAnimationGroup();
        animationGroup.Add(opacityFrameAnimation);
        animationGroup.Add(rotateTransformAngleAnimation);
        animationGroup.Add(translateTransformYAnimation);

        var size = mainWindowCompositionVisual.Size;
        mainWindowCompositionVisual.CenterPoint = new Vector3D((float)size.X / 2, (float)size.Y / 2,
            (float)mainWindowCompositionVisual.CenterPoint.Z);

        mainWindowCompositionVisual.StartAnimationGroup(animationGroup);
    }

    /// <summary>
    /// 关闭窗口的动画。
    /// </summary>
    private void AnimationOut()
    {
        // var animation = new AnimationHelper(
        // [
        //     new OpacityAnimation(this, TimeSpan.FromMilliseconds(140), TimeSpan.FromMilliseconds(40), 0d, new QuadraticEaseOut()),
        //     new ScaleTransformScaleXAnimation(this, TimeSpan.FromMilliseconds(180), 0.88d),
        //     new ScaleTransformScaleYAnimation(this, TimeSpan.FromMilliseconds(180), 0.88d),
        //     new TranslateTransformYAnimation(this, TimeSpan.FromMilliseconds(180), 20d, new QuadraticEaseOut()),
        //     new RotateTransformAngleAnimation(this, TimeSpan.FromMilliseconds(180), 0.6d, new QuadraticEaseInOut())
        // ]);
        // await animation.RunAsync();

        // AnimationHelper 性能太差，换用 CompositionAnimation

        var mainWindowCompositionVisual = ElementComposition.GetElementVisual(GridRoot)!;
        var compositor = mainWindowCompositionVisual.Compositor;

        var opacityFrameAnimation = compositor.CreateScalarKeyFrameAnimation();
        opacityFrameAnimation.DelayBehavior = AnimationDelayBehavior.SetInitialValueBeforeDelay;
        opacityFrameAnimation.DelayTime = TimeSpan.FromMilliseconds(40);
        opacityFrameAnimation.Duration = TimeSpan.FromMilliseconds(140);
        opacityFrameAnimation.InsertKeyFrame(0f, 1f, new QuadraticEaseOut());
        opacityFrameAnimation.InsertKeyFrame(1f, 0f, new QuadraticEaseOut());
        opacityFrameAnimation.Target = "Opacity";

        var rotateTransformAngleAnimation = compositor.CreateScalarKeyFrameAnimation();
        rotateTransformAngleAnimation.Duration = TimeSpan.FromMilliseconds(180);
        rotateTransformAngleAnimation.InsertKeyFrame(0f, 0f, new QuadraticEaseOut());
        rotateTransformAngleAnimation.InsertKeyFrame(1f, 0.0006f, new QuadraticEaseOut());
        rotateTransformAngleAnimation.Target = "RotationAngle";

        var translateTransformYAnimation = compositor.CreateVector3KeyFrameAnimation();
        translateTransformYAnimation.Duration = TimeSpan.FromMilliseconds(180);
        translateTransformYAnimation.InsertKeyFrame(0f, new Vector3(0f, 0f, 0f), new QuadraticEaseOut());
        translateTransformYAnimation.InsertKeyFrame(1f, new Vector3(0f, 20f, 0f), new QuadraticEaseOut());
        translateTransformYAnimation.Target = "Offset";

        var scaleTransformAnimation = compositor.CreateVector3KeyFrameAnimation();
        scaleTransformAnimation.Duration = TimeSpan.FromMilliseconds(180);
        scaleTransformAnimation.InsertKeyFrame(0f, new Vector3(1f, 1f, 1f), new QuadraticEaseOut());
        scaleTransformAnimation.InsertKeyFrame(1f, new Vector3(0.88f, 0.88f, 1f), new QuadraticEaseOut());
        scaleTransformAnimation.Target = "Scale";

        var animationGroup = compositor.CreateAnimationGroup();
        animationGroup.Add(opacityFrameAnimation);
        animationGroup.Add(rotateTransformAngleAnimation);
        animationGroup.Add(translateTransformYAnimation);
        animationGroup.Add(scaleTransformAnimation);

        var size = mainWindowCompositionVisual.Size;
        mainWindowCompositionVisual.CenterPoint = new Vector3D((float)size.X / 2, (float)size.Y / 2,
            (float)mainWindowCompositionVisual.CenterPoint.Z);

        mainWindowCompositionVisual.StartAnimationGroup(animationGroup);
    }
}
