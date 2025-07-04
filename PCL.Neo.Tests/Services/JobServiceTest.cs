using PCL.Neo.Services;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace PCL.Neo.Tests.Services;

public class TestJob1 : Job
{
    public override double Weighting => 1.0;

    public readonly Stage Stage1 = new() { Name = "Stage 1", Weighting = 1.0 };
    public readonly Stage Stage2 = new() { Name = "Stage 2", Weighting = 0.5 };

    public override async Task RunAsync()
    {
        Stage1.Running();
        await Task.Delay(500);
        Stage1.ReportProgress(0.5);
        await Task.Delay(500);
        Stage1.ReportProgress(1.0);
        Stage1.Complete();

        Stage2.Running();
        await Task.Delay(500);
        Stage2.ReportProgress(0.5);
        await Task.Delay(500);
        Stage2.ReportProgress(1.0);
        Stage2.Complete();
    }
}

public class TestJob2 : Job
{
    public override double Weighting => 1.5;

    public readonly Stage Stage1 = new() { Name = "Stage 3", Weighting = 1.0 };
    public readonly Stage Stage2 = new() { Name = "Stage 4", Weighting = 0.5 };

    public override async Task RunAsync()
    {
        Stage1.Running();
        await Task.Delay(1000);
        Stage1.ReportProgress(0.5);
        await Task.Delay(1000);
        Stage1.ReportProgress(1.0);
        Stage1.Complete();

        Stage2.Running();
        await Task.Delay(1000);
        Stage2.ReportProgress(0.5);
        await Task.Delay(1000);
        Stage2.ReportProgress(1.0);
        Stage2.Complete();
    }
}

public class JobServiceTest
{
    private static void PrintSummary(JobService js)
    {
        TestContext.Progress.WriteLine($"JobService Progress:\t{js.Progress * 100:0.00}%");
        TestContext.Progress.WriteLine("Jobs:");
        foreach (var j in js.Jobs)
        {
            TestContext.Progress.WriteLine($"\t{j.Name}:\t{j.Progress * 100:0.00}%\tIsCompleted: {j.IsCompleted}");
            foreach (var s in j.Stages)
            {
                TestContext.Progress.WriteLine($"\t\t{s.Name}:\t{s.Progress * 100:0.00}%\tStatus: {s.Status:G}");
            }
        }

        TestContext.Progress.WriteLine();
    }

    public async Task MonitorJobService(Func<JobService, Task> callback)
    {
        var js = new JobService();

        var cts = new CancellationTokenSource();
        var watcherTask = Task.Run(async () =>
        {
            try
            {
                var token = cts.Token;
                while (!token.IsCancellationRequested)
                {
                    await Task.Delay(250, token);
                    PrintSummary(js);
                }
            }
            catch (TaskCanceledException) {}
        }, cts.Token);

        await callback(js);

        await Task.Delay(500);

        await cts.CancelAsync();
        await watcherTask;
    }

    [Test]
    public async Task JobServiceInParallelTest()
    {
        await MonitorJobService(async js =>
        {
            var task1 = js
                .Submit(new TestJob1())
                .RunInNewTask();
            var task2 = js
                .Submit(new TestJob2())
                .RunInNewTask();
            await Task.WhenAll(task1, task2);
        });
    }

    [Test]
    public async Task JobServiceInSingleThreadTest()
    {
        await MonitorJobService(async js =>
        {
            var j1 = js.Submit(new TestJob1());
            var j2 = js.Submit(new TestJob2());
            await j1.RunAsync();
            await j2.RunAsync();
        });
    }

    [Test]
    public async Task JobServiceProgressChangedEventTest()
    {
        var lockObj = new object();
        var js = new JobService();
        js.ProgressChanged += (_, _) =>
        {
            lock (lockObj)
            {
                PrintSummary(js);
            }
        };

        var task1 = js.Submit(new TestJob1())
            .RunInNewTask();
        var task2 = js.Submit(new TestJob2())
            .RunInNewTask();
        await Task.WhenAll(task1, task2);
    }
}
