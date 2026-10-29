using Avalonia.Animation.Easings;

namespace PCL.Neo.UI.Media.Animation.Easings;

public class BackEaseWithPowerIn(EasePower power) : Easing
{
    private readonly double _p = 3 - (int)power * 0.5;

    public override double Ease(double progress)
    {
        return Math.Pow(progress, _p) * Math.Cos(1.5d * Math.PI * (1d - progress));
    }
}