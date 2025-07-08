using Avalonia.Animation.Easings;
using System;

namespace PCL.Neo.Animations.Easings;

public class MyBackEaseOut : Easing
{
    private readonly double p;

    public MyBackEaseOut(EasePower power = EasePower.Middle)
    {
        p = 3 - (int)power * 0.5;
    }

    public override double Ease(double progress)
    {
        return 1 - Math.Pow(1 - progress, p) * Math.Cos(1.5 * Math.PI * progress);
    }
}
