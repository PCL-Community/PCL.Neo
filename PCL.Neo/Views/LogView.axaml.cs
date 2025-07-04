using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using PCL.Neo.ViewModels;
using System;
using System.Collections.Specialized;
using System.Threading.Tasks;

namespace PCL.Neo.Views;

public partial class LogView : UserControl
{
    private ScrollViewer? _logScrollViewer;

    public LogView()
    {
        InitializeComponent();

        Loaded += LogView_Loaded;
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
        _logScrollViewer = this.FindControl<ScrollViewer>("LogScrollViewer");
    }

    private void LogView_Loaded(object? sender, EventArgs e)
    {
        if (DataContext is LogViewModel viewModel && _logScrollViewer != null)
        {
            // 监听过滤条件变化，更新滚动位置
            viewModel.PropertyChanged += (s, args) =>
            {
                if ((args.PropertyName == nameof(LogViewModel.FilterText) ||
                     args.PropertyName == nameof(LogViewModel.ShowErrorOnly)) &&
                    viewModel.IsAutoScroll)
                {
                    Task.Delay(100).ContinueWith(_ =>
                    {
                        ScrollToBottom();
                    });
                }
            };

            // 监听日志条目变化，更新滚动位置
            if (viewModel.LogEntries != null)
            {
                ((INotifyCollectionChanged)viewModel.LogEntries).CollectionChanged += (s, args) =>
                {
                    if (viewModel.IsAutoScroll && args.NewItems?.Count > 0)
                    {
                        ScrollToBottom();
                    }
                };
            }

            // 初始滚动到底部
            ScrollToBottom();
        }
    }

    private void ScrollToBottom()
    {
        if (_logScrollViewer != null)
        {
            _logScrollViewer.ScrollToEnd();
        }
    }
}
