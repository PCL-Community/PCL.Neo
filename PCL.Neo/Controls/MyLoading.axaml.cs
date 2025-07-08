using Avalonia;
using Avalonia.Animation.Easings;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Shapes;
using Avalonia.Threading;
using PCL.Neo.Animations;
using PCL.Neo.Animations.Easings;
using PCL.Neo.Helpers;
using System;
using System.Threading.Tasks;

namespace PCL.Neo.Controls;

[PseudoClasses(":loading", ":error")]
public class MyLoading : TemplatedControl
{
    private AnimationHelper _animation;
    private Path? _pathPickaxe;
    private Path? _pathError;
    private Path? _pathLeft;
    private Path? _pathRight;
    private bool _hasErrorOccurred;

    public MyLoading()
    {
        _animation = new AnimationHelper();
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        _pathPickaxe = e.NameScope.Find<Path>("PathPickaxe");
        _pathError = e.NameScope.Find<Path>("PathError");
        _pathLeft = e.NameScope.Find<Path>("PathLeft");
        _pathRight = e.NameScope.Find<Path>("PathRight");

        SetPseudoClasses();
        RefreshText();
        StartAnimation();
    }

    public static readonly StyledProperty<string> TextProperty = AvaloniaProperty.Register<MyLoading, string>(
        nameof(Text));

    public string Text
    {
        get => GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    public static readonly StyledProperty<string> TextErrorProperty = AvaloniaProperty.Register<MyLoading, string>(
        nameof(TextError),
        "加载失败");

    public string TextError
    {
        get => GetValue(TextErrorProperty);
        set
        {
            SetValue(TextErrorProperty, value);
            RefreshText();
        }
    }

    public static readonly StyledProperty<string> TextLoadingProperty = AvaloniaProperty.Register<MyLoading, string>(
        nameof(TextLoading),
        "加载中");

    public string TextLoading
    {
        get => GetValue(TextLoadingProperty);
        set
        {
            SetValue(TextLoadingProperty, value);
            RefreshText();
        }
    }

    public enum LoadingState
    {
        Loading,
        Error
    }

    public static readonly StyledProperty<LoadingState> StateProperty =
        AvaloniaProperty.Register<MyLoading, LoadingState>(
            nameof(State));

    public LoadingState State
    {
        get => GetValue(StateProperty);
        set
        {
            SetValue(StateProperty, value);
            SetPseudoClasses();
            RefreshText();
        }
    }

    private void StartAnimation()
    {
        Dispatcher.UIThread.InvokeAsync(async () =>
        {
            while (true)
            {
                var currentState = State;
                switch (currentState)
                {
                    case LoadingState.Loading:
                        if (_hasErrorOccurred)
                        {
                            await AnimationErrorToLoadingAsync();
                        }

                        _hasErrorOccurred = false;
                        await AnimationLoadingAsync();
                        break;
                    case LoadingState.Error:
                        if (!_hasErrorOccurred)
                        {
                            _hasErrorOccurred = true;
                            await AnimationLoadingToErrorAsync();
                            break;
                        }

                        await Task.Delay(100);
                        break;
                    default:
                        await Task.Delay(100);
                        break;
                }
            }
        });
    }

    private async Task AnimationErrorToLoadingAsync()
    {
        _animation.CancelAndClear();
        _animation.Animations.AddRange(
        [
            new RotateTransformAngleAnimation(_pathPickaxe!, TimeSpan.FromMilliseconds(350), 55d, -20d,
                new MyBackEaseIn(EasePower.Weak)),
            new OpacityAnimation(_pathError!, TimeSpan.FromMilliseconds(100), 0d),
            new ScaleTransformScaleXAnimation(_pathError!, TimeSpan.FromMilliseconds(100), 1d, 0.5d),
            new ScaleTransformScaleYAnimation(_pathError!, TimeSpan.FromMilliseconds(400), 1d, 0.5d)
        ]);
        await _animation.RunAsync();
    }

    private async Task AnimationLoadingToErrorAsync()
    {
        _animation.CancelAndClear();
        _animation.Animations.AddRange(
        [
            new RotateTransformAngleAnimation(_pathPickaxe!, TimeSpan.FromMilliseconds(900), 55d, new CubicEaseOut()),
            new OpacityAnimation(_pathError!, TimeSpan.FromMilliseconds(300), 1d),
            new ScaleTransformScaleXAnimation(_pathError!, TimeSpan.FromMilliseconds(400), 0.5d, 1d,
                new MyBackEaseOut()),
            new ScaleTransformScaleYAnimation(_pathError!, TimeSpan.FromMilliseconds(400), 0.5d, 1d,
                new MyBackEaseOut())
        ]);
        await _animation.RunAsync();
    }

    private async Task AnimationLoadingAsync()
    {
        // 循环动画，听说这里折磨龙猫很久(doge)
        _animation.CancelAndClear();
        _animation.Animations.AddRange(
        [
            new RotateTransformAngleAnimation(_pathPickaxe!, TimeSpan.FromMilliseconds(350), 55d, -20d,
                new MyBackEaseIn(EasePower.Weak)),
            new RotateTransformAngleAnimation(_pathPickaxe!, TimeSpan.FromMilliseconds(900), 30d, 55d,
                new ElasticEaseOut()),
            new RotateTransformAngleAnimation(_pathPickaxe!, TimeSpan.FromMilliseconds(180), -20d, 30d),
            new OpacityAnimation(_pathLeft!, TimeSpan.FromMilliseconds(100), TimeSpan.FromMilliseconds(50), 1d, 0d),
            new XAnimation(_pathLeft!, TimeSpan.FromMilliseconds(180), -5d, new CubicEaseOut()),
            new YAnimation(_pathLeft!, TimeSpan.FromMilliseconds(180), -6d, new CubicEaseOut()),
            new OpacityAnimation(_pathRight!, TimeSpan.FromMilliseconds(100), TimeSpan.FromMilliseconds(50), 1d, 0d),
            new XAnimation(_pathRight!, TimeSpan.FromMilliseconds(180), 5d, new CubicEaseOut()),
            new YAnimation(_pathRight!, TimeSpan.FromMilliseconds(180), -6d, new CubicEaseOut())
        ]);
        await _animation.RunAsync();
        _pathLeft!.Margin = new Thickness(7, 41, 0, 0);
        _pathRight!.Margin = new Thickness(14, 41, 0, 0);
    }

    private void SetPseudoClasses()
    {
        PseudoClasses.Remove(":loading");
        PseudoClasses.Remove(":error");
        if (State == LoadingState.Loading)
        {
            PseudoClasses.Set(":loading", true);
        }
        else
        {
            PseudoClasses.Set(":error", true);
        }
    }

    private void RefreshText()
    {
        if (State == LoadingState.Loading)
        {
            Text = TextLoading;
        }
        else
        {
            Text = TextError;
        }
    }
}
