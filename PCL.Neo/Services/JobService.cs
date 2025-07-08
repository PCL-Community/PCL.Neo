using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace PCL.Neo.Services;

public abstract class Job
{
    public enum StageStatus
    {
        Pending,
        Running,
        Completed,
        Canceled
    }

    public class Stage
    {
        public Stage()
        {
            ProgressHandler ??= new Progress<double>(x => Progress = x);
        }

        public string Name { get; init; } = "N/A";
        public double Weighting { get; init; } = 1.0;
        public double Progress { get; set; }
        public StageStatus Status { get; set; } = StageStatus.Pending;

        public Progress<double> ProgressHandler { get; init; }

        public void ReportProgress(double progress)
        {
            ((IProgress<double>)ProgressHandler).Report(progress);
        }

        public void Pending()
        {
            Status = StageStatus.Pending;
        }

        public void Running()
        {
            Status = StageStatus.Running;
        }

        public void Complete()
        {
            Status = StageStatus.Completed;
        }

        public void Canceled()
        {
            Status = StageStatus.Canceled;
        }
    }

    public Job()
    {
        Stages ??=
            GetType()
                .GetFields()
                .Where(x => x.FieldType.IsAssignableTo(typeof(Stage)))
                .Select(x => (Stage)x.GetValue(this)!)
                .ToArray();
        foreach (var s in Stages)
        {
            s.ProgressHandler.ProgressChanged += (_, _) => ProgressChanged?.Invoke(this, Progress);
        }
    }

    public event EventHandler<double>? ProgressChanged;

    public Stage[] Stages { get; }

    public double Progress
    {
        get
        {
            var wSum = Stages.Select(x => x.Weighting).Sum();
            return Stages.Select(x => x.Progress * (x.Weighting / wSum)).Sum();
        }
    }

    public bool IsCompleted => Stages.All(x => x.Status == StageStatus.Completed);

    public virtual string Name => GetType().Name;

    public virtual double Weighting => 1.0;

    public CancellationTokenSource CancellationTokenSource { get; } = new();

    public CancellationToken CancellationToken => CancellationTokenSource.Token;

    public abstract Task RunAsync();

    public void Run()
    {
        RunAsync().Wait(CancellationToken);
    }

    public Task RunInNewTask()
    {
        return Task.Run(RunAsync, CancellationToken);
    }

    public void Cancel()
    {
        CancellationTokenSource.Cancel();
    }
}

public class JobService
{
    public List<Job> Jobs { get; } = [];

    public EventHandler<double>? ProgressChanged;

    public double Progress
    {
        get
        {
            var wSum = Jobs.Select(x => x.Weighting).Sum();
            return Jobs.Select(x => x.Progress * (x.Weighting / wSum)).Sum();
        }
    }

    public T Submit<T>(T job) where T : Job
    {
        Jobs.Add(job);
        job.ProgressChanged += (_, _) => ProgressChanged?.Invoke(this, Progress);
        return job;
    }

    public void Remove<T>(T job) where T : Job
    {
        Jobs.Remove(job);
    }

    public void Clear()
    {
        Jobs.Clear();
    }
}
