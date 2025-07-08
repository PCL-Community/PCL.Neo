using Avalonia;
using Avalonia.Animation;
using Avalonia.Animation.Easings;
using Avalonia.Controls;
using Avalonia.Styling;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace PCL.Neo.Animations;

public class MarginAnimation : IAnimation
{
    private CancellationTokenSource _cancellationTokenSource;
    public Animatable Control { get; set; }
    public TimeSpan Duration { get; set; }
    public TimeSpan Delay { get; set; }
    public Thickness? ValueBefore { get; set; }
    public Thickness ValueAfter { get; set; }
    public Easing Easing { get; set; }
    public bool Wait { get; set; } = false;

    public MarginAnimation(Animatable control, Thickness valueAfter) : this(
        control, valueAfter, new LinearEasing())
    {
    }

    public MarginAnimation(Animatable control, Thickness valueAfter, Easing easing) : this(
        control, TimeSpan.FromSeconds(1), valueAfter, easing)
    {
    }

    public MarginAnimation(Animatable control, TimeSpan duration, Thickness valueAfter) : this(
        control, duration, valueAfter, new LinearEasing())
    {
    }

    public MarginAnimation(Animatable control, TimeSpan duration, TimeSpan delay, Thickness valueAfter) : this(
        control, duration, delay, valueAfter, new LinearEasing())
    {
    }

    public MarginAnimation(Animatable control, TimeSpan duration, Thickness valueAfter, Easing easing) : this(
        control, duration, GetCurrentMargin(control), valueAfter, easing)
    {
    }

    public MarginAnimation(Animatable control, TimeSpan duration, TimeSpan delay, Thickness valueAfter,
        Easing easing) : this(
        control, duration, delay, GetCurrentMargin(control), valueAfter, easing)
    {
    }

    public MarginAnimation(Animatable control, Thickness? valueBefore, Thickness valueAfter) : this(
        control, valueBefore, valueAfter, new LinearEasing())
    {
    }

    public MarginAnimation(Animatable control, Thickness? valueBefore, Thickness valueAfter, Easing easing) : this(
        control, TimeSpan.FromSeconds(1), valueBefore, valueAfter, easing)
    {
    }

    public MarginAnimation(Animatable control, TimeSpan duration, Thickness? valueBefore, Thickness valueAfter) : this(
        control, duration, valueBefore, valueAfter, new LinearEasing())
    {
    }

    public MarginAnimation(Animatable control, TimeSpan duration, TimeSpan delay, Thickness? valueBefore,
        Thickness valueAfter) : this(
        control, duration, delay, valueBefore, valueAfter, new LinearEasing())
    {
    }

    public MarginAnimation(Animatable control, TimeSpan duration, Thickness? valueBefore, Thickness valueAfter,
        Easing easing) : this(
        control, duration, TimeSpan.Zero, valueBefore, valueAfter, easing)
    {
    }

    public MarginAnimation(Animatable control, TimeSpan duration, TimeSpan delay, Thickness? valueBefore,
        Thickness valueAfter, Easing easing)
    {
        Control = control;
        Duration = duration;
        Delay = delay;
        ValueBefore = valueBefore;
        ValueAfter = valueAfter;
        Easing = easing;
        _cancellationTokenSource = new CancellationTokenSource();
    }

    private static Thickness? GetCurrentMargin(Animatable control)
    {
        if (control is Control c)
        {
            return c.Margin;
        }

        return null;
    }

    public async Task RunAsync()
    {
        if (Control is not Control controlWithMargin)
            return;

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
                        new Setter(Avalonia.Controls.Control.MarginProperty, ValueBefore)
                    },
                    Cue = new Cue(0d)
                },
                new KeyFrame
                {
                    Setters =
                    {
                        new Setter(Avalonia.Controls.Control.MarginProperty, ValueAfter)
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
