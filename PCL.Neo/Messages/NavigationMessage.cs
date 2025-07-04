using PCL.Neo.Services;
using PCL.Neo.ViewModels;
using System;

namespace PCL.Neo.Messages;

public class NavigationMessage(
    ViewModelBase? oldMainViewModel,
    ViewModelBase? newMainViewModel,
    ViewModelBase? oldSubViewModel,
    ViewModelBase? newSubViewModel,
    NavigationType navigationType)
{
    public static class Channels
    {
        public static readonly Guid Navigating = Guid.NewGuid();
        public static readonly Guid Navigated = Guid.NewGuid();
    }

    public bool IsMainViewModelChanged => oldMainViewModel != newMainViewModel;
    public ViewModelBase? OldMainViewModel => oldMainViewModel;
    public ViewModelBase? NewMainViewModel => newMainViewModel;
    public bool IsSubViewModelChanged => oldSubViewModel != newSubViewModel;
    public ViewModelBase? OldSubViewModel => oldSubViewModel;
    public ViewModelBase? NewSubViewModel => newSubViewModel;
    public NavigationType NavigationType => navigationType;
}
