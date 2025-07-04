using PCL.Neo.Services;
using System.Threading.Tasks;

namespace PCL.Neo.Jobs;

public class TestJob1 : Job
{
    public override double Weighting => 1.0;

    public double IntervalFactor { get; init; } = 1.0;

    public readonly Stage Stage1 = new() { Name = "Stage 1", Weighting = 1.0 };
    public readonly Stage Stage2 = new() { Name = "Stage 2", Weighting = 0.5 };

    public override async Task RunAsync()
    {
        Stage1.Running();
        await Task.Delay((int)(500 * IntervalFactor));
        Stage1.ReportProgress(0.5);
        await Task.Delay((int)(500 * IntervalFactor));
        Stage1.ReportProgress(1.0);
        Stage1.Complete();

        Stage2.Running();
        await Task.Delay((int)(500 * IntervalFactor));
        Stage2.ReportProgress(0.5);
        await Task.Delay((int)(500 * IntervalFactor));
        Stage2.ReportProgress(1.0);
        Stage2.Complete();
    }
}
