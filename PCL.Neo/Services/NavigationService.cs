using Microsoft.Extensions.DependencyInjection;
using PCL.Neo.ViewModels;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace PCL.Neo.Services;

public enum NavigationType
{
    Forward,
    Backward
}

public class NavigationEventArgs : EventArgs
{
    public ViewModelBase? OldViewModel { get; }
    public ViewModelBase? NewViewModel { get; }
    public NavigationType NavigationType { get; }

    public NavigationEventArgs(ViewModelBase? oldViewModel, ViewModelBase? newViewModel, NavigationType navigationType)
    {
        OldViewModel = oldViewModel;
        NewViewModel = newViewModel;
        NavigationType = navigationType;
    }
}

public class NavigationService
{
    public IServiceProvider ServiceProvider { get; init; }

    public event Action<ViewModelBase?>? CurrentViewModelChanged;
    public event Action<ViewModelBase?>? CurrentSubViewModelChanged;
    public event Action<NavigationEventArgs>? Navigating;

    // 导航历史记录
    private readonly Stack<(Type ViewModelType, Type? SubViewModelType)> _navigationHistory = new();
    // 最大历史记录数量
    private const int MaxHistoryCount = 30;

    private ViewModelBase? _currentViewModel;
    public ViewModelBase? CurrentViewModel
    {
        get => _currentViewModel;
        protected set
        {
            if (value == _currentViewModel)
                return;
                
            var oldViewModel = _currentViewModel;
            _currentViewModel = value;
            CurrentViewModelChanged?.Invoke(value);
        }
    }

    private ViewModelBase? _currentSubViewModel;
    public ViewModelBase? CurrentSubViewModel
    {
        get => _currentSubViewModel;
        protected set
        {
            if (value == _currentSubViewModel)
                return;
            
            var oldSubViewModel = _currentSubViewModel;
            _currentSubViewModel = value;
            CurrentSubViewModelChanged?.Invoke(value);
        }
    }

    public bool CanGoBack => _navigationHistory.Count > 0;

    public NavigationService(IServiceProvider serviceProvider)
    {
        ServiceProvider = serviceProvider;
    }

    public virtual T Goto<T>() where T : ViewModelBase
    {
        var oldViewModel = CurrentViewModel;
        var oldSubViewModel = CurrentSubViewModel;
        
        // 保存当前状态到历史记录
        if (CurrentViewModel != null)
        {
            _navigationHistory.Push((CurrentViewModel.GetType(), CurrentSubViewModel?.GetType()));
            
            // 限制历史记录数量
            if (_navigationHistory.Count > MaxHistoryCount)
            {
                var tempStack = new Stack<(Type, Type?)>();
                var count = 0;
                
                while (_navigationHistory.Count > 0 && count < MaxHistoryCount)
                {
                    tempStack.Push(_navigationHistory.Pop());
                    count++;
                }
                
                _navigationHistory.Clear();
                while (tempStack.Count > 0)
                {
                    _navigationHistory.Push(tempStack.Pop());
                }
            }
        }

        T targetVm;
        
        if (typeof(T).GetCustomAttribute<DefaultSubViewModelAttribute>() is { } dsvm)
        {
            var vm = CurrentViewModel as T;
            if (vm?.GetType() != typeof(T))
            {
                vm = ServiceProvider.GetRequiredService<T>();
                CurrentViewModel = vm;
            }
            if (CurrentSubViewModel?.GetType() != dsvm.SubViewModel)
                CurrentSubViewModel = ServiceProvider.GetRequiredService(dsvm.SubViewModel) as ViewModelBase;
            targetVm = vm;
        }
        else if (typeof(T).GetCustomAttribute<SubViewModelOfAttribute>() is { } svmo)
        {
            var subVm = CurrentSubViewModel as T;
            if (CurrentViewModel?.GetType() != svmo.ParentViewModel)
                CurrentViewModel = ServiceProvider.GetRequiredService(svmo.ParentViewModel) as ViewModelBase;
            if (subVm?.GetType() != typeof(T))
            {
                subVm = ServiceProvider.GetRequiredService<T>();
                CurrentSubViewModel = subVm;
            }
            targetVm = subVm;
        }
        else
        {
            targetVm = CurrentViewModel?.GetType() != typeof(T) || CurrentSubViewModel?.GetType() != typeof(T)
                ? ServiceProvider.GetRequiredService<T>()
                : (T)CurrentViewModel;
            if (CurrentViewModel?.GetType() != typeof(T))
                CurrentViewModel = targetVm;
            if (CurrentSubViewModel?.GetType() != typeof(T))
                CurrentSubViewModel = targetVm;
        }
        
        // 触发导航事件
        Navigating?.Invoke(new NavigationEventArgs(oldViewModel, CurrentViewModel, NavigationType.Forward));
        
        return targetVm;
    }
    
    /// <summary>
    /// 返回上一个导航状态
    /// </summary>
    /// <returns>是否成功返回</returns>
    public bool GoBack()
    {
        if (!CanGoBack)
            return false;
            
        var oldViewModel = CurrentViewModel;
        var oldSubViewModel = CurrentSubViewModel;
        
        var (viewModelType, subViewModelType) = _navigationHistory.Pop();
        
        // 恢复主视图
        if (viewModelType != null)
        {
            CurrentViewModel = ServiceProvider.GetRequiredService(viewModelType) as ViewModelBase;
        }
        
        // 恢复子视图
        if (subViewModelType != null)
        {
            CurrentSubViewModel = ServiceProvider.GetRequiredService(subViewModelType) as ViewModelBase;
        }
        else
        {
            CurrentSubViewModel = null;
        }
        
        // 触发导航事件
        Navigating?.Invoke(new NavigationEventArgs(oldViewModel, CurrentViewModel, NavigationType.Backward));
        
        return true;
    }
    
    /// <summary>
    /// 清空导航历史
    /// </summary>
    public void ClearHistory()
    {
        _navigationHistory.Clear();
    }
}