using PCL.Neo.Services;
using System;

namespace PCL.Neo.ViewModels.Job;

[SubViewModel(typeof(JobViewModel))]
public class JobSubViewModel : ViewModelBase
{
    public JobService JobService { get; }

    public JobSubViewModel()
    {
        throw new NotImplementedException();
    }

    public JobSubViewModel(JobService jobService)
    {
        this.JobService = jobService;
    }
}