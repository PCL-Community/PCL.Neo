using PCL.Neo.Services;
using PCL.Neo.ViewModels.Job;
using System;

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
        JobService = jobService;
    }

    public double Progress => JobService.Progress * 100;
}
