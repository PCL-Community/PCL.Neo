using PCL.Neo.Services;
using PCL.Neo.ViewModels.Job;
using System;
using System.Timers;

namespace PCL.Neo.ViewModels;

[MainViewModel(typeof(JobSubViewModel))]
public class JobViewModel : ViewModelBase
{
    private JobService JobService { get; }

    public JobViewModel()
    {
        throw new NotImplementedException();
    }

    public JobViewModel(JobService jobService)
    {
        this.JobService = jobService;
    }

    public double Progress => JobService.Progress * 100;
}