using Avalonia.Animation;
using Avalonia.Animation.Easings;
using Avalonia.Media;
using Avalonia.Styling;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace PCL.Neo.Animations;

public class ScaleTransformScaleXAnimation : IAnimation
{
    private CancellationTokenSource _cancellationTokenSource;
    public Animatable Control { get; set; }
    public TimeSpan Duration { get; set; }
    public TimeSpan Delay { get; set; }
    public double? ValueBefore { get; set; }
    public double ValueAfter { get; set; }
    public Easing Easing { get; set; }
    public bool Wait { get; set; } = false;

    public ScaleTransformScaleXAnimation(Animatable control, double valueAfter) : this(
        control, valueAfter, new LinearEasing())
    {
    }

    public ScaleTransformScaleXAnimation(Animatable control, double valueAfter, Easing easing) : this(
        control, TimeSpan.FromSeconds(1), valueAfter, easing)
    {
    }

    public ScaleTransformScaleXAnimation(Animatable control, TimeSpan duration, double valueAfter) : this(
        control, duration, valueAfter, new LinearEasing())
    {
    }

    public ScaleTransformScaleXAnimation(Animatable control, TimeSpan duration, TimeSpan delay, double valueAfter) :
        this(
            control, duration, delay, valueAfter, new LinearEasing())
    {
    }

    public ScaleTransformScaleXAnimation(Animatable control, TimeSpan duration, double valueAfter, Easing easing) :
        this(
            control, duration, control.GetValue(ScaleTransform.ScaleXProperty), valueAfter, easing)
    {
    }

    public ScaleTransformScaleXAnimation(Animatable control, TimeSpan duration, TimeSpan delay, double valueAfter,
        Easing easing) : this(
        control, duration, delay, control.GetValue(ScaleTransform.ScaleXProperty), valueAfter, easing)
    {
    }

    public ScaleTransformScaleXAnimation(Animatable control, double? valueBefore, double valueAfter) : this(
        control, valueBefore, valueAfter, new LinearEasing())
    {
    }

    public ScaleTransformScaleXAnimation(Animatable control, double? valueBefore, double valueAfter, Easing easing) :
        this(
            control, TimeSpan.FromSeconds(1), valueBefore, valueAfter, easing)
    {
    }

    public ScaleTransformScaleXAnimation(Animatable control, TimeSpan duration, double? valueBefore, double valueAfter)
        : this(
            control, duration, valueBefore, valueAfter, new LinearEasing())
    {
    }

    public ScaleTransformScaleXAnimation(Animatable control, TimeSpan duration, TimeSpan delay, double? valueBefore,
        double valueAfter) : this(
        control, duration, delay, valueBefore, valueAfter, new LinearEasing())
    {
    }

    public ScaleTransformScaleXAnimation(Animatable control, TimeSpan duration, double? valueBefore, double valueAfter,
        Easing easing) : this(
        control, duration, TimeSpan.Zero, valueBefore, valueAfter, easing)
    {
    }

    public ScaleTransformScaleXAnimation(Animatable control, TimeSpan duration, TimeSpan delay, double? valueBefore,
        double valueAfter, Easing easing)
    {
        Control = control;
        Duration = duration;
        Delay = delay;
        ValueBefore = valueBefore;
        ValueAfter = valueAfter;
        Easing = easing;
        _cancellationTokenSource = new CancellationTokenSource();
    }

    public async Task RunAsync()
    {
        var animation = new Animation
        {
            Easing = Easing,
            Duration = Duration,
            Delay = Delay,
            FillMode = FillMode.Both,
            Children =
            {
                new KeyFrame
                {
                    Setters =
                    {
                        new Setter(ScaleTransform.ScaleXProperty, ValueBefore)
                    },
                    Cue = new Cue(0d)
                },
                new KeyFrame
                {
                    Setters =
                    {
                        new Setter(ScaleTransform.ScaleXProperty, ValueAfter)
                    },
                    Cue = new Cue(1d)
                }
            }
        };
        await animation.RunAsync(Control, _cancellationTokenSource.Token);
    }

    public void Cancel()
    {
        _cancellationTokenSource.Cancel();
    }
}
