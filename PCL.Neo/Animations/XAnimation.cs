using Avalonia;
using Avalonia.Animation;
using Avalonia.Animation.Easings;
using Avalonia.Layout;
using Avalonia.Styling;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace PCL.Neo.Animations;

public class XAnimation : IAnimation
{
    private CancellationTokenSource _cancellationTokenSource;
    public Animatable Control { get; set; }
    public TimeSpan Duration { get; set; }
    public TimeSpan Delay { get; set; }
    public double Value { get; set; }
    public Easing Easing { get; set; }
    public bool Wait { get; set; } = false;

    public XAnimation(Animatable control, double value) : this(
        control, value, new LinearEasing())
    {
    }

    public XAnimation(Animatable control, double value, Easing easing) : this(
        control, TimeSpan.FromSeconds(1), value, easing)
    {
    }

    public XAnimation(Animatable control, TimeSpan duration, double value) : this(
        control, duration, value, new LinearEasing())
    {
    }

    public XAnimation(Animatable control, TimeSpan duration, TimeSpan delay, double value) : this(
        control, duration, delay, value, new LinearEasing())
    {
    }

    public XAnimation(Animatable control, TimeSpan duration, double value, Easing easing) : this(
        control, duration, TimeSpan.Zero, value, easing)
    {
    }

    public XAnimation(Animatable control, TimeSpan duration, TimeSpan delay, double value, Easing easing)
    {
        Control = control;
        Duration = duration;
        Delay = delay;
        Value = value;
        Easing = easing;
        _cancellationTokenSource = new CancellationTokenSource();
    }

    public async Task RunAsync()
    {
        var control = (Layoutable)Control;
        var marginOriginal = control.Margin;
        Thickness margin;
        switch (control.HorizontalAlignment)
        {
            case HorizontalAlignment.Left:
                margin = new Thickness(control.Margin.Left + Value, control.Margin.Top, control.Margin.Right,
                    control.Margin.Bottom);
                break;
            case HorizontalAlignment.Right:
                margin = new Thickness(control.Margin.Left, control.Margin.Top, control.Margin.Right - Value,
                    control.Margin.Bottom);
                break;
            default:
                margin = control.Margin;
                break;
        }

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
                        new Setter(Layoutable.MarginProperty, marginOriginal)
                    },
                    Cue = new Cue(1d)
                },
                new KeyFrame
                {
                    Setters =
                    {
                        new Setter(Layoutable.MarginProperty, margin)
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
